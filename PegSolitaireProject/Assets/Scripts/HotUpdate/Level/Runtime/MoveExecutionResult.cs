/****************************************************************************
 * Copyright (c) 2024 Game Project UNDER MIT License
 *
 * 移动执行结果数据结构
 *
 * 封装棋子移动执行后的结果信息
 ****************************************************************************/

using Game.Level.Data;

namespace Game.Level.Runtime
{
    /// <summary>
    /// 移动执行结果（用于 LevelController）
    /// 注意：这与 IPiece 中的 MoveResult（验证结果）不同
    /// </summary>
    public class MoveExecutionResult
    {
        public bool Success { get; }
        public IPiece MovedPiece { get; }
        public GridPosition From { get; }
        public GridPosition To { get; }
        public IPiece JumpedPiece { get; }
        public string ErrorMessage { get; }

        private MoveExecutionResult(bool success, IPiece movedPiece, GridPosition from, GridPosition to, 
            IPiece jumpedPiece, string errorMessage)
        {
            Success = success;
            MovedPiece = movedPiece;
            From = from;
            To = to;
            JumpedPiece = jumpedPiece;
            ErrorMessage = errorMessage;
        }

        public static MoveExecutionResult CreateSuccess(IPiece movedPiece, GridPosition from, GridPosition to, IPiece jumpedPiece)
        {
            return new MoveExecutionResult(true, movedPiece, from, to, jumpedPiece, null);
        }

        public static MoveExecutionResult CreateFailure(IPiece movedPiece, GridPosition from, GridPosition to, string error)
        {
            return new MoveExecutionResult(false, movedPiece, from, to, null, error);
        }
    }

    /// <summary>
    /// 目标状态
    /// </summary>
    public class GoalStatus
    {
        public bool IsCompleted { get; set; }
        public bool IsFailed { get; set; }
        public int MoveCount { get; set; }
        public int CurrentScore { get; set; }
        public int RemainingPieces { get; set; }
        public string Description { get; set; }
    }
}
