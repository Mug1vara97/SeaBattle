import React, { createContext, useContext, useState, useEffect, useCallback, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useSignalR } from './SignalRContext';
import { useAuth } from './AuthContext';

const GameContext = createContext();

export const useGame = () => {
    const context = useContext(GameContext);
    if (!context) {
        throw new Error('useGame must be used within a GameProvider');
    }
    return context;
};

export const GameProvider = ({ children }) => {
    const navigate = useNavigate();
    const { connection, isConnected, reconnect } = useSignalR();
    const { user } = useAuth();
    const [lobbies, setLobbies] = useState([]);
    const [currentGame, setCurrentGame] = useState(null);
    const [error, setError] = useState(null);
    const [gameHistory, setGameHistory] = useState([]);
    const [isLoadingHistory, setIsLoadingHistory] = useState(false);

    const setupHandlers = useCallback(() => {
        if (!connection) return;

        connection.on('LobbiesUpdated', (updatedLobbies) => {
            console.log('Lobbies updated:', updatedLobbies);
            setLobbies(updatedLobbies);
        });

        connection.on('GameCreated', (gameId) => {
            console.log('Game created:', gameId);
            navigate(`/game/${gameId}`);
        });

        connection.on('GameUpdated', (personalizedGameData) => {
            console.log("GameContext: GameUpdated received", personalizedGameData);
            setCurrentGame(prev => ({
                ...prev,
                ...personalizedGameData,
                myShots: [...(personalizedGameData.myShots || [])],
                opponentShots: [...(personalizedGameData.opponentShots || [])] 
            }));
        });

        connection.on('ShotResult', (shotResultPayload) => {
            console.log("GameContext: ShotResult received for processing", shotResultPayload);
            if (shotResultPayload && shotResultPayload.game) {
                const gameFromServer = shotResultPayload.game;

                if (shotResultPayload.myShots !== undefined && shotResultPayload.opponentShots !== undefined && shotResultPayload.isCreator !== undefined) {
                    console.log("GameContext: Using top-level myShots/opponentShots from ShotResult payload");
                    setCurrentGame(prev => ({
                        ...prev,
                        ...gameFromServer,
                        myShots: [...(shotResultPayload.myShots || [])],
                        opponentShots: [...(shotResultPayload.opponentShots || [])],
                        isCreator: shotResultPayload.isCreator,
                        currentTurn: gameFromServer.currentTurn,
                        state: gameFromServer.state,
                        winner: gameFromServer.winner,
                    }));
                } else {
                    console.log("GameContext: Deriving myShots/opponentShots from game data in ShotResult payload (with workaround)");
                    
                    const amICurrentlyCreator = gameFromServer.creatorName === user?.username;
                    const myServerShotListKey = amICurrentlyCreator ? 'creatorShots' : 'joinerShots';
                    const myCurrentShotsFromServer = gameFromServer[myServerShotListKey];

                    setCurrentGame(prev => {
                        if (!prev) {
                           console.warn("GameContext: prev state was null in ShotResult, initializing opponentShots as empty.");
                           return {
                                ...gameFromServer,
                                myShots: [...(myCurrentShotsFromServer || [])],
                                opponentShots: [], 
                                isCreator: amICurrentlyCreator,
                           };
                        }

                        return {
                            ...prev, 
                            ...gameFromServer,
                            myShots: [...(myCurrentShotsFromServer || [])], 
                            opponentShots: [...(prev.opponentShots || [])], 
                            isCreator: amICurrentlyCreator,
                        };
                    });
                }
            }
        });

        connection.on('GameState', (personalizedGameData) => {
            console.log("GameContext: GameState received", personalizedGameData);
            setCurrentGame(prev => ({
                ...prev,
                ...personalizedGameData,
                myShots: [...(personalizedGameData.myShots || [])],
                opponentShots: [...(personalizedGameData.opponentShots || [])]
            }));
        });

        connection.on('SecondPlayerJoined', (personalizedGameData) => {
            console.log('Second player joined:', personalizedGameData);
            setCurrentGame(prev => ({
                ...prev,
                ...personalizedGameData,
                myShots: [...(personalizedGameData.myShots || [])],
                opponentShots: [...(personalizedGameData.opponentShots || [])]
            }));
        });

        connection.on('GameStarted', (personalizedGameData) => {
            console.log('Game started:', personalizedGameData);
            setCurrentGame(prev => ({
                ...prev,
                ...personalizedGameData,
                myShots: [...(personalizedGameData.myShots || [])],
                opponentShots: [...(personalizedGameData.opponentShots || [])]
            }));
        });

        connection.on('TurnChanged', (currentTurn) => {
            console.log('Turn changed:', currentTurn);
            setCurrentGame(prev => {
                if (!prev) return null;
                return { ...prev, currentTurn };
            });
        });

        connection.on('GameEnded', (game) => {
            console.log('Game ended:', game);
            setCurrentGame(game);
        });

        connection.on('ReceiveGameHistory', (history) => {
            console.log("GameContext: ReceiveGameHistory received", history);
            setGameHistory(history || []);
            setIsLoadingHistory(false);
        });

        return () => {
            connection.off('LobbiesUpdated');
            connection.off('GameCreated');
            connection.off('GameUpdated');
            connection.off('ShotResult');
            connection.off('GameState');
            connection.off('SecondPlayerJoined');
            connection.off('GameStarted');
            connection.off('TurnChanged');
            connection.off('GameEnded');
            connection.off('ReceiveGameHistory');
        };
    }, [connection, navigate]);

    useEffect(() => {
        const cleanup = setupHandlers();
        return cleanup;
    }, [setupHandlers]);

    const getOpenLobbies = useCallback(async () => {
        if (!connection || !isConnected) {
            console.log('Нет соединения с SignalR, пропуск getOpenLobbies');
            return;
        }
        try {
            setError(null);
            console.log('Getting open lobbies...');
            await connection.invoke('GetOpenLobbies');
        } catch (error) {
            console.error('Error getting lobbies:', error);
            setError('Ошибка при получении списка игр');
        }
    }, [connection, isConnected, reconnect]);

    const createGame = useCallback(async (creatorName, isOpenLobby) => {
        if (!connection || !isConnected) {
            console.log('Нет соединения с SignalR, пропуск createGame');
            return;
        }
        try {
            setError(null);
            console.log('Creating game...', { creatorName, isOpenLobby });
            await connection.invoke('CreateGame', creatorName, isOpenLobby);
        } catch (error) {
            console.error('Error creating game:', error);
            setError('Ошибка при создании игры');
            throw error;
        }
    }, [connection, isConnected, reconnect]);

    const joinGame = useCallback(async (gameId, playerName) => {
        if (!connection || !isConnected) {
            console.log('Нет соединения с SignalR, пропуск joinGame');
            return;
        }
        try {
            setError(null);
            console.log('Joining game...', { gameId, playerName });
            await connection.invoke('JoinGame', gameId, playerName);
            navigate(`/game/${gameId}`);
        } catch (error) {
            console.error('Error joining game:', error);
            setError('Ошибка при присоединении к игре');
            throw error;
        }
    }, [connection, isConnected, navigate, reconnect]);

    const setReady = useCallback(async (gameId, playerName) => {
        if (!connection || !isConnected) {
            console.log('Нет соединения с SignalR, пропуск setReady');
            return;
        }
        try {
            setError(null);
            console.log('Setting ready...', { gameId, playerName });
            await connection.invoke('SetReady', gameId, playerName);
        } catch (error) {
            console.error('Error setting ready:', error);
            setError('Ошибка при установке статуса готовности');
            throw error;
        }
    }, [connection, isConnected, reconnect]);

    const makeShot = useCallback(async (gameId, playerName, position) => {
        if (!connection || !isConnected) {
            console.log('Нет соединения с SignalR, пропуск makeShot');
            return;
        }
        try {
            setError(null);
            console.log('Making shot...', { gameId, playerName, position });
            const result = await connection.invoke('MakeShot', gameId, playerName, position);
            console.log('Shot result from server:', result);
            return result;
        } catch (error) {
            console.error('Error making shot:', error);
            setError('Ошибка при выполнении выстрела');
            throw error;
        }
    }, [connection, isConnected, reconnect]);

    const getGameState = useCallback(async (gameId, playerName) => {
        if (!connection || !isConnected) {
            console.log('Нет соединения с SignalR, пропуск getGameState');
            return;
        }
        try {
            setError(null);
            console.log('Getting game state...', { gameId });
            await connection.invoke('GetGameState', gameId, playerName);
        } catch (error) {
            console.error('Error getting game state:', error);
            setError('Ошибка при получении состояния игры');
            throw error;
        }
    }, [connection, isConnected, reconnect]);

    const leaveGame = useCallback(() => {
        setCurrentGame(null);
        console.log("GameContext: Leaving game, resetting currentGame.");
    }, [setCurrentGame]);

    const isCreator = useMemo(() => currentGame?.isCreator, [currentGame?.isCreator]);

    const isMyTurn = useMemo(() => {
        if (!currentGame || !user) return false;
        return currentGame.currentTurn === user.username;
    }, [currentGame?.currentTurn, user?.username]);

    const isGameStarted = useMemo(() => currentGame?.state === 2, [currentGame?.state]);
    const isGameEnded = useMemo(() => currentGame?.state === 3, [currentGame?.state]);
    const winner = useMemo(() => currentGame?.winner, [currentGame?.winner]);

    const myBoard = useMemo(() => {
        if (!currentGame) return undefined;
        return currentGame.isCreator ? currentGame.creatorBoard : currentGame.joinerBoard;
    }, [currentGame?.isCreator, currentGame?.creatorBoard, currentGame?.joinerBoard]);

    const opponentBoard = useMemo(() => {
        if (!currentGame) return undefined;
        return currentGame.isCreator ? currentGame.joinerBoard : currentGame.creatorBoard;
    }, [currentGame?.isCreator, currentGame?.joinerBoard, currentGame?.creatorBoard]);

    const myShots = useMemo(() => currentGame?.myShots || [], [currentGame?.myShots]);
    const opponentShots = useMemo(() => currentGame?.opponentShots || [], [currentGame?.opponentShots]);

    const fetchGameHistory = useCallback(async (count = 5) => {
        if (!connection || !isConnected) {
            console.log('GameContext: No connection to SignalR, skipping fetchGameHistory');
            return;
        }
        if (!user) {
            console.log('GameContext: No user, skipping fetchGameHistory');
            return;
        }
        try {
            console.log(`GameContext: Fetching game history for ${user.username}, count: ${count}`);
            setIsLoadingHistory(true);
            await connection.invoke('GetMyGameHistory', user.username, count);
        } catch (error) {
            console.error('GameContext: Error fetching game history:', error);
            setIsLoadingHistory(false);
        }
    }, [connection, isConnected, user]);

    const submitBoardPlacement = useCallback(async (gameId, playerName, board) => {
        if (!connection || !isConnected) {
            console.log('GameContext: No connection to SignalR, skipping submitBoardPlacement');
            setError('Нет соединения с сервером для отправки расстановки.');
            return false;
        }
        if (!user || user.username !== playerName) {
            console.error('GameContext: User mismatch or no user, skipping submitBoardPlacement', {currentUser: user?.username, providedPlayerName: playerName});
            setError('Ошибка пользователя при отправке расстановки.');
            return false;
        }
        try {
            setError(null);
            console.log('GameContext: Submitting board placement...', { gameId, playerName, board });
            await connection.invoke('SubmitBoardPlacement', gameId, playerName, board);
            return true;
        } catch (err) {
            console.error('GameContext: Error submitting board placement:', err);
            setError('Ошибка при отправке расстановки кораблей: ' + (err.message || 'Проверьте консоль для деталей'));
            if (err.message && (err.message.includes("disconnected") || err.message.includes("transport error"))) {
                reconnect();
            }
            return false;
        }
    }, [connection, isConnected, user, reconnect]);

    const value = {
        lobbies,
        currentGame,
        error,
        isCreator,
        isMyTurn,
        isGameStarted,
        isGameEnded,
        winner,
        myBoard,
        opponentBoard,
        myShots,
        opponentShots,
        getOpenLobbies,
        createGame,
        joinGame,
        setReady,
        makeShot,
        getGameState,
        leaveGame,
        gameHistory,
        isLoadingHistory,
        fetchGameHistory,
        submitBoardPlacement,
        setCurrentGame
    };

    return (
        <GameContext.Provider value={value}>
            {children}
        </GameContext.Provider>
    );
}; 