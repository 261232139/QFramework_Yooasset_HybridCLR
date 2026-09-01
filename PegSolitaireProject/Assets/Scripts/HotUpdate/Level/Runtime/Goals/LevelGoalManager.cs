/****************************************************************************
 * Copyright (c) 2024 Game Project UNDER MIT License
 *
 * 关卡目标管理器
 *
 * 负责跟踪和验证关卡目标的完成情况
 ****************************************************************************/

using Game.Level.Data;

namespace Game.Level.Runtime
{
    /// <summary>
    /// 关卡目标类型
    /// </summary>
    public enum LevelGoalType
    {
        ClearAll,           // 清除所有棋子
        RemainOne,          // 只剩一个棋子
        ClearSpecific,      // 清除特定类型的棋子
        MoveCount,          // 限制移动次数
        ScoreTarget         // 达到目标分数
    }

    /// <summary>
    /// 关卡目标数据
    /// </summary>
    [System.Serializable]
    public class LevelGoal
    {
        public LevelGoalType goalType = LevelGoalType.RemainOne;
        public int targetCount = 1;
        public PieceType targetPieceType = PieceType.Normal;
    }

    /// <summary>
    /// 关卡目标管理器（纯 C# 类）
    /// 负责跟踪和验证关卡目标
    /// </summary>
    public class LevelGoalManager
    {
        private readonly LevelGoal currentGoal;
        private BoardStateManager boardState;
        private bool isTracking;
        private int moveCount;
        private int currentScore;

        // 事件
        public event System.Action OnGoalCompleted;
        public event System.Action OnGoalFailed;
        public event System.Action<int> OnScoreChanged;
        public event System.Action<int> OnMoveCountChanged;

        public LevelGoal CurrentGoal => currentGoal;
        public int MoveCount => moveCount;
        public int CurrentScore => currentScore;

        public LevelGoalManager(LevelGoal goal = null)
        {
            currentGoal = goal ?? new LevelGoal();
            moveCount = 0;
            currentScore = 0;
        }

        public void Initialize(BoardStateManager state)
        {
            boardState = state;
            moveCount = 0;
            currentScore = 0;
        }

        public void StartTracking()
        {
            isTracking = true;
            moveCount = 0;
            currentScore = 0;
        }

        public void StopTracking()
        {
            isTracking = false;
        }

        #region 事件处理

        /// <summary>
        /// 当棋子移动时调用
        /// </summary>
        public void OnPieceMoved(IPiece movedPiece, GridPosition from, GridPosition to, IPiece jumpedPiece)
        {
            if (!isTracking)
                return;

            // 增加移动次数
            moveCount++;
            OnMoveCountChanged?.Invoke(moveCount);

            // 计算分数
            if (jumpedPiece != null)
            {
                var score = CalculateScore(jumpedPiece);
                AddScore(score);
            }

            // 检查目标
            CheckGoal();
        }

        #endregion

        #region 目标检查

        /// <summary>
        /// 检查目标是否完成
        /// </summary>
        public bool IsGoalCompleted()
        {
            if (boardState == null)
                return false;

            var remainingPieces = boardState.AllPieces.Count;

            switch (currentGoal.goalType)
            {
                case LevelGoalType.ClearAll:
                    return remainingPieces == 0;

                case LevelGoalType.RemainOne:
                    return remainingPieces == 1;

                case LevelGoalType.ClearSpecific:
                    return CountPiecesByType(currentGoal.targetPieceType) == 0;

                case LevelGoalType.MoveCount:
                    // 移动次数限制，需要在限制内完成其他目标
                    return remainingPieces <= currentGoal.targetCount && moveCount <= currentGoal.targetCount;

                case LevelGoalType.ScoreTarget:
                    return currentScore >= currentGoal.targetCount;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 检查目标是否失败
        /// </summary>
        public bool IsGoalFailed()
        {
            if (boardState == null)
                return false;

            switch (currentGoal.goalType)
            {
                case LevelGoalType.MoveCount:
                    // 超过移动次数限制
                    return moveCount > currentGoal.targetCount;

                default:
                    return false;
            }
        }

        private void CheckGoal()
        {
            if (IsGoalCompleted())
            {
                OnGoalCompleted?.Invoke();
            }
            else if (IsGoalFailed())
            {
                OnGoalFailed?.Invoke();
            }
        }

        #endregion

        #region 分数系统

        private int CalculateScore(IPiece piece)
        {
            return 10;
        }

        private void AddScore(int score)
        {
            currentScore += score;
            OnScoreChanged?.Invoke(currentScore);
        }

        #endregion

        #region 工具方法

        private int CountPiecesByType(PieceType type)
        {
            if (boardState == null)
                return 0;

            var count = 0;
            foreach (var piece in boardState.AllPieces)
            {
                if (piece.PieceType == type)
                    count++;
            }
            return count;
        }

        /// <summary>
        /// 获取目标描述文本
        /// </summary>
        public string GetGoalDescription()
        {
            return currentGoal.goalType switch
            {
                LevelGoalType.ClearAll => "清除所有棋子",
                LevelGoalType.RemainOne => "只剩下一个棋子",
                LevelGoalType.ClearSpecific => $"清除所有{currentGoal.targetPieceType}",
                LevelGoalType.MoveCount => $"在{currentGoal.targetCount}步内完成",
                LevelGoalType.ScoreTarget => $"达到{currentGoal.targetCount}分",
                _ => "未知目标"
            };
        }

        #endregion
    }
}
