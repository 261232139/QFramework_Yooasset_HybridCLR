/****************************************************************************
 * Copyright (c) 2024 Game Project UNDER MIT License
 *
 * 关卡状态枚举
 *
 * 定义关卡 FSM 的所有状态
 ****************************************************************************/

namespace Game.Level.State
{
    /// <summary>
    /// 关卡状态枚举
    /// 
    /// 流程: LobbyToLevel → LoadLevel → LevelReady → LevelRunning ↔ LevelPause 
    ///       → LevelSuccess/LevelFail → LevelToLobby
    /// </summary>
    public enum LevelState
    {
        /// <summary>大厅→关卡（过场动画）</summary>
        LobbyToLevel,

        /// <summary>加载关卡资源和配置</summary>
        LoadLevel,

        /// <summary>关卡就绪（初始化完成，可能有额外操作）</summary>
        LevelReady,

        /// <summary>关卡进行中</summary>
        LevelRunning,

        /// <summary>关卡暂停</summary>
        LevelPause,

        /// <summary>关卡胜利</summary>
        LevelSuccess,

        /// <summary>关卡失败</summary>
        LevelFail,

        /// <summary>关卡→大厅（过场动画）</summary>
        LevelToLobby,
    }
}
