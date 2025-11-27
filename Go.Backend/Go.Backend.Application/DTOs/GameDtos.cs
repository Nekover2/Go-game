using System;
using System.Collections.Generic;

namespace Go.Backend.Application.DTOs
{
    // Output: Trạng thái bàn cờ trả về cho Client
    public class GameStateDto
    {
        public Guid GameId { get; set; }
        public int Size { get; set; } = 19;
        public string NextPlayer { get; set; } = "Black";
        public int MoveNumber { get; set; }
        public bool IsFinished { get; set; }
        public string? Winner { get; set; }
        public int BlackCaptures { get; set; }
        public int WhiteCaptures { get; set; }
        public string[] Board { get; set; } = Array.Empty<string>(); // Mảng 19 chuỗi .........
    }

    // Input: Yêu cầu đi quân của người chơi
    public class MakeMoveRequest
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Color { get; set; } = string.Empty; // "black" or "white"
        public bool Pass { get; set; }
    }

    // Input: Yêu cầu Bot đi quân
    public class BotMoveRequest
    {
        public string? Color { get; set; }
    }

    // Output: Kết quả sau một nước đi
    public class MoveResponseDto
    {
        public MoveCoordinateDto? Move { get; set; } // Null nếu Pass
        public List<MoveCoordinateDto> Captured { get; set; } = new();
        public GameStateDto State { get; set; } = new();
    }

    public class MoveCoordinateDto
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}