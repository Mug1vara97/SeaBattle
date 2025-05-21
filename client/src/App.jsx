import React from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './contexts/AuthContext';
import { SignalRProvider } from './contexts/SignalRContext';
import { GameProvider } from './contexts/GameContext';
import Login from './components/Login'; 
import Register from './components/Register'; 
import Home from './components/Home';
import Game from './components/Game';

const App = () => {
    return (
        <AuthProvider>
            <SignalRProvider>
                <GameProvider>
                    <div className="app">
                        <Routes>
                            <Route path="/login" element={<Login />} />
                            <Route path="/register" element={<Register />} />
                            <Route path="/home" element={<Home />} />
                            <Route path="/game/:gameId" element={<Game />} />
                            <Route path="*" element={<Navigate to="/login" replace />} />
                        </Routes>
                    </div>
                </GameProvider>
            </SignalRProvider>
        </AuthProvider>
    );
};

export default App;