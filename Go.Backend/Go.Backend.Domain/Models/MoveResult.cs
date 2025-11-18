using Go.Backend.Domain.Enums;
using Go.Backend.Domain.Entities;
using Go.Backend.Domain.ValueObjects;

namespace Go.Backend.Domain.Models;

public class MoveResult
{
    private MoveResult(bool success, string? error, Position? position, IReadOnlyCollection<Position> captured,
        Board? boardAfterMove)
    {
        Success = success;
        Error = error;
        Position = position;
        Captured = captured;
        BoardAfterMove = boardAfterMove;
    }

    public bool Success { get; }
    public string? Error { get; }
    public Position? Position { get; }
    public IReadOnlyCollection<Position> Captured { get; }
    public Board? BoardAfterMove { get; }

    public static MoveResult Failed(string reason) =>
        new(false, reason, null, Array.Empty<Position>(), null);

    public static MoveResult Ok(Position position, IReadOnlyCollection<Position> captured, Board boardAfterMove) =>
        new(true, null, position, captured, boardAfterMove);
}
