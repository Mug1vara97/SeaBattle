import React, { useState } from 'react';
import './Home.css';

const Home = ({ onCreateGame, onJoinGame, username }) => {
  const [gameId, setGameId] = useState('');
  const [error, setError] = useState('');

  const handleCreateGame = () => {
    onCreateGame();
  };

  const handleJoinGame = (e) => {
    e.preventDefault();
    if (!gameId.trim()) {
      setError('Введите ID игры');
      return;
    }
    onJoinGame(gameId);
  };

  return (
    <div className="home-container">
      <h1>Морской бой</h1>
      <p>Добро пожаловать, {username}!</p>
      <div className="game-options">
        <button onClick={handleCreateGame}>Создать игру</button>
        <div className="join-game">
          <h2>Присоединиться к игре</h2>
          {error && <div className="error">{error}</div>}
          <form onSubmit={handleJoinGame}>
            <input
              type="text"
              value={gameId}
              onChange={(e) => setGameId(e.target.value)}
              placeholder="Введите ID игры"
            />
            <button type="submit">Присоединиться</button>
          </form>
        </div>
      </div>
    </div>
  );
};

export default Home; 