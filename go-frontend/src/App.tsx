import { useState, useEffect } from 'react';
import { gameService } from './services/api';
import type { GameState } from './types/game';
import classNames from 'classnames';
import './App.css';

function App() {
  const [game, setGame] = useState<GameState | null>(null);
  const [statusMessage, setStatusMessage] = useState<string>("");
  const [isBotThinking, setIsBotThinking] = useState(false);
  const [userColor, setUserColor] = useState<"Black" | "White">("Black");
  const [playAsSelection, setPlayAsSelection] = useState<"Black" | "White" | "Random" | "BotVsBot">("Black");

  // Tọa độ 9 điểm sao (Hoshi) trên bàn 19x19
  const starPoints = [
    "3-3", "3-9", "3-15",
    "9-3", "9-9", "9-15",
    "15-3", "15-9", "15-15"
  ];
  useEffect(() => {
    let timer: any;

    // Điều kiện: Nếu đang chọn chế độ BotVsBot + Game đang chạy + Bot không đang nghĩ + Game chưa kết thúc
    if (playAsSelection === "BotVsBot" && game && !game.isFinished && !isBotThinking) {

      // Delay 1 chút (1000ms) để người xem kịp nhìn nước đi
      timer = setTimeout(() => {
        triggerBotMove(game.gameId, game.nextPlayer);
      }, 1000);
    }

    return () => clearTimeout(timer); // Cleanup timer nếu component unmount hoặc state đổi
  }, [game, playAsSelection, isBotThinking]);

  const startNewGame = async () => {
    try {
      const newGame = await gameService.createGame();
      setGame(newGame);
      setStatusMessage("");

      // Xử lý chọn màu
      let finalUserColor = playAsSelection;
      if (playAsSelection === "Random") {
        finalUserColor = Math.random() < 0.5 ? "Black" : "White";
        setUserColor(finalUserColor as "Black" | "White");
      }
      else if (playAsSelection === "BotVsBot") {
        // Nếu là Bot vs Bot, User chỉ là khán giả (Spectator)
        // Bot Đen đi trước ngay lập tức
        // (Logic useEffect ở trên sẽ tự bắt lấy event này để chạy tiếp, nhưng ta cần kích mồi nước đầu tiên)
        triggerBotMove(newGame.gameId, "Black");
      }
      else {
        setUserColor(playAsSelection as "Black" | "White");
      }
      setStatusMessage("");

      // Nếu User chọn Trắng -> Bot (Đen) đi trước
      if (finalUserColor === "White") {
        triggerBotMove(newGame.gameId, "Black");
      }

    } catch (error) {
      console.error(error);
      setStatusMessage("Lỗi: Không thể kết nối tới Server!");
    }
  };

  const triggerBotMove = async (gameId: string, botColor: string) => {
    // Nếu game đã kết thúc thì dừng lại
    if (game?.isFinished) return;

    setIsBotThinking(true);
    try {
      const botResult = await gameService.getBotMove(gameId, botColor);
      setGame(botResult.state);
    } catch (e) {
      setStatusMessage("Bot gặp lỗi hoặc chịu thua!");
    } finally {
      setIsBotThinking(false);
    }
  };

  const handleCellClick = async (x: number, y: number) => {
    if (!game || isBotThinking || game.isFinished) return;

    if (playAsSelection === "BotVsBot") return;

    // Check lượt đi
    if (game.nextPlayer !== userColor) {
      setStatusMessage(`It's ${game.nextPlayer}'s turn, not yours!`);
      return;
    }

    // Check ô có trống không (Logic frontend sơ bộ)
    if (game.board[x][y] !== '.') return;

    try {
      setStatusMessage("");
      // 1. Người đi
      const result = await gameService.playMove(game.gameId, x, y, userColor);
      setGame(result.state);

      // 2. Bot đi
      if (!result.state.isFinished) {
        const botColor = userColor === "Black" ? "White" : "Black";
        triggerBotMove(game.gameId, botColor);
      }
    } catch (error: any) {
      setStatusMessage(error.response?.data?.message || "Nước đi không hợp lệ!");
    }
  };

  // Render bàn cờ giả khi chưa có game
  // Render bàn cờ
  const renderBoard = () => {
    const boardData = game ? game.board : Array(19).fill(".".repeat(19));

    return boardData.map((rowStr, x) => (
      rowStr.split('').map((cellChar: string, y: number) => {
        const isStarPoint = starPoints.includes(`${x}-${y}`);

        return (
          <div
            key={`${x}-${y}`}
            className={classNames("intersection", `row-${x}`, `col-${y}`)}
            onClick={() => handleCellClick(x, y)}
          >
            {/* Điểm sao (chỉ hiện khi không có quân) */}
            {isStarPoint && !cellChar.match(/[BW]/) && <div className="star-point" />}

            {/* Quân cờ thật (chỉ hiện khi dữ liệu là B hoặc W) */}
            {cellChar === 'B' && <div className="stone black" />}
            {cellChar === 'W' && <div className="stone white" />}

          </div>
        );
      })
    ));
  };


  return (
    <div className="game-container">
      <h1>Go Game 19x19</h1>

      <div className="main-content">
        {/* --- LEFT: BOARD --- */}
        <div className="board-section">
          <div className="board-wrapper">
            <div className="go-board">
              {renderBoard()}
            </div>
          </div>
          {game && (
            <div className="board-footer">
              Next Player: <span className={classNames("next-player-badge", { white: game.nextPlayer === "White" })}>
                {game.nextPlayer.toUpperCase()}
              </span>
              <br />
              <span style={{ marginTop: '5px', display: 'block', fontSize: '12px' }}>
                Moves: {game.moveNumber} | Black: {game.blackCaptures} | White: {game.whiteCaptures}
              </span>
            </div>
          )}
        </div>

        {/* --- RIGHT: CONTROL PANEL --- */}
        <div className="control-panel">
          <div className="panel-title">Play As</div>

          <div className="play-as-group">
            {/* Các nút cũ giữ nguyên */}
            <div
              className={classNames("radio-btn", { active: playAsSelection === "Black" })}
              onClick={() => setPlayAsSelection("Black")}
            >
              <span className="icon-dot black"></span> Black
            </div>
            <div
              className={classNames("radio-btn", { active: playAsSelection === "White" })}
              onClick={() => setPlayAsSelection("White")}
            >
              <span className="icon-dot white"></span> White
            </div>
            <div
              className={classNames("radio-btn", { active: playAsSelection === "Random" })}
              onClick={() => setPlayAsSelection("Random")}
            >
              🎲 Random
            </div>

            {/* --- NÚT MỚI: BOT VS BOT --- */}
            <div
              className={classNames("radio-btn", { active: playAsSelection === "BotVsBot" })}
              onClick={() => setPlayAsSelection("BotVsBot")}
              style={{ borderTop: '1px solid #eee', marginTop: '5px', paddingTop: '10px' }}
            >
              🤖 Bot vs Bot 🤖
            </div>
          </div>

          {/* Hiển thị thông báo khi đang xem Bot đấu */}
          {game && playAsSelection === "BotVsBot" && (
            <div className="info-box gray">
              Mode: <strong>Spectator</strong> (Auto)
            </div>
          )}

          {/* ... (Các phần hiển thị User Color cũ nên ẩn đi nếu là BotVsBot) ... */}
          {game && playAsSelection !== "BotVsBot" && (
            <div className="info-box gray">
              Your Color: {userColor.toUpperCase()}
            </div>
          )}

          {/* Hiển thị màu quân của User */}
          {game && (
            <div className="info-box gray">
              Your Color: {userColor.toUpperCase()}
            </div>
          )}
          {isBotThinking && (
            <div className="info-box" style={{
              backgroundColor: '#fff8e1',
              color: '#f57f17',
              border: '1px solid #ffecb3',
              fontWeight: 'bold',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              gap: '8px'
            }}>
              {/* Icon loading xoay xoay (CSS spinner đơn giản) */}
              <span style={{
                width: '12px',
                height: '12px',
                border: '2px solid #f57f17',
                borderTop: '2px solid transparent',
                borderRadius: '50%',
                display: 'inline-block',
                animation: 'spin 1s linear infinite'
              }}></span>
              Bot is thinking... Please wait
            </div>
          )}
          {/* Thông báo lỗi / Lượt */}
          {statusMessage && (
            <div className="info-box error">
              {statusMessage}
            </div>
          )}

          <button
            className="btn-new-game"
            onClick={startNewGame}
            disabled={isBotThinking}
          >
            {game ? "New Game" : "Start Game"}
          </button>

          {/* Trạng thái game */}
          {game && (
            <div className="info-box success">
              Game Active ✓
              <div className="game-id">Game ID: {game.gameId.substring(0, 8)}...</div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default App;