import React from 'react';
import { TurnIcon, ReadyIcon } from './GameIcons';

const GameStatusBanners = ({ 
    currentGame, 
    playerIsCreator, 
    playerIsJoiner, 
    handleReadyClick 
}) => {
    return (
        <>
            {currentGame.state === 0 && (
                <div className="game-status-banner card info">
                    <p><TurnIcon /> Ожидание присоединения противника...</p>
                </div>
            )}

            {currentGame.state === 1 && !(currentGame.creatorBoardSet && currentGame.joinerBoardSet) && (
                <div className="game-status-banner card info">
                    <p><TurnIcon /> Игроки расставляют корабли. {(playerIsCreator && !currentGame.creatorBoardSet) || (playerIsJoiner && !currentGame.joinerBoardSet) ? <strong>Вам нужно расставить корабли.</strong> : "Ожидание завершения расстановки..."}</p>
                </div>
            )}

            {currentGame.state === 1 && currentGame.creatorBoardSet && currentGame.joinerBoardSet &&
                ((playerIsCreator && !currentGame.creatorReady) || (playerIsJoiner && !currentGame.joinerReady)) &&
                <div className="game-status-banner card actions">
                    <p>Все корабли расставлены. Готовы начать?</p>
                    <button className="ready-button primary-button" onClick={handleReadyClick}>
                        <ReadyIcon /> Готов к бою!
                    </button>
                </div>
            }

            {currentGame.state === 1 && currentGame.creatorBoardSet && currentGame.joinerBoardSet &&
                ((playerIsCreator && currentGame.creatorReady && !currentGame.joinerReady) ||
                (playerIsJoiner && currentGame.joinerReady && !currentGame.creatorReady)) &&
                <div className="game-status-banner card info">
                    <p><TurnIcon /> Вы готовы. Ожидание готовности противника...</p>
                </div>
            }
        </>
    );
};

export default GameStatusBanners; 