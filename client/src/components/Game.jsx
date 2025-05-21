import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { useGame } from '../contexts/GameContext';
import ShipPlacement from './ShipPlacement';
import GameHeader from './game/GameHeader';
import GameBoard from './game/GameBoard';
import GameErrorBanners from './game/GameErrorBanners';
import GameStatusBanners from './game/GameStatusBanners';
import { BackToLobbyIcon, StatusIcon, ErrorIcon } from './game/GameIcons';
import './Game.css';

const Game = () => {
    const { gameId } = useParams();
    const navigate = useNavigate();
    const { user } = useAuth();
    const {
        currentGame,
        isMyTurn,
        setReady,
        makeShot,
        getGameState,
        leaveGame,
        submitBoardPlacement,
        error: gameContextError,
        clearError: clearGameContextError
    } = useGame();

    const [localError, setLocalError] = useState(null);

    const playerIsCreator = currentGame?.creatorName === user?.username;
    const playerIsJoiner = currentGame?.joinerName === user?.username;

    const myBoardData = playerIsCreator ? currentGame?.creatorBoard : currentGame?.joinerBoard;
    const opponentBoardData = playerIsCreator ? currentGame?.joinerBoard : currentGame?.creatorBoard;

    const showPlacementUI = currentGame && user && (
        (playerIsCreator && !currentGame.creatorBoardSet) ||
        (playerIsJoiner && !currentGame.joinerBoardSet)
    );

    const isGameInProgress = currentGame?.state === 2;
    const isGameFinished = currentGame?.state === 3;

    useEffect(() => {
        if (!user) {
            navigate('/login');
            return;
        }
        if (!gameId) {
            navigate('/');
            return;
        }

        const fetchGameState = async () => {
            try {
                setLocalError(null);
                if (typeof clearGameContextError === 'function') {
                    clearGameContextError();
                } else {
                    console.warn('clearGameContextError is not a function during fetchGameState initial call');
                }
                await getGameState(gameId, user.username);
            } catch (err) {
                console.error('Error fetching game state:', err);
                setLocalError('Ошибка при получении состояния игры: ' + (err.message || 'Неизвестная ошибка'));
            }
        };

        fetchGameState();
    }, [gameId, user, getGameState, navigate, clearGameContextError]);

    const handlePlacementConfirmed = async (placedBoard) => {
        if (!gameId || !user?.username) {
            setLocalError('Ошибка: ID игры или имя пользователя отсутствуют для подтверждения расстановки.');
            return;
        }
        try {
            setLocalError(null);
            if (typeof clearGameContextError === 'function') {
                clearGameContextError();
            }
            const success = await submitBoardPlacement(gameId, user.username, placedBoard);
            if (success) {
                console.log('Расстановка успешно отправлена, ожидаем обновления состояния игры...');
            } else {
                if (!gameContextError) {
                    setLocalError('Не удалось подтвердить расстановку. Попробуйте снова.');
                }
            }
        } catch (err) {
            console.error('Error confirming placement:', err);
            setLocalError('Ошибка при подтверждении расстановки: ' + (err.message || 'Неизвестная ошибка'));
        }
    };

    const handleReadyClick = async () => {
        try {
            setLocalError(null);
            if (typeof clearGameContextError === 'function') {
                clearGameContextError();
            }
            await setReady(gameId, user.username);
        } catch (err) {
            console.error('Error setting ready:', err);
            setLocalError('Ошибка при установке статуса готовности: ' + (err.message || 'Проверьте консоль'));
        }
    };

    const handleCellClick = async (row, col) => {
        if (!isMyTurn || isGameFinished || !isGameInProgress) return;
        try {
            setLocalError(null);
            if (typeof clearGameContextError === 'function') {
                clearGameContextError();
            }
            const position = { row, col };
            await makeShot(gameId, user.username, position);
        } catch (err) {
            console.error('Error making shot:', err);
            if (!gameContextError) {
                setLocalError('Ошибка при совершении выстрела: ' + (err.message || 'Неизвестная ошибка'));
            }
        }
    };

    const handleReturnToLobby = () => {
        leaveGame();
        navigate('/home');
    };

    if (!user || !gameId) {
        return <div className="page-loading-container"><div className="loading-spinner"></div>Загрузка...</div>;
    }

    if (showPlacementUI) {
        return (
            <div className="game-page-container ship-placement-active">
                <ShipPlacement
                    gameId={gameId}
                    playerName={user.username}
                    onPlacementConfirmed={handlePlacementConfirmed}
                />
            </div>
        );
    }

    if (!currentGame && localError) {
        return (
            <div className="game-page-container error-page">
                <div className="game-error-banner card">
                    <h3><ErrorIcon /> Не удалось загрузить данные игры</h3>
                    <p>{localError}</p>
                    <button onClick={handleReturnToLobby} className="lobby-button secondary-button">
                        <BackToLobbyIcon /> Вернуться в лобби
                    </button>
                </div>
            </div>
        );
    }

    if (!currentGame && !isGameFinished) {
        return <div className="page-loading-container"><div className="loading-spinner"></div>Загрузка состояния игры...</div>;
    }

    if (!currentGame) {
        return (
            <div className="game-page-container error-page">
                <div className="game-error-banner card">
                    <h3><ErrorIcon /> Критическая ошибка</h3>
                    <p>Не удалось получить данные об игре. Возможно, она была удалена или не существует.</p>
                    <button onClick={handleReturnToLobby} className="lobby-button secondary-button">
                        <BackToLobbyIcon /> Вернуться в лобби
                    </button>
                </div>
            </div>
        );
    }

    if (isGameFinished) {
        return (
            <div className="game-finished-overlay">
                <div className="modal-content card">
                    <h2><StatusIcon /> Игра Завершена!</h2>
                    <p className={`game-result-message ${currentGame.winner === user.username ? 'winner' : 'loser'}`}>
                        {currentGame.winner === user.username ? '🎉 Вы победили! 🎉' : '😔 Вы проиграли. 😔'}
                    </p>
                    <p>Победитель: <strong>{currentGame.winner || "Не определен"}</strong></p>
                    <button onClick={handleReturnToLobby} className="lobby-button primary-button">
                        <BackToLobbyIcon /> Вернуться в лобби
                    </button>
                </div>
            </div>
        );
    }

    return (
        <div className="game-page-container">
            <GameHeader
                gameId={gameId}
                user={user}
                currentGame={currentGame}
                playerIsCreator={playerIsCreator}
                isGameInProgress={isGameInProgress}
                isMyTurn={isMyTurn}
            />

            <GameErrorBanners
                localError={localError}
                gameContextError={gameContextError}
                setLocalError={setLocalError}
                clearGameContextError={clearGameContextError}
            />

            <GameStatusBanners
                currentGame={currentGame}
                playerIsCreator={playerIsCreator}
                playerIsJoiner={playerIsJoiner}
                handleReadyClick={handleReadyClick}
            />

            {(!showPlacementUI && !isGameFinished && currentGame.state !== 0 && !(currentGame.state === 1 && !(currentGame.creatorBoardSet && currentGame.joinerBoardSet))) && (
                <div className="game-boards-container">
                    <div className="board-section my-board-section">
                        <h2>Ваше поле ({user?.username})</h2>
                        <GameBoard
                            isMyBoard={true}
                            boardData={myBoardData}
                            currentGame={currentGame}
                            user={user}
                            playerIsCreator={playerIsCreator}
                            isGameInProgress={isGameInProgress}
                            isGameFinished={isGameFinished}
                            isMyTurn={isMyTurn}
                            handleCellClick={handleCellClick}
                        />
                    </div>
                    <div className="board-section opponent-board-section">
                        <h2>Поле противника ({playerIsCreator ? (currentGame.joinerName || 'Ожидание...') : (currentGame.creatorName || 'Ожидание...')})</h2>
                        <GameBoard
                            isMyBoard={false}
                            boardData={opponentBoardData}
                            currentGame={currentGame}
                            user={user}
                            playerIsCreator={playerIsCreator}
                            isGameInProgress={isGameInProgress}
                            isGameFinished={isGameFinished}
                            isMyTurn={isMyTurn}
                            handleCellClick={handleCellClick}
                        />
                    </div>
                </div>
            )}

            <div className="game-footer">
                <button onClick={handleReturnToLobby} className="lobby-button secondary-button">
                    <BackToLobbyIcon /> Вернуться в лобби
                </button>
            </div>
        </div>
    );
};

export default Game;