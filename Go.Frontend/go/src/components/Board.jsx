import React, { useState } from "react";

export default function Board() {
    const size = 19;
    const paddingPercent = 6;
    const stepPercent = (100 - 2 * paddingPercent) / (size - 1);
    const indices = Array.from({ length: size }, (_, i) => i);
    const hoshi = [3, 9, 15];

    const [stones, setStones] = useState({});
    const [playerColor, setPlayerColor] = useState(null);
    const [difficulty, setDifficulty] = useState(null);
    const [gameState, setGameState] = useState(null); // Server-authoritative state
    const [gameId, setGameId] = useState(null);
    const [gameStarted, setGameStarted] = useState(false);
    const [loading, setLoading] = useState(false);
    const [moveInFlight, setMoveInFlight] = useState(false);
    const [error, setError] = useState(null);

    // Helper: Convert server board array to stones map
    const boardArrayToStones = (boardArray) => {
        const stoneMap = {};
        for (let r = 0; r < boardArray.length; r++) {
            const line = boardArray[r];
            for (let c = 0; c < line.length; c++) {
                const ch = line[c];
                if (ch === 'B') stoneMap[`${r}-${c}`] = 'black';
                else if (ch === 'W') stoneMap[`${r}-${c}`] = 'white';
            }
        }
        return stoneMap;
    };

    // Helper: Normalize server player name to lowercase
    const normalizePlayer = (serverPlayer) => {
        return serverPlayer ? serverPlayer.toLowerCase() : null;
    };

    // Helper: Apply server state to UI
    const applyGameState = (state) => {
        setGameState(state);
        setStones(boardArrayToStones(state.board));
    };

    const resetBoard = () => {
        setStones({});
        setGameState(null);
        setGameId(null);
        setGameStarted(false);
        setError(null);
        setPlayerColor(null);
    };

    const chooseColor = (color) => {
        if (color === "random") {
            setPlayerColor(Math.random() > 0.5 ? "black" : "white");
        } else {
            setPlayerColor(color);
        }
    };

    const handlePlayClick = async () => {
        // Validate player color is selected
        if (!playerColor) {
            setError("Please choose a color before playing!");
            return;
        }

        setLoading(true);
        setError(null);

        try {
            const response = await fetch("/api/games", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
            });

            if (!response.ok) {
                const errText = await response.text();
                throw new Error(errText || `Failed to create game: ${response.statusText}`);
            }

            const gameData = await response.json();
            setGameId(gameData.gameId);
            setGameStarted(true);
            applyGameState(gameData);
        } catch (err) {
            setError(err.message || "Error creating game");
            console.error("Error:", err);
        } finally {
            setLoading(false);
        }
    };

    // Play a human move
    const playMove = async (row, col) => {
        if (!gameStarted || !gameState || moveInFlight) return;

        // Check if position is occupied
        const key = `${row}-${col}`;
        if (stones[key]) {
            setError("Position already occupied!");
            return;
        }

        // Check if it's the player's turn
        const nextPlayer = normalizePlayer(gameState.nextPlayer);
        if (nextPlayer !== playerColor) {
            setError(`It's ${gameState.nextPlayer}'s turn, not yours!`);
            return;
        }

        setMoveInFlight(true);
        setError(null);

        try {
            const response = await fetch(`/api/games/${gameId}/moves`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ x: row, y: col, color: playerColor, pass: false })
            });

            if (!response.ok) {
                const errText = await response.text();
                throw new Error(errText || `Move failed: ${response.statusText}`);
            }

            const payload = await response.json();
            applyGameState(payload.state);
        } catch (err) {
            setError(err.message || "Error making move");
            console.error("Move error:", err);
        } finally {
            setMoveInFlight(false);
        }
    };

    const handleIntersectionClick = (row, col) => {
        // Board is disabled until game is started or if move is in flight
        if (!gameStarted || moveInFlight) return;
        playMove(row, col);
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

                <div style={mainLayoutStyle}>
                    {/* Left side: Board */}
                    <div style={boardWrapperStyle}>
                        <div style={gameStarted ? boardStyle : { ...boardStyle, opacity: 0.6, pointerEvents: "none" }}>
                            {renderIntersections()}
                            {renderGridLines()}
                            {renderHoshi()}
                            {renderStones()}

                            {!gameStarted && (
                                <div style={boardOverlayStyle}>
                                    Click "Play" to start
                                </div>
                            )}

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
                        <p style={statusStyle}>
                            {gameStarted && gameState ? (
                                <>
                                    <div>Next Player: <span style={{
                                        color: normalizePlayer(gameState.nextPlayer) === "black" ? "#000" : "#fff",
                                        backgroundColor: normalizePlayer(gameState.nextPlayer) === "black" ? "#fff" : "#000",
                                        padding: "2px 8px",
                                        borderRadius: "4px",
                                        fontWeight: "bold",
                                    }}>
                                        {gameState.nextPlayer.toUpperCase()}
                                    </span></div>
                                    <div style={{ fontSize: "12px", marginTop: "8px", color: "#666" }}>
                                        Moves: {gameState.moveNumber} | Black: {gameState.blackCaptures} | White: {gameState.whiteCaptures}
                                    </div>
                                    {gameState.isFinished && (
                                        <div style={{ marginTop: "8px", color: "#d4a574", fontWeight: "bold" }}>
                                            Game Over! Winner: {gameState.winner || "Draw"}
                                        </div>
                                    )}
                                </>
                            ) : (
                                "Click Play to start a game"
                            )}
                        </p>
                    </div>

                    {/* Right side: Control Panel */}
                    <div style={controlPanelStyle}>
                        {/* Play As Section */}
                        <div style={panelSectionStyle}>
                            <h3 style={panelTitleStyle}>Play As</h3>
                            <div style={optionGroupStyle}>
                                <button
                                    onClick={() => chooseColor("black")}
                                    style={{
                                        ...optionButtonStyle,
                                        backgroundColor: playerColor === "black" ? "#d4a574" : "transparent",
                                        borderColor: playerColor === "black" ? "#8b5a2b" : "#ccc",
                                    }}
                                >
                                    ⚫ Black
                                </button>
                                <button
                                    onClick={() => chooseColor("white")}
                                    style={{
                                        ...optionButtonStyle,
                                        backgroundColor: playerColor === "white" ? "#d4a574" : "transparent",
                                        borderColor: playerColor === "white" ? "#8b5a2b" : "#ccc",
                                    }}
                                >
                                    ⚪ White
                                </button>
                                <button
                                    onClick={() => chooseColor("random")}
                                    style={{
                                        ...optionButtonStyle,
                                        backgroundColor: playerColor === "random" ? "#d4a574" : "transparent",
                                        borderColor: playerColor === "random" ? "#8b5a2b" : "#ccc",
                                    }}
                                >
                                    🎲 Random
                                </button>
                            </div>
                            {playerColor && (
                                <p style={chosenColorStyle}>
                                    Your Color: <strong>{playerColor.toUpperCase()}</strong>
                                </p>
                            )}
                        </div>

                        {/* Difficulty Section */}
                        <div style={panelSectionStyle}>
                            <h3 style={panelTitleStyle}>Difficulty</h3>
                            <div style={optionGroupStyle}>
                                <button
                                    onClick={() => setDifficulty("easy")}
                                    style={{
                                        ...optionButtonStyle,
                                        backgroundColor: difficulty === "easy" ? "#d4a574" : "transparent",
                                        borderColor: difficulty === "easy" ? "#8b5a2b" : "#ccc",
                                    }}
                                >
                                    🟢 Easy
                                </button>
                                <button
                                    onClick={() => setDifficulty("normal")}
                                    style={{
                                        ...optionButtonStyle,
                                        backgroundColor: difficulty === "normal" ? "#d4a574" : "transparent",
                                        borderColor: difficulty === "normal" ? "#8b5a2b" : "#ccc",
                                    }}
                                >
                                    🟡 Normal
                                </button>
                                <button
                                    onClick={() => setDifficulty("hard")}
                                    style={{
                                        ...optionButtonStyle,
                                        backgroundColor: difficulty === "hard" ? "#d4a574" : "transparent",
                                        borderColor: difficulty === "hard" ? "#8b5a2b" : "#ccc",
                                    }}
                                >
                                    🔴 Hard
                                </button>
                            </div>
                            {difficulty && (
                                <p style={chosenColorStyle}>
                                    Selected: <strong>{difficulty.toUpperCase()}</strong>
                                </p>
                            )}
                        </div>

                        {/* Actions Section */}
                        <div style={panelSectionStyle}>
                            {error && (
                                <div style={errorMessageStyle}>
                                    {error}
                                </div>
                            )}
                            <button 
                                onClick={gameStarted ? resetBoard : handlePlayClick}
                                style={{
                                    ...primaryButtonStyle,
                                    opacity: (loading || moveInFlight) ? 0.6 : 1,
                                    cursor: (loading || moveInFlight) ? "not-allowed" : "pointer",
                                }}
                                disabled={loading || moveInFlight}
                            >
                                {loading ? "Starting..." : gameStarted ? "New Game" : "Play"}
                            </button>
                            {gameStarted && gameState && (
                                <div style={gameStatusStyle}>
                                    <p style={{ margin: "0 0 8px 0" }}>
                                        {gameState.isFinished ? "Game Finished ✓" : "Game Active ✓"}
                                    </p>
                                    <p style={{ margin: "0", fontSize: "12px", color: "#888" }}>
                                        Game ID: {gameId?.substring(0, 8)}...
                                    </p>
                                </div>
                            )}
                        </div>
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
    width: "100%",
    height: "100%",
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
    justifyContent: "flex-start",
    padding: "20px",
    boxSizing: "border-box",
};

