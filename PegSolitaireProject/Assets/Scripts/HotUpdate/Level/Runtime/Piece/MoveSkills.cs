using Game.Level.Data;

namespace Game.Level.Runtime
{
    /// <summary>一次由 MoveSkill 生成的合法跳跃。</summary>
    public readonly struct MoveOption
    {
        public GridPosition From { get; }
        public GridPosition Middle { get; }
        public GridPosition Target { get; }

        public MoveOption(GridPosition from, GridPosition middle, GridPosition target)
        {
            From = from;
            Middle = middle;
            Target = target;
        }
    }

    /// <summary>只负责计算一个棋子的原子移动规则，不处理表现或关卡状态。</summary>
    public interface IMoveSkill
    {
        MoveSkillType Type { get; }
        MoveResult ValidateMove(IPiece piece, GridPosition from, GridPosition to, IBoardState board);
        bool TryGetValidMove(IPiece piece, IBoardState board, out MoveOption move);
    }

    /// <summary>单方向、跨越一枚棋子的标准 Jump Skill。</summary>
    public sealed class JumpMoveSkill : IMoveSkill
    {
        public MoveSkillType Type { get; }

        public JumpMoveSkill(MoveSkillType type)
        {
            Type = type;
        }

        public MoveResult ValidateMove(IPiece piece, GridPosition from, GridPosition to, IBoardState board)
        {
            if (piece == null || board == null)
                return MoveResult.Fail("棋子或棋盘状态不存在");

            if (to != GetTarget(from))
                return MoveResult.Fail($"当前棋子不具备 {Type} 移动能力");

            if (!board.HasCell(to))
                return MoveResult.Fail("目标位置没有格子");

            if (board.HasPieceAt(to))
                return MoveResult.Fail("目标位置已有棋子");

            var middle = GetMiddle(from);
            if (!board.HasCell(middle))
                return MoveResult.Fail("移动路径中间没有有效格子");

            var jumpedPiece = board.GetPieceAt(middle);
            if (jumpedPiece == null)
                return MoveResult.Fail("移动路径中没有可跨越的棋子");

            if (!jumpedPiece.CanBeJumped)
                return MoveResult.Fail("移动路径中的棋子不可被跨越");

            return MoveResult.Success(to, middle);
        }

        public bool TryGetValidMove(IPiece piece, IBoardState board, out MoveOption move)
        {
            var from = piece.Position;
            var target = GetTarget(from);
            var result = ValidateMove(piece, from, target, board);
            if (!result.IsValid)
            {
                move = default;
                return false;
            }

            move = new MoveOption(from, result.JumpedPosition, target);
            return true;
        }

        private GridPosition GetTarget(GridPosition from)
        {
            switch (Type)
            {
                case MoveSkillType.JumpUp: return new GridPosition(from.x, from.y - 2);
                case MoveSkillType.JumpDown: return new GridPosition(from.x, from.y + 2);
                case MoveSkillType.JumpLeft: return new GridPosition(from.x - 2, from.y);
                case MoveSkillType.JumpRight: return new GridPosition(from.x + 2, from.y);
                default: return GridPosition.Invalid;
            }
        }

        private GridPosition GetMiddle(GridPosition from)
        {
            switch (Type)
            {
                case MoveSkillType.JumpUp: return new GridPosition(from.x, from.y - 1);
                case MoveSkillType.JumpDown: return new GridPosition(from.x, from.y + 1);
                case MoveSkillType.JumpLeft: return new GridPosition(from.x - 1, from.y);
                case MoveSkillType.JumpRight: return new GridPosition(from.x + 1, from.y);
                default: return GridPosition.Invalid;
            }
        }
    }

    public static class MoveSkillFactory
    {
        public static IMoveSkill Create(MoveSkillType type) => new JumpMoveSkill(type);
    }
}
