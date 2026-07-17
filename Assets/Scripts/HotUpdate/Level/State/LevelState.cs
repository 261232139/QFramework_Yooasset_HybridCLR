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
    /// Enter   → Prepare → Ready → Playing ↔ Pause → Win/Lose → Exit
    /// </summary>
    public enum LevelState
    {
        /// <summary>进入关卡（加载配置）</summary>
        Enter,

        /// <summary>准备阶段（预留道具逻辑）</summary>
        Prepare,

        /// <summary>就绪（初始化棋盘）</summary>
        Ready,

        /// <summary>游戏进行中（处理玩家操作、计分、倒计时）</summary>
        Playing,

        /// <summary>暂停</summary>
        Pause,

        /// <summary>胜利</summary>
        Win,

        /// <summary>失败</summary>
        Lose,

        /// <summary>退出关卡</summary>
        Exit,
    }
}