const titleStyle = {
    margin: "0 0 15px 0",
    textAlign: "center",
    color: "#333",
    fontSize: "36px",
    fontWeight: "bold",
};

const mainLayoutStyle = {
    display: "flex",
    gap: "25px",
    alignItems: "center",
    justifyContent: "center",
    width: "100%",
    height: "calc(100vh - 100px)",
    maxHeight: "calc(100vh - 100px)",
};

const boardWrapperStyle = {
    display: "flex",
    flexDirection: "column",
    gap: "15px",
    alignItems: "center",
    flex: "0 1 auto",
};

const boardStyle = {
    width: "min(75vw, 75vh)",
    height: "min(75vw, 75vh)",
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
};

const statusStyle = {
    margin: "0",
    textAlign: "center",
    color: "#666",
    fontSize: "16px",
};

const controlPanelStyle = {
    display: "flex",
    flexDirection: "column",
    gap: "20px",
    minWidth: "320px",
    padding: "25px",
    backgroundColor: "rgba(255, 255, 255, 0.95)",
    borderRadius: "8px",
    boxShadow: "0 4px 16px rgba(0,0,0,0.1)",
    height: "fit-content",
};

const panelSectionStyle = {
    display: "flex",
    flexDirection: "column",
    gap: "12px",
};

