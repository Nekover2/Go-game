using Go.Backend.Domain.Enums;
using Go.Backend.Domain.ValueObjects;

namespace Go.Backend.Domain.Entities;

public class Board
{
    public const int Size = 19;

    private readonly StoneColor[,] _grid;

    public Board()
    {
        _grid = new StoneColor[Size, Size];
    }

    private Board(StoneColor[,] grid)
    {
        _grid = grid;
    }

    public StoneColor Get(Position position) => _grid[position.X, position.Y];

    public void PlaceStone(Position position, StoneColor color) => _grid[position.X, position.Y] = color;

    public void RemoveStone(Position position) => _grid[position.X, position.Y] = StoneColor.Empty;

    public IEnumerable<Position> GetEmptyPositions()
    {
        for (var x = 0; x < Size; x++)
        {
            for (var y = 0; y < Size; y++)
            {
                if (_grid[x, y] == StoneColor.Empty)
                {
                    yield return new Position(x, y);
                }
            }
        }
    }

    public IEnumerable<Position> GetNeighbors(Position position)
    {
        if (position.X > 0) yield return new Position(position.X - 1, position.Y);
        if (position.X < Size - 1) yield return new Position(position.X + 1, position.Y);
        if (position.Y > 0) yield return new Position(position.X, position.Y - 1);
        if (position.Y < Size - 1) yield return new Position(position.X, position.Y + 1);
    }

    public HashSet<Position> CollectGroup(Position start)
    {
        var color = Get(start);
        var visited = new HashSet<Position>();
        var queue = new Queue<Position>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var neighbor in GetNeighbors(current))
            {
                if (visited.Contains(neighbor))
                {
                    continue;
                }

                if (Get(neighbor) == color)
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        return visited;
    }

    public int CountLiberties(HashSet<Position> group)
    {
        var liberties = new HashSet<Position>();
        foreach (var position in group)
        {
            foreach (var neighbor in GetNeighbors(position))
            {
                if (Get(neighbor) == StoneColor.Empty)
                {
                    liberties.Add(neighbor);
                }
            }
        }

        return liberties.Count;
    }

    public string BuildSignature(StoneColor nextPlayer)
    {
        var buffer = new char[Size * Size + 1];
        var index = 0;
        for (var x = 0; x < Size; x++)
        {
            for (var y = 0; y < Size; y++)
            {
                buffer[index++] = _grid[x, y] switch
                {
                    StoneColor.Black => 'B',
                    StoneColor.White => 'W',
                    _ => '.'
                };
            }
        }

        buffer[buffer.Length - 1] = nextPlayer switch
        {
            StoneColor.Black => 'B',
            StoneColor.White => 'W',
            _ => '.'
        };

        return new string(buffer);
    }

    public Board Clone()
    {
        var clone = new StoneColor[Size, Size];
        Array.Copy(_grid, clone, _grid.Length);
        return new Board(clone);
    }
}
