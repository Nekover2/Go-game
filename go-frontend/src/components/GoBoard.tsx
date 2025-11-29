import React from 'react';
import './GoBoard.css';

interface Props {
    boardData: string[]; // Mảng 19 chuỗi từ backend
    onCellClick: (x: number, y: number) => void;
}

export const GoBoard: React.FC<Props> = ({ boardData, onCellClick }) => {
    // boardData là mảng string: [".........", "........."]
    
    return (
        <div className="go-board">
            {boardData.map((rowStr, x) => (
                rowStr.split('').map((cellChar, y) => (
                    <div key={`${x}-${y}`} className="cell" onClick={() => onCellClick(x, y)}>
                        {/* Render quân cờ nếu có */}
                        {cellChar === 'B' && <div className="stone black" />}
                        {cellChar === 'W' && <div className="stone white" />}
                    </div>
                ))
            ))}
        </div>
    );
};