using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Go.Backend.Application.DTOs;
using Go.Backend.Application.Interfaces;
using Go.Backend.Domain.Entities;
using Go.Backend.Domain.Enums;

namespace Go.Backend.Infrastructure.AI
{
    /// <summary>
    /// Mock AI Service for testing without a valid ONNX model.
    /// This service makes random valid moves on the board.
    /// </summary>
    public class MockGoAiService : IGoAiService
    {
        private readonly Random _random = new Random();

        public Task<MoveCoordinateDto?> GetBestMoveAsync(Board board, PlayerColor aiColor)
        {
            return Task.Run(() =>
            {
                // Get all valid empty positions
                var validMoves = new List<(int x, int y)>();

                for (int x = 0; x < board.Size; x++)
                {
                    for (int y = 0; y < board.Size; y++)
                    {
                        if (board.Stones[x, y] == PlayerColor.None)
                        {
                            validMoves.Add((x, y));
                        }
                    }
                }

                if (validMoves.Count == 0)
                {
                    // No valid moves, must pass
                    return null;
                }

                // Pick a random valid move (prefer center area for better gameplay)
                var centerMoves = validMoves
                    .Where(m => m.x >= 5 && m.x < board.Size - 5 && m.y >= 5 && m.y < board.Size - 5)
                    .ToList();

                var selectedMoves = centerMoves.Count > 0 ? centerMoves : validMoves;
                var selectedMove = selectedMoves[_random.Next(selectedMoves.Count)];

                return new MoveCoordinateDto
                {
                    X = selectedMove.x,
                    Y = selectedMove.y
                };
            });
        }
    }
}