const panelTitleStyle = {
    margin: "0",
    color: "#333",
    fontSize: "18px",
    fontWeight: "bold",
    textAlign: "center",
};

const optionGroupStyle = {
    display: "flex",
    flexDirection: "column",
    gap: "8px",
};

const optionButtonStyle = {
    padding: "12px 16px",
    fontSize: "14px",
    color: "#333",
    border: "2px solid #ccc",
    borderRadius: "6px",
    cursor: "pointer",
    fontWeight: "600",
    transition: "all 0.2s ease",
    backgroundColor: "transparent",
};

const chosenColorStyle = {
    margin: "10px 0 0 0",
    padding: "10px",
    textAlign: "center",
    backgroundColor: "#f0f0f0",
    borderRadius: "4px",
    color: "#666",
    fontSize: "14px",
};

const primaryButtonStyle = {
    padding: "14px 24px",
    fontSize: "16px",
    backgroundColor: "#8b5a2b",
    color: "white",
    border: "none",
    borderRadius: "6px",
    cursor: "pointer",
    fontWeight: "bold",
    transition: "background-color 0.3s ease",
    boxShadow: "0 4px 12px rgba(0,0,0,0.2)",
};

const errorMessageStyle = {
    padding: "12px",
    backgroundColor: "#ffebee",
    color: "#c62828",
    borderRadius: "4px",
    fontSize: "14px",
    textAlign: "center",
    border: "1px solid #ef5350",
};

const gameStatusStyle = {
    padding: "12px",
    backgroundColor: "#e8f5e9",
    color: "#2e7d32",
    borderRadius: "4px",
    fontSize: "14px",
    textAlign: "center",
    border: "1px solid #81c784",
};

const boardOverlayStyle = {
    position: "absolute",
    top: "50%",
    left: "50%",
    transform: "translate(-50%, -50%)",
    backgroundColor: "rgba(0, 0, 0, 0.7)",
    color: "white",
    padding: "20px 30px",
    borderRadius: "8px",
    fontSize: "18px",
    fontWeight: "bold",
    zIndex: 10,
    whiteSpace: "nowrap",
};
