using Go.Backend.Domain.Enums;
using Go.Backend.Domain.Entities;
using Go.Backend.Domain.ValueObjects;

namespace Go.Backend.Domain.Models;

public class MoveResult
{
    private MoveResult(bool success, string? error, Position? position, IReadOnlyCollection<Position> captured,
        Board? boardAfterMove, bool isPass)
    {
        Success = success;
        Error = error;
        Position = position;
        Captured = captured;
        BoardAfterMove = boardAfterMove;
        IsPass = isPass;
    }

    public bool Success { get; }
    public string? Error { get; }
    public Position? Position { get; }
    public IReadOnlyCollection<Position> Captured { get; }
    public Board? BoardAfterMove { get; }
    public bool IsPass { get; }

    public static MoveResult Failed(string reason) =>
        new(false, reason, null, Array.Empty<Position>(), null, false);

    public static MoveResult Ok(Position position, IReadOnlyCollection<Position> captured, Board boardAfterMove) =>
        new(true, null, position, captured, boardAfterMove, false);

    public static MoveResult PassOk(Board boardAfterMove, StoneColor nextPlayer) =>
        new(true, null, null, Array.Empty<Position>(), boardAfterMove, true);
}
