import React from 'react';

const GameBoard = ({ 
    isMyBoard, 
    boardData, 
    currentGame, 
    user, 
    playerIsCreator, 
    isGameInProgress, 
    isGameFinished, 
    isMyTurn, 
    handleCellClick 
}) => {
    const boardTitle = isMyBoard ? user?.username : (playerIsCreator ? currentGame.joinerName : currentGame.creatorName);
    const boardWrapperClass = isMyBoard ? "my-board-wrapper" : "opponent-board-wrapper";

    if (!boardData && currentGame) {
        if (isGameInProgress || currentGame.state === 1) {
            return (
                <div className={`board-placeholder card ${boardWrapperClass}`}>
                    <div className="loading-spinner-small"></div>
                    {isGameInProgress ? 'Загрузка доски...' : 'Подготовка доски...'}
                </div>
            );
        }
    }

    if (!currentGame || !boardData) {
        return (
            <div className={`board-placeholder card ${boardWrapperClass}`}>
                <div className="loading-spinner-small"></div>
                Ожидание данных {boardTitle ? `для ${boardTitle}` : ''}...
            </div>
        );
    }

    const renderCell = (row, col) => {
        let cellClass = 'game-cell';
        let cellState = null;

        if (boardData && boardData[row] !== undefined && boardData[row][col] !== undefined) {
            cellState = boardData[row][col];
        }

        if (isMyBoard) {
            if (cellState === 1) {
                cellClass += ' ship';
            }
            const opponentShotOnMyCell = currentGame?.opponentShots?.find(s => s.row === row && s.col === col);
            if (opponentShotOnMyCell) {
                cellClass += opponentShotOnMyCell.isHit ? ' hit' : ' miss';
            }
        } else {
            if (isMyTurn && isGameInProgress && !isGameFinished) {
                const myShotOnCell = currentGame?.myShots?.find(s => s.row === row && s.col === col);
                if (!myShotOnCell) {
                    cellClass += ' clickable';
                }
            }
            const myShotOnOpponentCell = currentGame?.myShots?.find(s => s.row === row && s.col === col);
            if (myShotOnOpponentCell) {
                cellClass += myShotOnOpponentCell.isHit ? ' hit' : ' miss';
            }
        }

        return (
            <div
                key={`${row}-${col}`}
                className={cellClass}
                onClick={() => !isMyBoard && isGameInProgress && handleCellClick(row, col)}
            />
        );
    };

    return (
        <div className={`game-board card ${!isMyBoard && isMyTurn && isGameInProgress && !isGameFinished ? 'clickable-board' : ''}`}>
            {Array(10 * 10).fill(0).map((_, idx) => {
                const row = Math.floor(idx / 10);
                const col = idx % 10;
                return renderCell(row, col);
            })}
        </div>
    );
};

export default GameBoard; 