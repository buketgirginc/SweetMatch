using System;

namespace SweetMatch.Model
{
    public readonly struct GridPosition
    {
        public int X { get; }
        public int Y { get; }

        public GridPosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        // İki pozisyon yan yana mı? (yatay veya dikey komşu)
        public bool IsAdjacent(GridPosition other)
        {
            int dx = Math.Abs(X - other.X);
            int dy = Math.Abs(Y - other.Y);
            return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
        }

        // Komşu pozisyonlar
        public GridPosition Up() => new GridPosition(X, Y + 1);
        public GridPosition Down() => new GridPosition(X, Y - 1);
        public GridPosition Left() => new GridPosition(X - 1, Y);
        public GridPosition Right() => new GridPosition(X + 1, Y);

        // Equality (struct için manuel implement etmek best practice)
        public override bool Equals(object obj) =>
            obj is GridPosition other && X == other.X && Y == other.Y;

        public override int GetHashCode() => HashCode.Combine(X, Y);

        public static bool operator ==(GridPosition a, GridPosition b) => a.Equals(b);
        public static bool operator !=(GridPosition a, GridPosition b) => !a.Equals(b);

        public override string ToString() => $"({X}, {Y})";
    }
}