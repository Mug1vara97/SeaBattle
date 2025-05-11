import React from 'react';
import './GameBoard.css';

function GameBoard({ isOpponent, onCellClick, board }) {
  return (
    <div className="game-board">
      {board.map((row, rowIndex) => (
        <div key={rowIndex} className="board-row">
          {row.map((cell, colIndex) => (
            <div
              key={`${rowIndex}-${colIndex}`}
              className={`board-cell ${getCellClass(cell, isOpponent)}`}
              onClick={() => onCellClick(rowIndex, colIndex)}
            />
          ))}
        </div>
      ))}
    </div>
  );
}

function getCellClass(cell, isOpponent) {
  if (isOpponent) {
    switch (cell) {
      case 2: return 'hit';
      case 3: return 'miss';
      default: return '';
    }
  } else {
    switch (cell) {
      case 1: return 'ship';
      case 2: return 'hit';
      case 3: return 'miss';
      default: return '';
    }
  }
}

export default GameBoard; 