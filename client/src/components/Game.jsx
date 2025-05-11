import { useState, useEffect, useRef } from 'react';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import GameBoard from './GameBoard';
import './Game.css';

const SHIP_SIZES = [4, 3, 3, 2, 2, 2, 1, 1, 1, 1];

function Game({ playerName, gameId: initialGameId, onBack }) {
  const [gameState, setGameState] = useState({
    gameId: initialGameId,
    isMyTurn: false,
    gameStarted: false,
    winner: null,
    isPlacingShips: true,
    currentShipIndex: 0,
    ships: Array(10).fill().map(() => Array(10).fill(0)),
    opponentBoard: Array(10).fill().map(() => Array(10).fill(0)),
    opponentName: '',
    message: '',
    isConnected: false,
    isGameInitialized: false,
    isPlayer1: false
  });

  const connectionRef = useRef(null);
  const isGameReady = useRef(false);
  const gameIdRef = useRef(initialGameId);

  useEffect(() => {
    const newConnection = new HubConnectionBuilder()
      .withUrl('http://localhost:5183/gamehub')
      .withAutomaticReconnect([0, 2000, 5000, 10000, 20000])
      .configureLogging(LogLevel.Information)
      .build();

    connectionRef.current = newConnection;

    return () => {
      if (newConnection) {
        newConnection.stop();
      }
    };
  }, []);

  useEffect(() => {
    if (!connectionRef.current) return;

    const setupConnection = async () => {
      try {
        if (connectionRef.current.state === 'Disconnected') {
          await connectionRef.current.start();
          console.log('Connected to SignalR');
          setGameState(prev => ({ ...prev, isConnected: true }));

          if (gameIdRef.current) {
            console.log('Joining existing game:', gameIdRef.current);
            await connectionRef.current.invoke('JoinGame', playerName, gameIdRef.current);
            isGameReady.current = true;
            setGameState(prev => ({ 
              ...prev, 
              isGameInitialized: true,
              isPlayer1: false,
              message: 'Подключено к существующей игре. Разместите корабли.'
            }));
          } else {
            console.log('Creating new game');
            setGameState(prev => ({
              ...prev,
              message: 'Создание новой игры...'
            }));
            const newGameId = await connectionRef.current.invoke('CreateGame', playerName);
            if (!newGameId) {
              console.error('Failed to create game: received null gameId');
              setGameState(prev => ({
                ...prev,
                message: 'Ошибка при создании игры. Попробуйте еще раз.'
              }));
              return;
            }
            console.log('Created new game with ID:', newGameId);
            gameIdRef.current = newGameId;
            setGameState(prev => ({ 
              ...prev, 
              gameId: newGameId,
              isGameInitialized: true,
              isPlayer1: true,
              message: `Игра создана. ID игры: ${newGameId}. Ожидание второго игрока...`
            }));
            isGameReady.current = true;
          }
        }
      } catch (err) {
        console.error('SignalR Connection Error: ', err);
        setGameState(prev => ({ 
          ...prev, 
          isConnected: false,
          message: 'Ошибка подключения к серверу. Попытка переподключения...'
        }));
        setTimeout(setupConnection, 2000);
      }
    };

    const setupHandlers = () => {
      if (!connectionRef.current) return;

      connectionRef.current.on('ReceiveMessage', (message) => {
        console.log('Received message from server:', message);
        setGameState(prev => ({
          ...prev,
          message
        }));
      });

      connectionRef.current.on('Error', (errorMessage) => {
        console.error('Received error from server:', errorMessage);
        setGameState(prev => ({
          ...prev,
          message: errorMessage
        }));
      });

      connectionRef.current.on('ReceiveShot', (row, col, result) => {
        console.log('Received shot at opponent:', { row, col, result });
        setGameState(prev => {
          const newOpponentBoard = [...prev.opponentBoard];
          newOpponentBoard[row][col] = result === 'hit' ? 2 : 3;
          return { ...prev, opponentBoard: newOpponentBoard };
        });
      });

      connectionRef.current.on('ReceiveHit', (row, col, result) => {
        console.log('Received hit on own ships:', { row, col, result });
        setGameState(prev => {
          const newShips = [...prev.ships];
          newShips[row][col] = result === 'hit' ? 2 : 3;
          return { ...prev, ships: newShips };
        });
      });

      connectionRef.current.on('GameStarted', () => {
        setGameState(prev => ({ 
          ...prev, 
          gameStarted: true, 
          isPlacingShips: false,
          opponentBoard: Array(10).fill().map(() => Array(10).fill(0))
        }));
      });

      connectionRef.current.on('TurnChanged', (isPlayer1Turn) => {
        console.log('Turn changed:', isPlayer1Turn);
        setGameState(prev => ({ 
          ...prev, 
          isMyTurn: isPlayer1Turn === prev.isPlayer1 
        }));
      });

      connectionRef.current.on('GameCreated', (newGameId) => {
        if (!newGameId) {
          console.error('Received null gameId in GameCreated event');
          setGameState(prev => ({
            ...prev,
            message: 'Ошибка при создании игры. Попробуйте еще раз.'
          }));
          return;
        }
        console.log('Game created with ID:', newGameId);
        gameIdRef.current = newGameId;
        setGameState(prev => ({ 
          ...prev, 
          gameId: newGameId,
          isGameInitialized: true,
          message: `Игра создана. ID игры: ${newGameId}. Ожидание второго игрока...` 
        }));
        isGameReady.current = true;
      });

      connectionRef.current.on('PlayerJoined', (opponentName) => {
        console.log('Player joined:', opponentName);
        setGameState(prev => ({ 
          ...prev, 
          opponentName, 
          message: 'Второй игрок присоединился. Разместите корабли.' 
        }));
      });

      connectionRef.current.on('PlayerReady', (playerName) => {
        console.log('Player ready:', playerName);
        setGameState(prev => ({
          ...prev,
          message: `${playerName} разместил корабли. ${prev.isPlacingShips ? 'Разместите ваши корабли.' : 'Ожидание размещения кораблей противником.'}`
        }));
      });

      connectionRef.current.on('InvalidShipsPlacement', () => {
        console.log('Invalid ships placement');
        setGameState(prev => ({
          ...prev,
          message: 'Неправильное размещение кораблей. Попробуйте еще раз.',
          ships: Array(10).fill().map(() => Array(10).fill(0)),
          currentShipIndex: 0
        }));
      });

      connectionRef.current.on('GameOver', (winner) => {
        console.log('Game over, winner:', winner);
        setGameState(prev => ({ ...prev, winner, message: `Игра окончена. Победитель: ${winner}` }));
      });

      connectionRef.current.onreconnecting((error) => {
        console.log('Reconnecting to SignalR...', error);
        setGameState(prev => ({
          ...prev,
          isConnected: false,
          message: 'Переподключение к серверу...'
        }));
      });

      connectionRef.current.onreconnected((connectionId) => {
        console.log('Reconnected to SignalR with connection ID:', connectionId);
        setGameState(prev => ({
          ...prev,
          isConnected: true,
          message: 'Соединение восстановлено'
        }));
        if (gameIdRef.current) {
          connectionRef.current.invoke('JoinGame', playerName, gameIdRef.current);
        }
      });
    };

    setupConnection();
    setupHandlers();

    return () => {
      if (connectionRef.current) {
        connectionRef.current.off('ReceiveMessage');
        connectionRef.current.off('Error');
        connectionRef.current.off('ReceiveShot');
        connectionRef.current.off('ReceiveHit');
        connectionRef.current.off('GameStarted');
        connectionRef.current.off('TurnChanged');
        connectionRef.current.off('GameCreated');
        connectionRef.current.off('PlayerJoined');
        connectionRef.current.off('PlayerReady');
        connectionRef.current.off('InvalidShipsPlacement');
        connectionRef.current.off('GameOver');
        connectionRef.current.off('reconnecting');
        connectionRef.current.off('reconnected');
      }
    };
  }, [playerName]);

  const handleCellClick = async (row, col) => {
    if (!gameState.gameStarted || !gameState.isMyTurn || !connectionRef.current || !gameState.isConnected) return;

    try {
      if (!gameIdRef.current) {
        console.error('Game ID is missing');
        return;
      }
      await connectionRef.current.invoke('MakeShot', gameIdRef.current, row, col);
    } catch (err) {
      console.error('Error making shot: ', err);
    }
  };

  const handlePlaceShip = async (row, col) => {
    if (
      !gameState.isPlacingShips ||
      gameState.currentShipIndex >= SHIP_SIZES.length ||
      !gameState.isConnected ||
      !gameState.isGameInitialized
    ) {
      return;
    }

    const shipSize = SHIP_SIZES[gameState.currentShipIndex];
    const newShips = [...gameState.ships];

    if (canPlaceShip(newShips, row, col, shipSize)) {
      for (let i = 0; i < shipSize; i++) {
        newShips[row][col + i] = 1;
      }

      const isLastShip = gameState.currentShipIndex + 1 >= SHIP_SIZES.length;

      setGameState(prev => ({
        ...prev,
        ships: newShips,
        currentShipIndex: prev.currentShipIndex + 1
      }));

      if (isLastShip) {
        const currentGameId = gameState.gameId || gameIdRef.current;
        
        if (!currentGameId) {
          setGameState(prev => ({
            ...prev,
            message: 'Ошибка: ID игры отсутствует. Попробуйте еще раз.',
            ships: Array(10).fill().map(() => Array(10).fill(0)),
            currentShipIndex: 0
          }));
          return;
        }

        try {
          const flatShips = newShips.flat();
          await connectionRef.current.invoke('PlaceShips', currentGameId, flatShips);
          setGameState(prev => ({
            ...prev,
            message: 'Корабли размещены. Ожидание противника...'
          }));
        } catch (error) {
          console.error('Error placing ships:', error);
          setGameState(prev => ({
            ...prev,
            message: 'Ошибка при размещении кораблей. Попробуйте еще раз.',
            ships: Array(10).fill().map(() => Array(10).fill(0)),
            currentShipIndex: 0
          }));
        }
      }
    }
  };

  const canPlaceShip = (board, row, col, size) => {
    if (col + size > 10) return false;

    for (let i = -1; i <= 1; i++) {
      for (let j = -1; j <= size; j++) {
        const newRow = row + i;
        const newCol = col + j;
        if (newRow >= 0 && newRow < 10 && newCol >= 0 && newCol < 10) {
          if (board[newRow][newCol] === 1) return false;
        }
      }
    }

    return true;
  };

  return (
    <div className="game-container">
      <div className="game-header">
        <h1>Морской бой</h1>
        <button className="back-button" onClick={onBack}>Вернуться в меню</button>
      </div>
      <div className="game-status">
        {!gameState.isConnected && (
          <p className="error">Отсутствует соединение с сервером. Попытка переподключения...</p>
        )}
        {!gameState.isGameInitialized && (
          <p className="message">Инициализация игры...</p>
        )}
        {gameState.message && <p className="message">{gameState.message}</p>}
        {gameState.gameStarted && (
          <p>{gameState.isMyTurn ? 'Ваш ход' : 'Ход противника'}</p>
        )}
        {gameState.winner && <p className="winner">Победитель: {gameState.winner}</p>}
      </div>
      <div className="boards-container">
        <div className="board-section">
          <h2>Ваше поле</h2>
          <GameBoard
            isOpponent={false}
            onCellClick={handlePlaceShip}
            board={gameState.ships}
          />
        </div>
        <div className="board-section">
          <h2>Поле противника</h2>
          <GameBoard
            isOpponent={true}
            onCellClick={handleCellClick}
            board={gameState.opponentBoard}
          />
        </div>
      </div>
    </div>
  );
}

export default Game; 