/****************************************************************************
 * Copyright (c) 2024 Game Project UNDER MIT License
 *
 * 棋子接口
 *
 * 定义所有棋子的通用行为
 ****************************************************************************/

using Game.Level.Data;
using UnityEngine;

namespace Game.Level.Runtime
{
    /// <summary>
    /// 移动方向（仅支持四个正方向）
    /// </summary>
    public enum MoveDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    /// <summary>
    /// 移动结果
    /// </summary>
    public struct MoveResult
    {
        public bool IsValid;
        public string ErrorMessage;
        public GridPosition TargetPosition;
        public GridPosition JumpedPosition; // 跨越的棋子位置

        public static MoveResult Success(GridPosition target, GridPosition jumped)
        {
            return new MoveResult
            {
                IsValid = true,
                TargetPosition = target,
                JumpedPosition = jumped
            };
        }

        public static MoveResult Fail(string error)
        {
            return new MoveResult
            {
                IsValid = false,
                ErrorMessage = error
            };
        }
    }

    /// <summary>
    /// 棋子接口
    /// 定义所有棋子必须实现的行为
    /// </summary>
    public interface IPiece
    {
        /// <summary>棋子ID</summary>
        string Id { get; }

        /// <summary>棋子类型</summary>
        PieceType PieceType { get; }

        /// <summary>当前位置</summary>
        GridPosition Position { get; }

        /// <summary>是否可移动</summary>
        bool IsMovable { get; }

        /// <summary>是否被选中</summary>
        bool IsSelected { get; set; }

        /// <summary>
        /// 验证移动是否合法
        /// </summary>
        /// <param name="from">起始位置</param>
        /// <param name="to">目标位置</param>
        /// <param name="board">棋盘引用</param>
        /// <returns>移动结果</returns>
        MoveResult ValidateMove(GridPosition from, GridPosition to, IBoardState board);

        /// <summary>
        /// 执行移动
        /// </summary>
        /// <param name="newPosition">新位置</param>
        void MoveTo(GridPosition newPosition);

        /// <summary>
        /// 重置到初始位置
        /// </summary>
        void Reset();

        /// <summary>
        /// 获取视觉表现对象（用于拖拽等）
        /// </summary>
        GameObject GetVisualObject();
    }

    /// <summary>
    /// 棋盘状态接口
    /// 用于棋子验证移动时查询棋盘信息
    /// </summary>
    public interface IBoardState
    {
        /// <summary>检查位置是否在棋盘内</summary>
        bool IsInBounds(GridPosition position);

        /// <summary>检查位置是否存在格子</summary>
        bool HasCell(GridPosition position);

        /// <summary>获取指定位置的棋子</summary>
        IPiece GetPieceAt(GridPosition position);

        /// <summary>检查位置是否有棋子</summary>
        bool HasPieceAt(GridPosition position);

        /// <summary>棋盘宽度</summary>
        int Width { get; }

        /// <summary>棋盘高度</summary>
        int Height { get; }
    }
}
