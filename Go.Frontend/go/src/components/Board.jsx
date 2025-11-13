import React, { useState } from "react";

export default function Board() {
    const size = 19;
    const paddingPercent = 6;
    const stepPercent = (100 - 2 * paddingPercent) / (size - 1);
    const indices = Array.from({ length: size }, (_, i) => i);
    const hoshi = [3, 9, 15];

    const [stones, setStones] = useState({});
    const [currentPlayer, setCurrentPlayer] = useState("black");
    const [playerColor, setPlayerColor] = useState(null);

    const handleIntersectionClick = (row, col) => {
        const key = `${row}-${col}`;
        if (stones[key]) return;

        const newStones = { ...stones, [key]: currentPlayer };
        setStones(newStones);
        setCurrentPlayer(currentPlayer === "black" ? "white" : "black");
    };

    const resetBoard = () => {
        setStones({});
        setCurrentPlayer("black");
    };

    const chooseColor = (color) => {
        if (color === "random") {
            setPlayerColor(Math.random() > 0.5 ? "black" : "white");
        } else {
            setPlayerColor(color);
        }
    };

    const getStoneAtPosition = (row, col) => {
        return stones[`${row}-${col}`];
    };

    const renderGridLines = () => {
        const lines = [];

        // Horizontal lines
        for (let i = 0; i < size; i++) {
            const top = paddingPercent + i * stepPercent;
            lines.push(
                <div
                    key={`h-${i}`}
                    style={{
                        position: "absolute",
                        left: `${paddingPercent}%`,
                        width: `calc(100% - ${paddingPercent * 2}%)`,
                        height: "1.6px",
                        background: "rgba(30,20,10,0.95)",
                        top: `${top}%`,
                        transform: "translateY(-0.8px)",
                        pointerEvents: "none",
                        zIndex: 0,
                    }}
                />
            );
        }

        // Vertical lines
        for (let i = 0; i < size; i++) {
            const left = paddingPercent + i * stepPercent;
            lines.push(
                <div
                    key={`v-${i}`}
                    style={{
                        position: "absolute",
                        top: `${paddingPercent}%`,
                        height: `calc(100% - ${paddingPercent * 2}%)`,
                        width: "1.6px",
                        background: "rgba(30,20,10,0.95)",
                        left: `${left}%`,
                        transform: "translateX(-0.8px)",
                        pointerEvents: "none",
                        zIndex: 0,
                    }}
                />
            );
        }

        return lines;
    };

    const renderHoshi = () => {
        const points = [];
        for (let r = 0; r < hoshi.length; r++) {
            for (let c = 0; c < hoshi.length; c++) {
                const row = hoshi[r];
                const col = hoshi[c];
                const top = paddingPercent + row * stepPercent;
                const left = paddingPercent + col * stepPercent;

                points.push(
                    <div
                        key={`hoshi-${row}-${col}`}
                        style={{
                            position: "absolute",
                            left: `${left}%`,
                            top: `${top}%`,
                            transform: "translate(-50%, -50%)",
                            width: 10,
                            height: 10,
                            background: "rgba(20,20,20,0.95)",
                            borderRadius: "50%",
                            boxShadow: "0 1px 0 rgba(255,255,255,0.08) inset, 0 2px 6px rgba(0,0,0,0.25)",
                            pointerEvents: "none",
                            zIndex: 2,
                        }}
                    />
                );
            }
        }
        return points;
    };

    const renderIntersections = () => {
        const intersections = [];
        for (let row = 0; row < size; row++) {
            for (let col = 0; col < size; col++) {
                const top = paddingPercent + row * stepPercent;
                const left = paddingPercent + col * stepPercent;
                const hasStone = getStoneAtPosition(row, col);

                intersections.push(
                    <div
                        key={`intersection-${row}-${col}`}
                        onClick={() => handleIntersectionClick(row, col)}
                        style={{
                            position: "absolute",
                            left: `${left}%`,
                            top: `${top}%`,
                            width: "40px",
                            height: "40px",
                            transform: "translate(-50%, -50%)",
                            cursor: hasStone ? "not-allowed" : "pointer",
                            zIndex: 1,
                        }}
                    />
                );
            }
        }
        return intersections;
    };

    const renderStones = () => {
        const stoneElements = [];
        for (let row = 0; row < size; row++) {
            for (let col = 0; col < size; col++) {
                const stone = getStoneAtPosition(row, col);
                if (!stone) continue;

                const top = paddingPercent + row * stepPercent;
                const left = paddingPercent + col * stepPercent;
                const isBlack = stone === "black";

                stoneElements.push(
                    <div
                        key={`stone-${row}-${col}`}
                        style={{
                            position: "absolute",
                            left: `${left}%`,
                            top: `${top}%`,
                            transform: "translate(-50%, -50%)",
                            width: "32px",
                            height: "32px",
                            borderRadius: "50%",
                            background: isBlack
                                ? "radial-gradient(circle at 30% 30%, rgba(100,100,100,0.6), #000)"
                                : "radial-gradient(circle at 30% 30%, rgba(255,255,255,0.9), #ddd)",
                            boxShadow: isBlack
                                ? "0 4px 12px rgba(0,0,0,0.6), inset 0 -2px 6px rgba(0,0,0,0.4)"
                                : "0 4px 12px rgba(0,0,0,0.3), inset 0 -2px 6px rgba(0,0,0,0.2)",
                            pointerEvents: "none",
                            zIndex: 3,
                        }}
                    />
                );
            }
        }
        return stoneElements;
    };

    return (
        <div style={pageStyle}>
            <div style={containerStyle}>
                <h1 style={titleStyle}>Go Game 19x19</h1>
                <p style={statusStyle}>
                    Current Player:{" "}
                    <span
                        style={{
                            color: currentPlayer === "black" ? "#000" : "#fff",
                            backgroundColor: currentPlayer === "black" ? "#fff" : "#000",
                            padding: "2px 8px",
                            borderRadius: "4px",
                            fontWeight: "bold",
                        }}
                    >
                        {currentPlayer.toUpperCase()}
                    </span>
                    {playerColor && (
                        <span style={{ marginLeft: "15px" }}>
                            Your Color:{" "}
                            <span
                                style={{
                                    color: playerColor === "black" ? "#000" : "#fff",
                                    backgroundColor: playerColor === "black" ? "#fff" : "#000",
                                    padding: "2px 8px",
                                    borderRadius: "4px",
                                    fontWeight: "bold",
                                }}
                            >
                                {playerColor.toUpperCase()}
                            </span>
                        </span>
                    )}
                </p>

                <div style={boardContainerStyle}>
                    <div style={boardStyle}>
                        {renderIntersections()}
                        {renderGridLines()}
                        {renderHoshi()}
                        {renderStones()}

                        <div
                            style={{
                                position: "absolute",
                                right: `${paddingPercent / 2}%`,
                                top: `${paddingPercent / 2}%`,
                                color: "rgba(40,20,10,0.7)",
                                fontSize: 12,
                                fontFamily: "serif",
                                pointerEvents: "none",
                                userSelect: "none",
                                opacity: 0.85,
                                zIndex: 2,
                            }}
                        >
                            黒白
                        </div>
                    </div>

                    <div style={sideButtonsStyle}>
                        <div style={buttonGroupStyle}>
                            <h3 style={buttonGroupTitleStyle}>Choose Color</h3>
                            <button
                                onClick={() => chooseColor("black")}
                                style={{
                                    ...colorButtonStyle,
                                    backgroundColor: playerColor === "black" ? "#6f4a2b" : "#8b5a2b",
                                    borderWidth: playerColor === "black" ? "3px" : "1px",
                                }}
                            >
                                ⚫ Black
                            </button>
                            <button
                                onClick={() => chooseColor("white")}
                                style={{
                                    ...colorButtonStyle,
                                    backgroundColor: playerColor === "white" ? "#6f4a2b" : "#8b5a2b",
                                    borderWidth: playerColor === "white" ? "3px" : "1px",
                                }}
                            >
                                ⚪ White
                            </button>
                            <button
                                onClick={() => chooseColor("random")}
                                style={{
                                    ...colorButtonStyle,
                                    backgroundColor: playerColor === "random" ? "#6f4a2b" : "#8b5a2b",
                                }}
                            >
                                🎲 Random
                            </button>
                        </div>
                        <button onClick={resetBoard} style={resetButtonStyle}>
                            Reset Board
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
}

const pageStyle = {
    width: "100vw",
    height: "100vh",
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
    justifyContent: "flex-start",
    background: "#f5f4f2",
    margin: 0,
    padding: "0",
    boxSizing: "border-box",
    overflow: "hidden",
};

const containerStyle = {
    width: "100vw",
    height: "100vh",
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
    justifyContent: "space-between",
    padding: "10px 0",
    boxSizing: "border-box",
};

const titleStyle = {
    margin: "20px 0 10px 0",
    textAlign: "center",
    color: "#333",
    fontSize: "32px",
};

const statusStyle = {
    margin: "0 0 20px 0",
    textAlign: "center",
    color: "#666",
    fontSize: "16px",
};

const boardStyle = {
    width: "min(90vw, 90vh - 120px)",
    height: "min(90vw, 90vh - 120px)",
    aspectRatio: "1 / 1",
    position: "relative",
    boxSizing: "border-box",
    borderRadius: 12,
    border: "12px solid #6f4a2b",
    background:
        "linear-gradient(180deg, #e9c99a 0%, #d7a55e 50%, #c58e3f 100%)," +
        "repeating-linear-gradient(90deg, rgba(0,0,0,0.03) 0 2px, transparent 2px 18px)," +
        "radial-gradient(ellipse at 30% 20%, rgba(0,0,0,0.04) 0 3%, transparent 8%)," +
        "radial-gradient(ellipse at 70% 70%, rgba(0,0,0,0.03) 0 2%, transparent 10%)",
    boxShadow: "0 18px 50px rgba(20,15,10,0.45), inset 0 -12px 30px rgba(0,0,0,0.08)",
    cursor: "default",
    margin: "0 auto",
};

const resetButtonStyle = {
    padding: "10px 30px",
    fontSize: "16px",
    backgroundColor: "#8b5a2b",
    color: "white",
    border: "none",
    borderRadius: "6px",
    cursor: "pointer",
    fontWeight: "bold",
    transition: "background-color 0.3s ease",
    boxShadow: "0 4px 8px rgba(0,0,0,0.2)",
};

const boardContainerStyle = {
    display: "flex",
    gap: "20px",
    alignItems: "center",
    justifyContent: "center",
    flex: 1,
    width: "100%",
};

const sideButtonsStyle = {
    display: "flex",
    flexDirection: "column",
    gap: "15px",
    justifyContent: "center",
    alignItems: "center",
    minWidth: "150px",
};

const buttonGroupStyle = {
    display: "flex",
    flexDirection: "column",
    gap: "10px",
    backgroundColor: "rgba(255, 255, 255, 0.8)",
    padding: "15px",
    borderRadius: "8px",
    boxShadow: "0 4px 12px rgba(0,0,0,0.1)",
};

const buttonGroupTitleStyle = {
    margin: "0 0 10px 0",
    textAlign: "center",
    color: "#333",
    fontSize: "16px",
    fontWeight: "bold",
};

const colorButtonStyle = {
    padding: "12px 20px",
    fontSize: "14px",
    backgroundColor: "#8b5a2b",
    color: "white",
    border: "1px solid #6f4a2b",
    borderRadius: "6px",
    cursor: "pointer",
    fontWeight: "bold",
    transition: "all 0.3s ease",
    boxShadow: "0 4px 8px rgba(0,0,0,0.2)",
};
