namespace SeaBattle.Tests;
using Xunit;
using Moq;
using SeaBattle.Data;
using SeaBattle.Services;
using SeaBattle.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System;
using System.Threading;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;

internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new TestAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object Execute(Expression expression)
    {
        return _inner.Execute(expression);
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
    {
        var resultType = typeof(TResult).GetGenericArguments()[0];
        var executionResult = typeof(IQueryProvider)
                              .GetMethods()
                              .First(method => method.Name == nameof(IQueryProvider.Execute) && method.IsGenericMethod)
                              .MakeGenericMethod(resultType)
                              .Invoke(this._inner, new object[] { expression });

        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
                                     .MakeGenericMethod(resultType)
                                     .Invoke(null, new[] { executionResult });
    }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable)
        : base(enumerable) { }

    public TestAsyncEnumerable(Expression expression)
        : base(expression) { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }

    public T Current => _inner.Current;

    public ValueTask<bool> MoveNextAsync()
    {
        return new ValueTask<bool>(_inner.MoveNext());
    }
}

public static class DbSetMocking
{
    public static Mock<DbSet<T>> CreateMockSet<T>(List<T> listData) where T : class
    {
        var queryableData = listData.AsQueryable(); 
        var mockSet = new Mock<DbSet<T>>();
        
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryableData.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryableData.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryableData.GetEnumerator());

        mockSet.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(queryableData.GetEnumerator()));

        mockSet.As<IQueryable<T>>().Setup(m => m.Provider)
            .Returns((System.Linq.IQueryProvider)new TestAsyncQueryProvider<T>(queryableData.Provider)); 
        
        mockSet.Setup(m => m.Add(It.IsAny<T>()))
               .Callback((T entity) => listData.Add(entity))
               .Returns((T entity) => default(EntityEntry<T>));

        mockSet.Setup(m => m.AddAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
               .Callback((T entity, CancellationToken ct) => listData.Add(entity))
               .ReturnsAsync((T entity, CancellationToken ct) => default(EntityEntry<T>));

        mockSet.Setup(m => m.AddRange(It.IsAny<IEnumerable<T>>()))
               .Callback((IEnumerable<T> entities) => listData.AddRange(entities));

        mockSet.Setup(m => m.AddRangeAsync(It.IsAny<IEnumerable<T>>(), It.IsAny<CancellationToken>()))
               .Callback((IEnumerable<T> entities, CancellationToken ct) => listData.AddRange(entities))
               .Returns(Task.CompletedTask);

        return mockSet;
    }
}

public class GameServiceTests
{
    private readonly Mock<ApplicationDbContext> _mockContext;
    private readonly GameService _gameService;
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<GameService>> _mockLogger;

