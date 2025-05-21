import React, { createContext, useContext, useEffect, useState, useCallback } from 'react';
import * as signalR from '@microsoft/signalr';

const SignalRContext = createContext();

export const useSignalR = () => {
    const context = useContext(SignalRContext);
    if (!context) {
        throw new Error('useSignalR must be used within a SignalRProvider');
    }
    return context;
};

export const SignalRProvider = ({ children }) => {
    const [connection, setConnection] = useState(null);
    const [isConnected, setIsConnected] = useState(false);
    const [error, setError] = useState(null);

    const startConnection = useCallback(async () => {
        try {
            const hubConnection = new signalR.HubConnectionBuilder()
                .withUrl('http://localhost:5183/gamehub')
                .withAutomaticReconnect({
                    nextRetryDelayInMilliseconds: retryContext => {
                        if (retryContext.previousRetryCount === 0) {
                            return 0;
                        } else if (retryContext.previousRetryCount < 3) {
                            return 2000;
                        } else {
                            return 5000;
                        }
                    }
                })
                .configureLogging(signalR.LogLevel.Information)
                .build();

            hubConnection.onclose(error => {
                console.log('Connection closed:', error);
                setIsConnected(false);
                if (error) {
                    setError('Соединение закрыто с ошибкой: ' + error.message);
                }
            });

            hubConnection.onreconnecting(error => {
                console.log('Reconnecting:', error);
                setIsConnected(false);
                setError('Переподключение к серверу...');
            });

            hubConnection.onreconnected(connectionId => {
                console.log('Reconnected:', connectionId);
                setIsConnected(true);
                setError(null);
            });

            await hubConnection.start();
            console.log('Connected to SignalR Hub');
            setConnection(hubConnection);
            setIsConnected(true);
            setError(null);
            return hubConnection;
        } catch (err) {
            console.error('Error starting SignalR connection:', err);
            setError('Ошибка подключения к серверу: ' + err.message);
            return null;
        }
    }, []);

    useEffect(() => {
        let hubConnection = null;

        const initializeConnection = async () => {
            hubConnection = await startConnection();
        };

        initializeConnection();

        return () => {
            if (hubConnection) {
                hubConnection.stop()
                    .then(() => console.log('SignalR connection stopped'))
                    .catch(err => console.error('Error stopping SignalR connection:', err));
            }
        };
    }, [startConnection]);

    const reconnect = useCallback(async () => {
        if (connection) {
            try {
                await connection.stop();
            } catch (err) {
                console.error('Error stopping connection:', err);
            }
        }
        return startConnection();
    }, [connection, startConnection]);

    const value = {
        connection,
        isConnected,
        error,
        reconnect
    };

    return (
        <SignalRContext.Provider value={value}>
            {children}
        </SignalRContext.Provider>
    );
}; 