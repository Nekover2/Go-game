using System.Collections.Generic;
using Go.Backend.Domain.ValueObjects;

namespace Go.Backend.Domain.Entities
{
    public class MoveResult
    {
        public bool IsSuccess { get; set; }
        public List<Coordinate> CapturedStones { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public static MoveResult Success(List<Coordinate> captured) 
            => new MoveResult { IsSuccess = true, CapturedStones = captured };

        public static MoveResult Failure(string message) 
            => new MoveResult { IsSuccess = false, ErrorMessage = message };
    }
}