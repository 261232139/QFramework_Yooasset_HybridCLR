using Game.Level.Data;

namespace Game.Level.Runtime
{
    /// <summary>Runtime state for one initial piece. Movement rules are intentionally not implemented here.</summary>
    public class RuntimePiece
    {
        public PieceData Config { get; }
        public GridPosition Position { get; private set; }
        public bool IsMovable => Config.HasMoveSkills;

        public RuntimePiece(PieceData config)
        {
            Config = config;
            Position = config.position;
        }

        public void Reset() => Position = Config.position;
    }
}
