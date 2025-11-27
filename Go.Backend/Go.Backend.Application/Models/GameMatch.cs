using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Go.Backend.Domain.Entities;
using Go.Backend.Domain.Enums;

namespace Go.Backend.Application.Models
{
    public class GameMatch
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Board Board { get; set; }
        public PlayerColor NextPlayer { get; set; } = PlayerColor.Black;
        public int MoveNumber { get; set; } = 0;
        public bool IsFinished { get; set; }
        public PlayerColor? Winner { get; set; }
        
        // Thống kê tù binh
        public int BlackCaptures { get; set; }
        public int WhiteCaptures { get; set; }

        // Logic check 2 lần pass liên tiếp để kết thúc game
        private bool _lastMoveWasPass = false;

        public GameMatch(int size = 19)
        {
            Board = new Board(size);
        }

        // Helper chuyển đổi Board[,] thành string[] cho API
        public string[] GetBoardStringArray()
        {
            var result = new string[Board.Size];
            for (int r = 0; r < Board.Size; r++)
            {
                var sb = new StringBuilder();
                for (int c = 0; c < Board.Size; c++)
                {
                    char ch = Board.Stones[r, c] switch
                    {
                        PlayerColor.Black => 'B',
                        PlayerColor.White => 'W',
                        _ => '.'
                    };
                    sb.Append(ch);
                }
                result[r] = sb.ToString();
            }
            return result;
        }
        
        public void ApplyPass()
        {
            if (_lastMoveWasPass)
            {
                IsFinished = true;
                // TODO: Tính điểm endgame nếu cần
            }
            _lastMoveWasPass = true;
            NextPlayer = NextPlayer.Opponent();
            MoveNumber++;
        }

        public void RegisterMoveSuccess(int capturedCount)
        {
            _lastMoveWasPass = false;
            
            if (NextPlayer == PlayerColor.Black) BlackCaptures += capturedCount;
            else WhiteCaptures += capturedCount;

            NextPlayer = NextPlayer.Opponent();
            MoveNumber++;
        }
    }
}