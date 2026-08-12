/****************************************************************************
 * Copyright (c) 2024 Game Project UNDER MIT License
 *
 * 棋子基类
 *
 * 实现棋子的通用逻辑和移动规则验证
 ****************************************************************************/

using Game.Level.Data;
using UnityEngine;

namespace Game.Level.Runtime
{
    /// <summary>
    /// 棋子基类
    /// 实现通用的移动规则验证逻辑
    /// </summary>
    public abstract class PieceBase : IPiece
    {
        protected readonly PieceData mConfig;
        protected GridPosition mCurrentPosition;
        protected bool mIsSelected;
        protected GameObject mVisualObject;

        public string Id => mConfig.id;
        public PieceType PieceType => mConfig.pieceType;
        public GridPosition Position => mCurrentPosition;
        public bool IsMovable => mConfig.isMovable;
        public bool IsSelected
        {
            get => mIsSelected;
            set => mIsSelected = value;
        }

        protected PieceBase(PieceData config, GameObject visualObject = null)
        {
            mConfig = config;
            mCurrentPosition = config.position;
            mVisualObject = visualObject;
        }

        #region 移动验证

        /// <summary>
        /// 验证移动是否合法（模板方法）
        /// </summary>
        public virtual MoveResult ValidateMove(GridPosition from, GridPosition to, IBoardState board)
        {
            if (board == null)
                return MoveResult.Fail("棋盘状态不存在");

            // 基础检查
            if (!IsMovable)
                return MoveResult.Fail("棋子不可移动");

            if (!from.Equals(Position) || board.GetPieceAt(from) != this)
                return MoveResult.Fail("起点不存在该棋子");

            if (from.Equals(to))
                return MoveResult.Fail("起点和终点相同");

            // 检查目标位置是否在棋盘内
            if (!board.IsInBounds(to))
                return MoveResult.Fail("目标位置超出棋盘范围");

            if (!board.HasCell(to))
                return MoveResult.Fail("目标位置没有格子");

            // 检查目标位置是否已有棋子
            if (board.HasPieceAt(to))
                return MoveResult.Fail("目标位置已有棋子");

            // 检查移动方向（仅支持上下左右）
            var direction = GetMoveDirection(from, to);
            if (!direction.HasValue)
                return MoveResult.Fail("移动方向无效（仅支持上下左右）");

            // 检查移动距离（必须刚好跨越一个棋子）
            var jumpValidation = ValidateJumpMove(from, to, direction.Value, board);
            if (!jumpValidation.IsValid)
                return jumpValidation;

            // 子类可以覆盖此方法添加额外验证
            return ValidateMoveCustom(from, to, board, jumpValidation);
        }

        /// <summary>
        /// 子类可以覆盖此方法添加特殊移动规则
        /// </summary>
        protected virtual MoveResult ValidateMoveCustom(GridPosition from, GridPosition to, IBoardState board, MoveResult baseResult)
        {
            return baseResult;
        }

        /// <summary>
        /// 验证跳跃移动：必须刚好跨越一个棋子
        /// </summary>
        private MoveResult ValidateJumpMove(GridPosition from, GridPosition to, MoveDirection direction, IBoardState board)
        {
            // 计算移动距离
            var distance = CalculateDistance(from, to, direction);
            
            // 距离必须为2（刚好跨越一个棋子）
            if (distance != 2)
                return MoveResult.Fail($"移动距离错误：必须刚好跨越一个棋子（当前距离：{distance}）");

            // 获取中间位置（被跨越的棋子位置）
            var middlePosition = GetMiddlePosition(from, to, direction);

            if (!board.HasCell(middlePosition))
                return MoveResult.Fail("移动路径中间没有有效格子");

            // 中间位置必须有棋子
            if (!board.HasPieceAt(middlePosition))
                return MoveResult.Fail("移动路径中没有可跨越的棋子");

            // 移动合法
            return MoveResult.Success(to, middlePosition);
        }

        /// <summary>
        /// 获取移动方向
        /// </summary>
        protected MoveDirection? GetMoveDirection(GridPosition from, GridPosition to)
        {
            var dx = to.x - from.x;
            var dy = to.y - from.y;

            // 必须是单一方向移动（横向或纵向，不能斜向）
            if (dx != 0 && dy != 0)
                return null;

            if (dx == 0 && dy == 0)
                return null;

            // 判断方向
            if (dy > 0) return MoveDirection.Down;  // Unity UI: Y增大向下
            if (dy < 0) return MoveDirection.Up;    // Unity UI: Y减小向上
            if (dx > 0) return MoveDirection.Right;
            if (dx < 0) return MoveDirection.Left;

            return null;
        }

        /// <summary>
        /// 计算指定方向上的移动距离
        /// </summary>
        protected int CalculateDistance(GridPosition from, GridPosition to, MoveDirection direction)
        {
            switch (direction)
            {
                case MoveDirection.Up:
                case MoveDirection.Down:
                    return Mathf.Abs(to.y - from.y);
                case MoveDirection.Left:
                case MoveDirection.Right:
                    return Mathf.Abs(to.x - from.x);
                default:
                    return 0;
            }
        }

        /// <summary>
        /// 获取两个位置之间的中间位置
        /// </summary>
        protected GridPosition GetMiddlePosition(GridPosition from, GridPosition to, MoveDirection direction)
        {
            switch (direction)
            {
                case MoveDirection.Up:
                    return new GridPosition(from.x, from.y - 1);
                case MoveDirection.Down:
                    return new GridPosition(from.x, from.y + 1);
                case MoveDirection.Left:
                    return new GridPosition(from.x - 1, from.y);
                case MoveDirection.Right:
                    return new GridPosition(from.x + 1, from.y);
                default:
                    return from;
            }
        }

        #endregion

        #region 移动执行

        public virtual void MoveTo(GridPosition newPosition)
        {
            mCurrentPosition = newPosition;
            OnPositionChanged(newPosition);
        }

        /// <summary>
        /// 位置改变时的回调（子类可覆盖以更新视觉表现）
        /// </summary>
        protected virtual void OnPositionChanged(GridPosition newPosition)
        {
            // 子类可以在这里更新视觉对象的位置
        }

        public virtual void Reset()
        {
            mCurrentPosition = mConfig.position;
            OnPositionChanged(mCurrentPosition);
        }

        #endregion

        public GameObject GetVisualObject() => mVisualObject;

        public void SetVisualObject(GameObject obj) => mVisualObject = obj;
    }
}
