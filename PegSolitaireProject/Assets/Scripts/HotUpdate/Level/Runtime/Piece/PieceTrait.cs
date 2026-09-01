namespace Game.Level.Runtime
{
    /// <summary>描述其他棋子可以对当前棋子执行的操作。</summary>
    public sealed class PieceTrait
    {
        public bool CanBeJumped { get; }
        public bool IsRescueTarget { get; }

        public PieceTrait(bool canBeJumped, bool isRescueTarget)
        {
            CanBeJumped = canBeJumped;
            IsRescueTarget = isRescueTarget;
        }
    }
}
