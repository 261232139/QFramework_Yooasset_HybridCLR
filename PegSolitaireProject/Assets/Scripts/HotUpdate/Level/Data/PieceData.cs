using System;
using System.Collections.Generic;

namespace Game.Level.Data
{
    public enum PieceType
    {
        Normal = 0
    }

    /// <summary>V1 原子跳跃能力。棋子的实际移动能力由此列表组合决定。</summary>
    public enum MoveSkillType
    {
        JumpUp,
        JumpDown,
        JumpLeft,
        JumpRight
    }

    [Serializable]
    public struct GridPosition : IEquatable<GridPosition>
    {
        public int x;
        public int y;
        public static readonly GridPosition Invalid = new GridPosition(-1, -1);
        public GridPosition(int x, int y) { this.x = x; this.y = y; }
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
        public PieceType pieceType = PieceType.Normal;
        public List<MoveSkillType> moveSkills = new List<MoveSkillType>
        {
            MoveSkillType.JumpUp, MoveSkillType.JumpDown,
            MoveSkillType.JumpLeft, MoveSkillType.JumpRight
        };
        public bool canBeJumped = true;
        public bool isRescueTarget;
        public GridPosition position;

        public bool HasMoveSkills => moveSkills != null && moveSkills.Count > 0;
    }
}
