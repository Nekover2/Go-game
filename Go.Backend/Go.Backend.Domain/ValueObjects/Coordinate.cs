using System;

namespace Go.Backend.Domain.ValueObjects
{
    public readonly struct Coordinate : IEquatable<Coordinate>
    {
        public int X { get; }
        public int Y { get; }

        public Coordinate(int x, int y)
        {
            X = x;
            Y = y;
        }

        // Override các phương thức so sánh để dùng được trong Dictionary/HashSet
        public override bool Equals(object? obj) => obj is Coordinate c && Equals(c);
        public bool Equals(Coordinate other) => X == other.X && Y == other.Y;
        public override int GetHashCode() => HashCode.Combine(X, Y);
        
        public static bool operator ==(Coordinate left, Coordinate right) => left.Equals(right);
        public static bool operator !=(Coordinate left, Coordinate right) => !(left == right);

        public override string ToString() => $"({X}, {Y})";
    }
}