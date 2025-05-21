import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useGame } from '../contexts/GameContext';
import { useAuth } from '../contexts/AuthContext';
import './Home.css';


const CreateGameIcon = () => <span role="img" aria-label="create game">➕</span>;
const JoinByIdIcon = () => <span role="img" aria-label="join by id">🔗</span>;
const TropyIcon = () => <span role="img" aria-label="leaderboard">🏆</span>;
const HistoryIcon = () => <span role="img" aria-label="game history">📜</span>;
const OpenLobbyIcon = () => <span role="img" aria-label="open lobbies">🚪</span>;

const Home = () => {
    const navigate = useNavigate();
    const { user, authLoading, authError: contextAuthError, clearAuthError } = useAuth();
    const { 
        lobbies, 
        getOpenLobbies, 
        createGame, 
        joinGame, 
        error: gameContextError,
        clearError: clearGameContextError,
        gameHistory,
        isLoadingHistory,
        fetchGameHistory
    } = useGame();
    
    const [isProcessingCreate, setIsProcessingCreate] = useState(false);
    const [isJoiningGame, setIsJoiningGame] = useState(null);
    const [joinGameIdInput, setJoinGameIdInput] = useState('');
    const [lobbyError, setLobbyError] = useState(null);
    
    const [showCreateGameModal, setShowCreateGameModal] = useState(false);
    const [newGameLobbyType, setNewGameLobbyType] = useState(true);

    const [leaderboard, setLeaderboard] = useState([]);
    const [isLoadingLeaderboard, setIsLoadingLeaderboard] = useState(true);
    const [leaderboardError, setLeaderboardError] = useState(null);

    useEffect(() => {
        const fetchLeaderboardData = async () => {
            setIsLoadingLeaderboard(true);
            try {
                const response = await fetch('http://localhost:5183/api/game/leaderboard?count=10'); 
                if (!response.ok) {
                    const errorText = await response.text();
                    throw new Error(`HTTP error! status: ${response.status}, message: ${errorText}`);
                }
                const data = await response.json();
                setLeaderboard(data);
                setLeaderboardError(null);
            } catch (e) {
                console.error("Ошибка при загрузке таблицы лидеров в Лобби:", e);
                setLeaderboardError(`Не удалось загрузить таблицу лидеров: ${e.message}`);
                setLeaderboard([]);
            }
            setIsLoadingLeaderboard(false);
        };

        if (user && !authLoading) {
            fetchLeaderboardData();
        }
    }, [user, authLoading]);

    useEffect(() => {
        if (authLoading) {
            return;
        }
        if (!user) {
            navigate('/auth');
        } else {
            getOpenLobbies();
            fetchGameHistory(5);
        }
    }, [user, authLoading, navigate, getOpenLobbies, fetchGameHistory]);

    useEffect(() => {
        if (contextAuthError) {
            setLobbyError(`Ошибка аутентификации: ${contextAuthError}`);
        } else if (gameContextError) {
            setLobbyError(`Ошибка игры: ${gameContextError}`);
        }
    }, [contextAuthError, gameContextError]);

    useEffect(() => {
        return () => {
            if (clearAuthError) clearAuthError();
            if (clearGameContextError) clearGameContextError();
        };
    }, [clearAuthError, clearGameContextError]);

    const clearAllLobbyErrors = () => {
        setLobbyError(null);
        if (clearAuthError) clearAuthError();
        if (clearGameContextError) clearGameContextError();
    };

    const openCreateGameModal = () => {
        if (!user || authLoading) return;
        clearAllLobbyErrors();
        setNewGameLobbyType(true);
        setShowCreateGameModal(true);
    };

    const handleConfirmCreateGame = async () => {
        if (!user || authLoading) return;
        setIsProcessingCreate(true);
        clearAllLobbyErrors();
        try {
            await createGame(user.username, newGameLobbyType); 
            setShowCreateGameModal(false);
        } catch (err) {
            console.error("Lobby: Error confirming create game", err);
        } finally {
            setIsProcessingCreate(false);
        }
    };

    const handleJoinListedGame = async (gameIdToJoin) => {
        if (!user || authLoading || !gameIdToJoin) return;
        clearAllLobbyErrors();
        setIsJoiningGame(gameIdToJoin);
        try {
            await joinGame(gameIdToJoin, user.username);
        } catch (err) {
            console.error(`Lobby: Error joining game ${gameIdToJoin}`, err);
        } finally {
            setIsJoiningGame(null);
        }
    };

    const handleJoinById = async (e) => {
        e.preventDefault();
        if (!user || authLoading) {
            setLobbyError("Пожалуйста, войдите в систему или подождите завершения проверки.");
            return;
        }
        const idToJoin = joinGameIdInput.trim();
        if (!idToJoin) {
            setLobbyError("Пожалуйста, введите ID игры.");
            return;
        }
        clearAllLobbyErrors();
        setIsJoiningGame(idToJoin);
        try {
            await joinGame(idToJoin, user.username);
        } catch (err) {
            console.error(`Lobby: Error joining game by ID ${idToJoin}`, err);
        } finally {
            setIsJoiningGame(null);
        }
    };

    const formatDate = (dateString) => {
        if (!dateString) return 'Неизвестно';
        try {
            return new Date(dateString).toLocaleString('ru-RU', {
                day: '2-digit',
                month: '2-digit',
                year: 'numeric',
                hour: '2-digit',
                minute: '2-digit'
            });
        } catch (e) {
            console.warn("Invalid date string for formatDate:", dateString);
            return dateString;
        }
    };

    if (authLoading) {
        return (
            <div className="loading-full-page">
                <div className="spinner"></div>
                <p>Проверка аутентификации...</p>
            </div>
        );
    }
    
    if (!user) {
        return null;
    }

    return (
        <div className="lobby-page-container">
            <header className="lobby-main-header">
                <h1>Морской Бой: Лобби</h1>
                {user && <span className="welcome-message">Добро пожаловать, {user.username}!</span>}
            </header>

            {lobbyError && (
                <div className="lobby-error-banner">
                    <p>{lobbyError}</p>
                    <button onClick={clearAllLobbyErrors} className="close-error-button" aria-label="Закрыть ошибку">&times;</button>
                </div>
            )}

            <div className="lobby-main-content">
                <div className="lobby-left-column">
                    <section className="lobby-actions-section card">
                        <h2><CreateGameIcon /> Действия</h2>
                        <div className="action-buttons-group">
                    <button 
                        onClick={openCreateGameModal} 
                                disabled={isProcessingCreate || authLoading} 
                                className="lobby-button primary"
                    >
                                Создать Новую Игру
                    </button>
                            <form onSubmit={handleJoinById} className="join-by-id-form">
                        <input 
                            type="text" 
                            value={joinGameIdInput} 
                            onChange={(e) => setJoinGameIdInput(e.target.value)}
                                    placeholder="Введите ID для присоединения"
                            className="lobby-input"
                                    disabled={!!isJoiningGame || authLoading}
                        />
                        <button 
                                    type="submit" 
                                    disabled={!joinGameIdInput.trim() || !!isJoiningGame || authLoading} 
                                    className="lobby-button"
                        >
                                    <JoinByIdIcon /> Присоединиться по ID
                        </button>
                            </form>
                        </div>
                    </section>

                    <section className="open-lobbies-section card">
                        <h2><OpenLobbyIcon /> Открытые Игры</h2>
                        {lobbies && lobbies.length > 0 ? (
                            <ul className="lobbies-list-ul">
                                {lobbies.filter(lobby => lobby.isOpenLobby && lobby.creatorName !== user.username && !lobby.joinerName).map(lobby => (
                                    <li key={lobby.id} className="lobby-item-card">
                                        <div className="lobby-item-info">
                                            <p><strong>Создатель:</strong> {lobby.creatorName}</p>
                                            <p><small>ID Игры: {lobby.id}</small></p>
                                            <p><small>Создана: {formatDate(lobby.createdAt)}</small></p>
                    </div>
                                        <button 
                                            onClick={() => handleJoinListedGame(lobby.id)}
                                            disabled={isJoiningGame === lobby.id || authLoading}
                                            className="lobby-button join"
                                        >
                                            {isJoiningGame === lobby.id ? 'Вход...' : 'Присоединиться'}
                                        </button>
                                    </li>
                                ))}
                                {lobbies.filter(lobby => lobby.isOpenLobby && lobby.creatorName !== user.username && !lobby.joinerName).length === 0 &&
                                    <p className="no-items-message">Нет доступных открытых игр, созданных другими игроками. Попробуйте создать свою!</p>
                                }
                            </ul>
                        ) : (
                            <p className="no-items-message">Нет открытых игр. Создайте свою!</p>
                        )}
                    </section>
                </div>

                <div className="lobby-right-column">
                    <section className="leaderboard-section card">
                        <h2><TropyIcon /> Таблица Лидеров (Топ 10)</h2>
                        {isLoadingLeaderboard && <div className="loading-spinner-small">Загрузка лидеров...</div>}
                        {leaderboardError && <p className="error-text">{leaderboardError}</p>}
                        {!isLoadingLeaderboard && !leaderboardError && leaderboard.length > 0 && (
                            <table className="leaderboard-table">
                                <thead>
                                    <tr>
                                        <th>#</th>
                                        <th>Игрок</th>
                                        <th>Рейтинг</th>
                                        <th>Побед</th>
                                        <th>Поражений</th>
                                        <th>Всего игр</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {leaderboard.map((player, index) => (
                                        <tr key={player.playerUsername} className={player.playerUsername === user.username ? 'current-user-highlight' : ''}>
                                            <td>{index + 1}</td>
                                            <td>{player.playerUsername}</td>
                                            <td>{player.rating}</td>
                                            <td>{player.wins}</td>
                                            <td>{player.losses}</td>
                                            <td>{player.totalGames}</td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        )}
                        {!isLoadingLeaderboard && !leaderboardError && leaderboard.length === 0 && (
                            <p className="no-items-message">Таблица лидеров пуста.</p>
                        )}
            </section>

                    <section className="game-history-section card">
                        <h2><HistoryIcon /> Моя История Игр (Последние 5)</h2>
                        {isLoadingHistory && <div className="loading-spinner-small">Загрузка истории...</div>}
                        {gameContextError && !lobbyError && <p className="error-text">{gameContextError}</p>}
                        {!isLoadingHistory && !gameContextError && gameHistory.length > 0 && (
                            <ul className="history-list-ul">
                                {gameHistory.map(game => (
                                    <li key={game.id} className={`history-item-card ${game.result?.toLowerCase()}`}>
                                        <p><strong>Противник:</strong> {game.opponentUsername || 'Неизвестно'}</p>
                                        <p><strong>Результат:</strong> {game.result || 'N/A'}</p>
                                        <p><small>Завершена: {formatDate(game.gameFinishedAt)}</small></p>
                                    </li>
                                ))}
                            </ul>
                        )}
                        {!isLoadingHistory && !gameContextError && gameHistory.length === 0 && (
                            <p className="no-items-message">У вас пока нет истории игр.</p>
                        )}
                    </section>
                </div>
            </div>

            {showCreateGameModal && (
                <div className="modal-overlay" onClick={() => setShowCreateGameModal(false)}>
                    <div className="modal-content" onClick={e => e.stopPropagation()}>
                        <h2>Создать Новую Игру</h2>
                        <div className="modal-options">
                            <label>
                                <input 
                                    type="radio" 
                                    name="lobbyType" 
                                    value="public"
                                    checked={newGameLobbyType === true} 
                                    onChange={() => setNewGameLobbyType(true)} 
                                    disabled={isProcessingCreate}
                                />
                                Открытое лобби (любой сможет присоединиться)
                            </label>
                            <label>
                                <input 
                                    type="radio" 
                                    name="lobbyType" 
                                    value="private"
                                    checked={newGameLobbyType === false} 
                                    onChange={() => setNewGameLobbyType(false)} 
                                    disabled={isProcessingCreate}
                                />
                                Приватное лобби (присоединение по ID)
                            </label>
                        </div>
                        <div className="modal-actions">
                            <button 
                                onClick={handleConfirmCreateGame} 
                                disabled={isProcessingCreate}
                                className="lobby-button primary"
                            >
                                {isProcessingCreate ? 'Создание...' : 'Подтвердить и Создать'}
                            </button>
                            <button 
                                onClick={() => setShowCreateGameModal(false)} 
                                disabled={isProcessingCreate}
                                className="lobby-button secondary"
                            >
                                Отмена
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default Home; 