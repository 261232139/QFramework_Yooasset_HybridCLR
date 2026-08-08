/****************************************************************************
 * Copyright (c) 2024 Game Project UNDER MIT License
 *
 * 关卡逻辑控制器
 *
 * 纯逻辑控制器，不依赖 Unity MonoBehaviour
 ****************************************************************************/

using UnityEngine;
using Game.Level.Data;

namespace Game.Level.Runtime
{
    /// <summary>
    /// 关卡逻辑控制器（纯 C# 类）
    /// 负责游戏逻辑、状态管理、规则验证
    /// </summary>
    public class LevelController
    {
        private readonly LevelConfig config;
        private readonly BoardStateManager boardState;
        private readonly LevelGoalManager goalManager;

        public LevelConfig Config => config;
        public BoardStateManager BoardState => boardState;
        public LevelGoalManager GoalManager => goalManager;

        public LevelController(LevelConfig levelConfig, LevelGoalManager goalMgr)
        {
            config = levelConfig;
            goalManager = goalMgr;
            boardState = new BoardStateManager(config);

            Debug.Log($"[LevelController] Created for level: {config.levelId}");
        }

        /// <summary>
        /// 验证移动是否合法
        /// </summary>
        public bool ValidateMove(IPiece piece, GridPosition to)
        {
            if (piece == null)
                return false;

            var from = piece.Position;
            var validation = piece.ValidateMove(from, to, boardState);
            
            return validation.IsValid;
        }

        /// <summary>
        /// 执行移动
        /// </summary>
        public MoveExecutionResult ExecuteMove(IPiece piece, GridPosition to)
        {
            if (piece == null)
                return MoveExecutionResult.CreateFailure(null, GridPosition.Invalid, to, "棋子为空");

            var from = piece.Position;

            // 验证移动
            var validation = piece.ValidateMove(from, to, boardState);
            if (!validation.IsValid)
            {
                return MoveExecutionResult.CreateFailure(piece, from, to, validation.ErrorMessage);
            }

            // 获取被跨越的棋子
            IPiece jumpedPiece = null;
            if (validation.JumpedPosition != GridPosition.Invalid)
            {
                jumpedPiece = boardState.GetPieceAt(validation.JumpedPosition);
            }

            // 执行移动
            boardState.MovePiece(piece, to);

            // 移除被跨越的棋子
            if (jumpedPiece != null)
            {
                boardState.RemovePiece(jumpedPiece.Position);
            }

            // 更新目标
            goalManager.OnPieceMoved(piece, from, to, jumpedPiece);

            Debug.Log($"[LevelController] Move executed: {piece.Id} from {from} to {to}");

            return MoveExecutionResult.CreateSuccess(piece, from, to, jumpedPiece);
        }

        /// <summary>
        /// 检查游戏是否结束
        /// </summary>
        public bool CheckGameOver()
        {
            return !boardState.HasMovablePieces();
        }

        /// <summary>
        /// 检查目标是否完成
        /// </summary>
        public bool IsGoalCompleted()
        {
            return goalManager.IsGoalCompleted();
        }

        /// <summary>
        /// 检查目标是否失败
        /// </summary>
        public bool IsGoalFailed()
        {
            return goalManager.IsGoalFailed();
        }

        /// <summary>
        /// 获取目标状态
        /// </summary>
        public GoalStatus GetGoalStatus()
        {
            return new GoalStatus
            {
                IsCompleted = goalManager.IsGoalCompleted(),
                IsFailed = goalManager.IsGoalFailed(),
                MoveCount = goalManager.MoveCount,
                CurrentScore = goalManager.CurrentScore,
                RemainingPieces = boardState.AllPieces.Count,
                Description = goalManager.GetGoalDescription()
            };
        }

        /// <summary>
        /// 重置关卡
        /// </summary>
        public void Reset()
        {
            boardState.ResetAllPieces();
            goalManager.StartTracking();
            
            Debug.Log("[LevelController] Level reset");
        }
    }
}
