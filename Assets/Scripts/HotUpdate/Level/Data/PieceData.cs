using System;

namespace Game.Level.Data
{
    public enum PieceType
    {
        Peg = 0,
        Gem = 1,
        Stone = 2,
    }

    [Serializable]
    public struct GridPosition : IEquatable<GridPosition>
    {
        public int x;
        public int y;

        public static readonly GridPosition Invalid = new GridPosition(-1, -1);

        public GridPosition(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public bool Equals(GridPosition other) => x == other.x && y == other.y;
        public override bool Equals(object obj) => obj is GridPosition other && Equals(other);
        public override int GetHashCode() => (x * 397) ^ y;
        public override string ToString() => $"({x}, {y})";
        
        public static bool operator ==(GridPosition a, GridPosition b) => a.Equals(b);
        public static bool operator !=(GridPosition a, GridPosition b) => !a.Equals(b);
    }

    [Serializable]
    public class PieceData
    {
        public string id;
        public PieceType pieceType = PieceType.Peg;
        public bool isMovable;
        public GridPosition position;
    }
}
