namespace Go.Backend.Domain.Enums;

public enum StoneColor
{
    Empty = 0,
    Black = 1,
    White = 2
}

public static class StoneColorExtensions
{
    public static StoneColor Opponent(this StoneColor color) =>
        color switch
        {
            StoneColor.Black => StoneColor.White,
            StoneColor.White => StoneColor.Black,
            _ => StoneColor.Empty
        };
}
