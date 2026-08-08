/****************************************************************************
 * Copyright (c) 2024 Game Project UNDER MIT License
 *
 * 关卡事件系统
 *
 * 用于关卡状态机与外部系统（如UI、音效等）通信
 ****************************************************************************/

using System;

namespace Game.Level.State
{
    /// <summary>
    /// 关卡事件类型
    /// </summary>
    public enum LevelEventType
    {
        /// <summary>关卡开始加载</summary>
        LevelLoadStart,
        
        /// <summary>关卡加载完成</summary>
        LevelLoadComplete,
        
        /// <summary>关卡就绪（可以开始游戏）</summary>
        LevelReady,
        
        /// <summary>关卡开始运行</summary>
        LevelStart,
        
        /// <summary>关卡暂停</summary>
        LevelPaused,
        
        /// <summary>关卡恢复</summary>
        LevelResumed,
        
        /// <summary>关卡胜利</summary>
        LevelWon,
        
        /// <summary>关卡失败</summary>
        LevelLost,
        
        /// <summary>准备返回大厅</summary>
        ReturnToLobby,
        
        /// <summary>已返回大厅</summary>
        ReturnedToLobby,
    }

    /// <summary>
    /// 关卡事件数据
    /// </summary>
    public class LevelEventArgs
    {
        public LevelEventType EventType { get; }
        public int LevelNumber { get; }
        public object Data { get; }

        public LevelEventArgs(LevelEventType eventType, int levelNumber = 0, object data = null)
        {
            EventType = eventType;
            LevelNumber = levelNumber;
            Data = data;
        }
    }

    /// <summary>
    /// 关卡事件管理器
    /// </summary>
    public static class LevelEventManager
    {
        public static event Action<LevelEventArgs> OnLevelEvent;

        public static void TriggerEvent(LevelEventType eventType, int levelNumber = 0, object data = null)
        {
            OnLevelEvent?.Invoke(new LevelEventArgs(eventType, levelNumber, data));
        }

        public static void Clear()
        {
            OnLevelEvent = null;
        }
    }
}
