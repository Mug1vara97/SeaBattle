import React, { useState } from 'react'
import Home from './components/Home'
import Game from './components/Game'
import Auth from './components/Auth'
import './App.css'

export default function App() {
  const [gameState, setGameState] = useState({
    isInGame: false,
    gameId: null
  });

  const [username, setUsername] = useState(null);

  const handleCreateGame = () => {
    setGameState({
      isInGame: true,
      gameId: null
    });
  };

  const handleJoinGame = (gameId) => {
    setGameState({
      isInGame: true,
      gameId
    });
  };

  const handleBackToMenu = () => {
    setGameState({
      isInGame: false,
      gameId: null
    });
  };

  if (!username) {
    return <Auth onAuth={setUsername} />;
  }

  return (
    <div className="app">
      {!gameState.isInGame ? (
        <Home 
          onCreateGame={handleCreateGame}
          onJoinGame={handleJoinGame}
          username={username}
        />
      ) : (
        <Game 
          playerName={username}
          gameId={gameState.gameId}
          onBack={handleBackToMenu}
        />
      )}
    </div>
  )
}
