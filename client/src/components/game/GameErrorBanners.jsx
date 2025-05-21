import React from 'react';
import { ErrorIcon } from './GameIcons';

const GameErrorBanners = ({ localError, gameContextError, setLocalError, clearGameContextError }) => {
    return (
        <>
            {localError && (
                <div className="game-error-banner card">
                    <p><ErrorIcon /> {localError}</p>
                    <button onClick={() => setLocalError(null)} className="close-error-button" aria-label="Закрыть ошибку">&times;</button>
                </div>
            )}
            {gameContextError && (
                <div className="game-error-banner card">
                    <p><ErrorIcon /> Ошибка игры: {gameContextError}</p>
                    {typeof clearGameContextError === 'function' &&
                        <button onClick={clearGameContextError} className="close-error-button" aria-label="Закрыть ошибку">&times;</button>
                    }
                </div>
            )}
        </>
    );
};

export default GameErrorBanners; 