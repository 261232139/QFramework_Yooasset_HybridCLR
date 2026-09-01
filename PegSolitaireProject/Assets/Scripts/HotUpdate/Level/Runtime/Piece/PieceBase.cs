using System.Collections.Generic;
using Game.Level.Data;
using UnityEngine;

namespace Game.Level.Runtime
{
    /// <summary>棋子的运行时状态与 MoveSkill 组合；不负责 UI 或关卡流程。</summary>
    public abstract class PieceBase : IPiece
    {
        protected readonly PieceData mConfig;
        protected GridPosition mCurrentPosition;
        protected bool mIsSelected;
        protected GameObject mVisualObject;
        private readonly List<IMoveSkill> mMoveSkills;
        private readonly PieceTrait mTraits;

        public string Id => mConfig.id;
        public PieceType PieceType => mConfig.pieceType;
        public GridPosition Position => mCurrentPosition;
        public bool IsMovable => mMoveSkills.Count > 0;
        public IReadOnlyList<IMoveSkill> MoveSkills => mMoveSkills;
        public PieceTrait Traits => mTraits;
        public bool CanBeJumped => mTraits.CanBeJumped;
        public bool IsRescueTarget => mTraits.IsRescueTarget;
        public bool IsSelected { get => mIsSelected; set => mIsSelected = value; }

        protected PieceBase(PieceData config, GameObject visualObject = null)
        {
            mConfig = config;
            mCurrentPosition = config.position;
            mVisualObject = visualObject;
            mTraits = new PieceTrait(config.canBeJumped, config.isRescueTarget);
            mMoveSkills = CreateMoveSkills(config);
        }

        public virtual MoveResult ValidateMove(GridPosition from, GridPosition to, IBoardState board)
        {
            if (board == null)
                return MoveResult.Fail("棋盘状态不存在");
            if (!IsMovable)
                return MoveResult.Fail("棋子不可移动");
            if (!from.Equals(Position) || board.GetPieceAt(from) != this)
                return MoveResult.Fail("起点不存在该棋子");
            if (from.Equals(to))
                return MoveResult.Fail("起点和终点相同");
            if (!board.IsInBounds(to))
                return MoveResult.Fail("目标位置超出棋盘范围");

            foreach (var skill in mMoveSkills)
            {
                var validation = skill.ValidateMove(this, from, to, board);
                if (validation.IsValid)
                    return ValidateMoveCustom(from, to, board, validation);
            }

            return MoveResult.Fail("目标位置不符合当前棋子的移动能力或跳跃规则");
        }

        public IEnumerable<MoveOption> GetValidMoves(IBoardState board)
        {
            if (board == null || !IsMovable)
                yield break;

            foreach (var skill in mMoveSkills)
            {
                if (skill.TryGetValidMove(this, board, out var move))
                    yield return move;
            }
        }

        protected virtual MoveResult ValidateMoveCustom(GridPosition from, GridPosition to, IBoardState board, MoveResult baseResult) => baseResult;

        private static List<IMoveSkill> CreateMoveSkills(PieceData config)
        {
            var skillTypes = config.moveSkills;
            if (skillTypes == null)
            {
                skillTypes = config.isMovable
                    ? new List<MoveSkillType> { MoveSkillType.JumpUp, MoveSkillType.JumpDown, MoveSkillType.JumpLeft, MoveSkillType.JumpRight }
                    : new List<MoveSkillType>();
            }

            var skills = new List<IMoveSkill>(skillTypes.Count);
            foreach (var skillType in skillTypes)
                skills.Add(MoveSkillFactory.Create(skillType));
            return skills;
        }

        public virtual void MoveTo(GridPosition newPosition)
        {
            mCurrentPosition = newPosition;
            OnPositionChanged(newPosition);
        }

        protected virtual void OnPositionChanged(GridPosition newPosition) { }

        public virtual void Reset()
        {
            mCurrentPosition = mConfig.position;
            OnPositionChanged(mCurrentPosition);
        }

        public GameObject GetVisualObject() => mVisualObject;
        public void SetVisualObject(GameObject obj) => mVisualObject = obj;
    }
}
