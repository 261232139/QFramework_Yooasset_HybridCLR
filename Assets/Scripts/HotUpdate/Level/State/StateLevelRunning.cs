/****************************************************************************
 * Copyright (c) 2024 Game Project UNDER MIT License
 *
 * 关卡运行状态
 *
 * 游戏主循环，处理玩家输入、逻辑更新等
 ****************************************************************************/

using UnityEngine;
using QFramework;
using Game.Level.Runtime;

namespace Game.Level.State
{
    /// <summary>
    /// 关卡运行状态
    /// 游戏主循环，处理玩家输入、逻辑更新等
    /// </summary>
    internal class StateLevelRunning : LevelStateBase
    {
        public StateLevelRunning(FSM<LevelState> fsm, LevelStateMachine target) : base(fsm, target) { }

        protected override void OnEnter()
        {
            Debug.Log("[LevelState] LevelRunning - Game started");
            LevelEventManager.TriggerEvent(LevelEventType.LevelStart, Context.LevelNumber);
        }

        protected override void OnUpdate()
        {
            // TODO: 游戏主循环逻辑
            // 例如：检测玩家输入
            // 例如：更新计时器
            // 例如：检测胜利/失败条件
        }

        protected override void OnExit()
        {
            Debug.Log("[LevelState] LevelRunning - Game paused or ended");
        }
    }
}