    public GameServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())    
            .Options;
        _mockContext = new Mock<ApplicationDbContext>(options);
        _mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<GameService>>();

        _gameService = new GameService(_mockLogger.Object, _mockContext.Object);
    }

    [Fact]
    public async Task CreateGame_ShouldCreateAndReturnNewGame()
    {
        var playerName = "Player1";
        var isOpenLobby = true;

        var game = await _gameService.CreateGame(playerName, isOpenLobby);

        Assert.NotNull(game);
        Assert.Equal(playerName, game.CreatorName);
        Assert.False(game.CreatorBoardSet);
        Assert.False(game.JoinerBoardSet);
        Assert.Null(game.CreatorBoard);
        Assert.Null(game.JoinerBoard);
        Assert.NotEmpty(game.Id);
    }

    [Fact]
    public async Task PlaceShipsAsync_ShouldSetPlayerBoardAndFlag()
    {
        var creatorName = "CreatorPlayer";
        var isOpenLobby = true;
        var game = await _gameService.CreateGame(creatorName, isOpenLobby);
        Assert.NotNull(game);

        var clientBoard = new CellState[10, 10];
        var updatedGame = await _gameService.PlaceShipsAsync(game.Id, creatorName, clientBoard);

        Assert.NotNull(updatedGame);
        Assert.True(updatedGame.CreatorBoardSet);
        Assert.NotNull(updatedGame.CreatorBoard);
        Assert.Equal(clientBoard.Length, updatedGame.CreatorBoard.Length);
    }

    [Fact]
    public async Task AddGameToHistoryAsync_ShouldAddHistoryAndUpdateRankings()
    {
        var winnerName = "Winner";
        var loserName = "Loser";
        
        var gameToLog = new Game 
        { 
            Id = Guid.NewGuid().ToString(), 
            CreatorName = winnerName, 
            JoinerName = loserName,  
            State = GameState.Finished, 
            Winner = winnerName 
        };

        var mockGameHistories = new List<GameHistory>();
        var mockPlayerRankings = new List<PlayerRanking>();

        var mockGameHistoriesDbSet = DbSetMocking.CreateMockSet(mockGameHistories); 
        var mockPlayerRankingsDbSet = DbSetMocking.CreateMockSet(mockPlayerRankings);

        _mockContext.Setup(c => c.GameHistories).Returns(mockGameHistoriesDbSet.Object);
        _mockContext.Setup(c => c.PlayerRankings).Returns(mockPlayerRankingsDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1); 

        const int expectedInitialRating = 1000;
        const int expectedRatingChangeOnWin = 15;
        const int expectedRatingChangeOnLoss = 10;

        await _gameService.AddGameToHistory(gameToLog, winnerName, loserName);

        Assert.Equal(2, mockGameHistories.Count);

        var winnerHistoryEntry = mockGameHistories.FirstOrDefault(gh => gh.PlayerUsername == winnerName);
        var loserHistoryEntry = mockGameHistories.FirstOrDefault(gh => gh.PlayerUsername == loserName);

        Assert.NotNull(winnerHistoryEntry);
        Assert.Equal(gameToLog.Id, winnerHistoryEntry.GameId); 
        Assert.Equal(loserName, winnerHistoryEntry.OpponentUsername);
        Assert.Equal("Победа", winnerHistoryEntry.Result);

        Assert.NotNull(loserHistoryEntry);
        Assert.Equal(gameToLog.Id, loserHistoryEntry.GameId); 
        Assert.Equal(winnerName, loserHistoryEntry.OpponentUsername);
        Assert.Equal("Поражение", loserHistoryEntry.Result);

        var winnerRanking = mockPlayerRankings.FirstOrDefault(pr => pr.PlayerUsername == winnerName);
        var loserRanking = mockPlayerRankings.FirstOrDefault(pr => pr.PlayerUsername == loserName);

        Assert.NotNull(winnerRanking);
        Assert.Equal(expectedInitialRating + expectedRatingChangeOnWin, winnerRanking.Rating);
        Assert.Equal(1, winnerRanking.Wins);
        Assert.Equal(0, winnerRanking.Losses);
        Assert.Equal(1, winnerRanking.TotalGames);

        Assert.NotNull(loserRanking);
        Assert.Equal(expectedInitialRating - expectedRatingChangeOnLoss, loserRanking.Rating);
        Assert.Equal(0, loserRanking.Wins);
        Assert.Equal(1, loserRanking.Losses);
        Assert.Equal(1, loserRanking.TotalGames);

        mockGameHistoriesDbSet.Verify(m => m.Add(It.IsAny<GameHistory>()), Times.Exactly(2)); 
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task GetPlayerGameHistoryAsync_ShouldReturnCorrectHistory()
    {
        var playerName = "TestPlayer";
        var otherPlayer1 = "Opponent1";
        var otherPlayer2 = "Opponent2";
        var thirdPlayer = "ThirdPlayer";

        var gameHistoriesListData = new List<GameHistory>
        {
            new GameHistory { Id = Guid.NewGuid(), PlayerUsername = playerName, OpponentUsername = otherPlayer1, Result = "Победа", GameId = "game1", GameFinishedAt = DateTime.UtcNow.AddDays(-1) },
            new GameHistory { Id = Guid.NewGuid(), PlayerUsername = otherPlayer1, OpponentUsername = playerName, Result = "Поражение", GameId = "game1", GameFinishedAt = DateTime.UtcNow.AddDays(-1) },

            new GameHistory { Id = Guid.NewGuid(), PlayerUsername = playerName, OpponentUsername = otherPlayer2, Result = "Поражение", GameId = "game2", GameFinishedAt = DateTime.UtcNow.AddDays(-2) },
            new GameHistory { Id = Guid.NewGuid(), PlayerUsername = otherPlayer2, OpponentUsername = playerName, Result = "Победа", GameId = "game2", GameFinishedAt = DateTime.UtcNow.AddDays(-2) },

            new GameHistory { Id = Guid.NewGuid(), PlayerUsername = playerName, OpponentUsername = thirdPlayer, Result = "Победа", GameId = "game3", GameFinishedAt = DateTime.UtcNow }, // Самая новая игра для playerName
            new GameHistory { Id = Guid.NewGuid(), PlayerUsername = thirdPlayer, OpponentUsername = playerName, Result = "Поражение", GameId = "game3", GameFinishedAt = DateTime.UtcNow },

            new GameHistory { Id = Guid.NewGuid(), PlayerUsername = thirdPlayer, OpponentUsername = otherPlayer1, Result = "Победа", GameId = "game4", GameFinishedAt = DateTime.UtcNow.AddDays(-3) },
            new GameHistory { Id = Guid.NewGuid(), PlayerUsername = otherPlayer1, OpponentUsername = thirdPlayer, Result = "Поражение", GameId = "game4", GameFinishedAt = DateTime.UtcNow.AddDays(-3) }
        };

        var mockGameHistoriesDbSet = DbSetMocking.CreateMockSet(gameHistoriesListData);
        _mockContext.Setup(c => c.GameHistories).Returns(mockGameHistoriesDbSet.Object);

        var result = await _gameService.GetPlayerGameHistory(playerName, 5);

        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.True(result.All(gh => gh.PlayerUsername == playerName));
        

        Assert.Equal("game3", result[0].GameId);
        Assert.Equal(thirdPlayer, result[0].OpponentUsername); 
        Assert.Equal("Победа", result[0].Result);              

        Assert.Equal("game1", result[1].GameId);
        Assert.Equal(otherPlayer1, result[1].OpponentUsername); 
        Assert.Equal("Победа", result[1].Result);              

        Assert.Equal("game2", result[2].GameId);
        Assert.Equal(otherPlayer2, result[2].OpponentUsername); 
        Assert.Equal("Поражение", result[2].Result);           
    }

    [Fact]
    public async Task GetLeaderboardAsync_ShouldReturnTopPlayersSortedByRating()
    {
        var playerRankingsListData = new List<PlayerRanking>
        {
            new PlayerRanking { PlayerUsername = "PlayerA", Rating = 1200, Wins = 10, Losses = 2, TotalGames = 12 },
            new PlayerRanking { PlayerUsername = "PlayerB", Rating = 1500, Wins = 15, Losses = 0, TotalGames = 15 },
            new PlayerRanking { PlayerUsername = "PlayerC", Rating = 1200, Wins = 8, Losses = 1, TotalGames = 9 },
            new PlayerRanking { PlayerUsername = "PlayerD", Rating = 1000, Wins = 5, Losses = 5, TotalGames = 10 },
            new PlayerRanking { PlayerUsername = "PlayerE", Rating = 1600, Wins = 20, Losses = 3, TotalGames = 23 }
        };

        var mockPlayerRankingsDbSet = DbSetMocking.CreateMockSet(playerRankingsListData);
        _mockContext.Setup(c => c.PlayerRankings).Returns(mockPlayerRankingsDbSet.Object);

        var leaderboard = await _gameService.GetLeaderboardAsync(3);

        Assert.NotNull(leaderboard);
        Assert.Equal(3, leaderboard.Count);
        Assert.Equal("PlayerE", leaderboard[0].PlayerUsername);
        Assert.Equal("PlayerB", leaderboard[1].PlayerUsername);
        Assert.Equal("PlayerC", leaderboard[2].PlayerUsername); 
    }
}
