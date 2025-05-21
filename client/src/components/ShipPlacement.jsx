import React, { useState, useCallback } from 'react';
import './ShipPlacement.css';

const ShipIcon = () => <span role="img" aria-label="ship">🚢</span>;
const RotateIcon = () => <span role="img" aria-label="rotate">🔄</span>;
const ConfirmIcon = () => <span role="img" aria-label="confirm">✔️</span>;
const ResetIcon = () => <span role="img" aria-label="reset">🗑️</span>;
const ErrorIcon = () => <span role="img" aria-label="error">⚠️</span>;
const CopyIcon = () => <span role="img" aria-label="copy">📋</span>;
const CheckIcon = () => <span role="img" aria-label="check">✓</span>;

const BOARD_SIZE = 10;
const initialShipsToPlace = [
    { name: 'Авианосец', id: 'carrier', size: 4, count: 1, placed: 0 },
    { name: 'Линкор', id: 'battleship', size: 3, count: 2, placed: 0 },
    { name: 'Крейсер', id: 'cruiser', size: 2, count: 3, placed: 0 },
    { name: 'Эсминец', id: 'destroyer', size: 1, count: 4, placed: 0 },
];

const CellStateEnum = {
    EMPTY: 0,
    SHIP: 1,
    // MISS: 2, 
    // HIT: 3,  
};
Object.freeze(CellStateEnum);

