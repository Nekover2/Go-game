import React from 'react';
import './Stone.css';

const Stone = ({ color, x, y, onClick }) => {
    return (
        <div
            className={`stone stone-${color}`}
            style={{
                left: `${x}px`,
                top: `${y}px`,
            }}
            onClick={onClick}
        />
    );
};

export default Stone;