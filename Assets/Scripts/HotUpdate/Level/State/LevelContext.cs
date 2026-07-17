/****************************************************************************
 * Copyright (c) 2024 Game Project UNDER MIT License
 *
 * 关卡数据上下文
 *
 * 职责: 持有关卡运行时的所有共享数据，供各状态访问
 ****************************************************************************/

using System;
using System.Collections.Generic;
using Game.Level.Data;

namespace Game.Level.State
{
    /// <summary>
    /// 关卡数据上下文
    /// 
    /// 在 LevelStateMachine 中创建，各状态通过此对象访问和修改关卡数据。
    /// 支持事件通知状态变化。
    /// </summary>
    public class LevelContext
    {
        /// <summary>关卡配置（只读）</summary>
        public LevelConfig Config { get; set; }

        /// <summary>运行时棋盘数据（一维数组）</summary>
        public List<RuntimeCardSlot> BoardSlots { get; set; } = new List<RuntimeCardSlot>();

        /// <summary>剩余时间（秒）</summary>
        public float RemainingTime { get; set; }

        /// <summary>当前分数</summary>
        public int Score { get; set; }

        // ── 事件通知 ────────────────────────────────────────────────
        /// <summary>卡牌被替换时触发</summary>
        public event Action<int, int, CardData> OnCardReplaced;

        /// <summary>时间变化时触发</summary>
        public event Action<float> OnTimeChanged;

        /// <summary>分数变化时触发</summary>
        public event Action<int> OnScoreChanged;

        public void RaiseCardReplaced(int slotIndex, int x, int y, CardData newCard)
        {
            OnCardReplaced?.Invoke(slotIndex, x * 1000 + y, newCard);
        }

        public void RaiseTimeChanged(float remainingTime)
        {
            RemainingTime = remainingTime;
            OnTimeChanged?.Invoke(remainingTime);
        }

        public void RaiseScoreChanged(int newScore)
        {
            Score = newScore;
            OnScoreChanged?.Invoke(newScore);
        }

        /// <summary>获取指定坐标的运行时格子数据</summary>
        public RuntimeCardSlot GetSlot(int x, int y)
        {
            if (x < 0 || x >= Config.mapWidth || y < 0 || y >= Config.mapHeight)
                return null;

            int index = y * Config.mapWidth + x;
            return BoardSlots[index];
        }

        /// <summary>重置为初始状态</summary>
        public void Reset()
        {
            RemainingTime = Config.duration;
            Score = 0;
            BoardSlots.Clear();
            BoardSlots.ForEach(slot => slot?.Reset());
        }
    }
}