const ShipPlacement = ({ gameId, playerName, onPlacementConfirmed }) => {
    const [board, setBoard] = useState(Array(BOARD_SIZE).fill(null).map(() => Array(BOARD_SIZE).fill(CellStateEnum.EMPTY)));
    const [ships, setShips] = useState(JSON.parse(JSON.stringify(initialShipsToPlace)));
    const [selectedShip, setSelectedShip] = useState(ships.find(s => s.placed < s.count) || null);
    const [orientation, setOrientation] = useState('horizontal');
    const [error, setError] = useState('');
    const [isCopied, setIsCopied] = useState(false);

    const handleCopyGameId = async () => {
        try {
            await navigator.clipboard.writeText(gameId);
            setIsCopied(true);
            setTimeout(() => setIsCopied(false), 2000);
        } catch (err) {
            console.error('Ошибка при копировании ID игры:', err);
            setError('Не удалось скопировать ID игры');
        }
    };

    const canPlaceShip = useCallback((currentBoard, row, col, shipSize, shipOrientation) => {
        if (shipOrientation === 'horizontal') {
            if (col + shipSize > BOARD_SIZE) return false;
        } else { 
            if (row + shipSize > BOARD_SIZE) return false;
        }
        for (let i = 0; i < shipSize; i++) {
            const r = shipOrientation === 'horizontal' ? row : row + i;
            const c = shipOrientation === 'horizontal' ? col + i : col;
            for (let dr = -1; dr <= 1; dr++) {
                for (let dc = -1; dc <= 1; dc++) {
                    const nr = r + dr;
                    const nc = c + dc;
                    let isPartOfCurrentShipSegment = false;
                    if (shipOrientation === 'horizontal') {
                        if (dr === 0 && nc >= col && nc < col + shipSize && nr === row) isPartOfCurrentShipSegment = true;
                    } else { 
                        if (dc === 0 && nr >= row && nr < row + shipSize && nc === col) isPartOfCurrentShipSegment = true;
                    }
                    if (nr >= 0 && nr < BOARD_SIZE && nc >= 0 && nc < BOARD_SIZE) {
                        if (!isPartOfCurrentShipSegment) {
                            if (currentBoard[nr][nc] === CellStateEnum.SHIP) {
                                return false; 
                            }
                        }
                    } 
                }
            }
            if (currentBoard[r][c] === CellStateEnum.SHIP) {
                return false; 
            }
        }
        return true;
    }, []);

    const handleCellClick = (row, col) => {
        setError('');
        if (!selectedShip) {
            setError('Пожалуйста, выберите корабль для размещения.');
            return;
        }
        if (selectedShip.placed >= selectedShip.count) {
            setError('Все корабли этого типа уже размещены.'); 
            return;
        }
        if (!canPlaceShip(board, row, col, selectedShip.size, orientation)) {
            setError('Невозможно разместить корабль здесь (пересечение, выход за границы или касание других кораблей).');
            return;
        }
        const newBoard = board.map(r => [...r]);
        for (let i = 0; i < selectedShip.size; i++) {
            const r_coord = orientation === 'horizontal' ? row : row + i;
            const c_coord = orientation === 'horizontal' ? col + i : col;
            newBoard[r_coord][c_coord] = CellStateEnum.SHIP;
        }
        setBoard(newBoard);
        const newShips = ships.map(s =>
            s.id === selectedShip.id ? { ...s, placed: s.placed + 1 } : s
        );
        setShips(newShips);
        const nextShip = newShips.find(s => s.id === selectedShip.id && s.placed < s.count) || 
                         newShips.find(s => s.placed < s.count) || 
                         null;
        setSelectedShip(nextShip);
    };
    
    const handleShipSelect = (shipId) => {
        const ship = ships.find(s => s.id === shipId);
        if (ship && ship.placed < ship.count) {
            setSelectedShip(ship);
            setError('');
        } else if (ship) {
            setError(`Все корабли типа "${ship.name}" уже размещены.`);
            setSelectedShip(null); 
        }
    };

    const handleConfirmPlacement = () => {
        const allShipsPlaced = ships.every(s => s.placed === s.count);
        if (!allShipsPlaced) {
            setError('Не все корабли размещены. Пожалуйста, разместите все доступные корабли.');
            return;
        }
        setError('');
        onPlacementConfirmed(board);
    };
    
    const handleResetBoard = () => {
        setBoard(Array(BOARD_SIZE).fill(null).map(() => Array(BOARD_SIZE).fill(CellStateEnum.EMPTY)));
        const resetShips = JSON.parse(JSON.stringify(initialShipsToPlace));
        setShips(resetShips);
        setSelectedShip(resetShips.find(s => s.placed < s.count) || null);
        setError('');
    };

    const getCellClass = (cellValue) => {
        const stateName = Object.keys(CellStateEnum).find(key => CellStateEnum[key] === cellValue);
        return stateName ? `cell-${stateName.toLowerCase()}` : 'cell-empty';
    };

    const allShipsFullyPlaced = ships.every(s => s.placed === s.count);

    return (
        <div className="ship-placement-container card">
            <h2 className="placement-title">
                Расстановка кораблей: {playerName}
                <div className="game-id-section">
                    <span>ID:</span>
                    <span className="game-id">#{gameId}</span>
                    <button 
                        onClick={handleCopyGameId} 
                        className={`copy-button ${isCopied ? 'copied' : ''}`}
                        title="Копировать ID игры"
                    >
                        {isCopied ? <CheckIcon /> : <CopyIcon />}
                        {isCopied ? 'Скопировано' : 'Копировать'}
                    </button>
                </div>
            </h2>
            
            {error && 
                <div className="game-error-banner card">
                    <p><ErrorIcon /> {error}</p>
                    <button onClick={() => setError('')} className="close-error-button" aria-label="Закрыть ошибку">&times;</button>
                </div>
            }

            <div className="placement-main-content">
                <div className="placement-controls-area">
                    <div className="control-group ship-selection">
                        <h4><ShipIcon /> Выберите корабль:</h4>
                        <div className="buttons-group horizontal">
                            {ships.map(ship => (
                                <button
                                    key={ship.id}
                                    onClick={() => handleShipSelect(ship.id)}
                                    disabled={ship.placed >= ship.count}
                                    className={`secondary-button ${selectedShip?.id === ship.id && ship.placed < ship.count ? 'active' : ''}`}
                                >
                                    {ship.name} ({ship.size}) 
                                    <span className="ship-count-badge">{ship.count - ship.placed}</span>
                                </button>
                            ))}
                        </div>
                    </div>
                    <div className="control-group orientation-selection">
                        <h4><RotateIcon /> Ориентация:</h4>
                        <div className="buttons-group horizontal">
                            <button
                                onClick={() => setOrientation('horizontal')}
                                className={`secondary-button ${orientation === 'horizontal' ? 'active' : ''}`}
                            >
                                Горизонтально
                            </button>
                            <button
                                onClick={() => setOrientation('vertical')}
                                className={`secondary-button ${orientation === 'vertical' ? 'active' : ''}`}
                            >
                                Вертикально
                            </button>
                        </div>
                    </div>
                </div>

                <div className="placement-board-area">
                    <div className="placement-board">
                        {board.flat().map((cell, index) => {
                            const rowIndex = Math.floor(index / BOARD_SIZE);
                            const colIndex = index % BOARD_SIZE;
                            return (
                                <div
                                    key={`${rowIndex}-${colIndex}`}
                                    className={`board-cell ${getCellClass(cell)}`}
                                    onClick={() => handleCellClick(rowIndex, colIndex)}
                                    role="button"
                                    tabIndex={0}
                                    aria-label={`Cell ${rowIndex}-${colIndex}`}
                                >
                                </div>
                            );
                        })}
                    </div>
                </div>
            </div>

            <div className="placement-actions">
                <button onClick={handleResetBoard} className="secondary-button">
                    <ResetIcon /> Сбросить
                </button>
                <button 
                    onClick={handleConfirmPlacement} 
                    className="primary-button" 
                    disabled={!allShipsFullyPlaced}
                >
                    <ConfirmIcon /> Подтвердить расстановку
                </button>
            </div>
        </div>
    );
};

export default ShipPlacement; 