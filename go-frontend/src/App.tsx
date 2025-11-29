import { useState } from 'react';
import { GoBoard } from './components/GoBoard';
import { gameService } from './services/api';
import type { GameState } from './types/game';
import './App.css';

function App() {
    const [game, setGame] = useState<GameState | null>(null);
    const [status, setStatus] = useState<string>("Nhấn 'New Game' để bắt đầu");
    const [isBotThinking, setIsBotThinking] = useState(false);

    const startNewGame = async () => {
        try {
            const newGame = await gameService.createGame();
            setGame(newGame);
            setStatus(`Lượt của: ${newGame.nextPlayer}`);
        } catch (error) {
            console.error(error);
            setStatus("Lỗi kết nối Backend!");
        }
    };

    const handleCellClick = async (x: number, y: number) => {
        if (!game || isBotThinking || game.isFinished) return;

        try {
            // 1. Người chơi đi (Đen)
            const result = await gameService.playMove(game.gameId, x, y, "Black");
            setGame(result.state);
            setStatus("Bot đang suy nghĩ...");
            setIsBotThinking(true);

            // 2. Gọi Bot đi ngay sau đó (Trắng)
            // (Thêm delay nhỏ cho tự nhiên nếu muốn)
            setTimeout(async () => {
                try {
                    const botResult = await gameService.getBotMove(game.gameId, "White");
                    setGame(botResult.state);
                    setStatus(`Đến lượt bạn (${botResult.state.nextPlayer})`);
                } catch (e) {
                    setStatus("Bot gặp lỗi hoặc chịu thua!");
                } finally {
                    setIsBotThinking(false);
                }
            }, 100);

        } catch (error: any) {
            alert(error.response?.data?.message || "Nước đi không hợp lệ!");
        }
    };

    return (
        <div className="app-container" style={{ padding: 20, fontFamily: 'Arial' }}>
            <h1>Go AI Project (.NET + React + PyTorch)</h1>
            
            <div className="controls" style={{ marginBottom: 20 }}>
                <button onClick={startNewGame} disabled={isBotThinking} style={{ padding: '10px 20px', fontSize: 16, cursor: 'pointer' }}>
                    New Game
                </button>
                <span style={{ marginLeft: 20, fontWeight: 'bold' }}>{status}</span>
            </div>

            {game && (
                <div style={{ display: 'flex', gap: 40 }}>
                    <GoBoard boardData={game.board} onCellClick={handleCellClick} />
                    
                    <div className="info-panel">
                        <h3>Thông tin trận đấu</h3>
                        <p>Số nước đi: {game.moveNumber}</p>
                        <p>Đen ăn: {game.blackCaptures}</p>
                        <p>Trắng ăn: {game.whiteCaptures}</p>
                    </div>
                </div>
            )}
        </div>
    );
}

export default App;