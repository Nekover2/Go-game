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
        public int BlackCaptures { get; set; }
        public int WhiteCaptures { get; set; }

        private bool _lastMoveWasPass = false;

        // --- GIỮ NGUYÊN Constructor chính ---
        public GameMatch(int size = 19)
        {
            Board = new Board(size);
        }

        // --- THÊM MỚI ĐOẠN NÀY ĐỂ SỬA LỖI ---
        // Constructor rỗng dành riêng cho EF Core (Binding)
        // Khi load từ DB, EF Core sẽ gọi hàm này trước, sau đó mới gán dữ liệu vào các Properties
#pragma warning disable CS8618 // Tắt cảnh báo Non-nullable vì EF sẽ tự fill data
        private GameMatch() 
        {
            // Khởi tạo tạm Board để tránh null reference nếu lỡ truy cập trước khi load xong
            Board = new Board(19); 
        }
#pragma warning restore CS8618
        // -------------------------------------

        // ... (Các hàm GetBoardStringArray, ApplyPass, RegisterMoveSuccess giữ nguyên)
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
                // Game over logic...
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