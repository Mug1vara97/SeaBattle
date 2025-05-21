import React from 'react';
import { GameIdIcon, PlayerIcon, OpponentIcon, StatusIcon, TurnIcon } from './GameIcons';
import { getGameStateText } from './GameUtils';

const GameHeader = ({ gameId, user, currentGame, playerIsCreator, isGameInProgress, isMyTurn }) => {
    return (
        <div className="game-main-header card">
            <div className="game-title-section">
                <h1>Морской бой</h1>
                <span className="game-id-display"><GameIdIcon /> #{gameId}</span>
            </div>
            <div className="game-info-bar">
                <span><PlayerIcon /> Вы: <strong>{user.username}</strong> {playerIsCreator ? '(Создатель)' : '(Присоед.)'}</span>
                <span><OpponentIcon /> Противник: <strong>{playerIsCreator ? (currentGame.joinerName || 'Ожидание...') : (currentGame.creatorName || 'Ожидание...')}</strong></span>
                <span><StatusIcon /> Статус: <strong>{getGameStateText(currentGame.state)}</strong></span>
                {isGameInProgress && (
                    <span className={`turn-indicator ${isMyTurn ? 'my-turn' : 'opponent-turn'}`}>
                        <TurnIcon /> {isMyTurn ? <strong>Ваш ход</strong> : 'Ход противника'}
                    </span>
                )}
            </div>
        </div>
    );
};

export default GameHeader; 