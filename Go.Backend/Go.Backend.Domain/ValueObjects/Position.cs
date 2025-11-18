using Go.Backend.Domain.Entities;
using Go.Backend.Domain.Enums;

namespace Go.Backend.Domain.ValueObjects;

public readonly record struct Position(int X, int Y)
{
    public static Position CreateValidated(int x, int y)
    {
        if (x is < 0 or >= Board.Size || y is < 0 or >= Board.Size)
        {
            throw new ArgumentOutOfRangeException(nameof(x), $"Position ({x},{y}) is outside the board.");
        }

        return new Position(x, y);
    }

    public bool IsInsideBoard() => X is >= 0 and < Board.Size && Y is >= 0 and < Board.Size;

    public override string ToString() => $"({X},{Y})";
}
