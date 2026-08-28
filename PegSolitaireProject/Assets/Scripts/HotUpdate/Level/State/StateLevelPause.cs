/****************************************************************************
 * Copyright (c) 2024 Game Project UNDER MIT License
 *
 * 关卡暂停状态
 *
 * 暂停游戏，显示暂停菜单
 ****************************************************************************/

using UnityEngine;
using QFramework;
using Game.Level.Runtime;

namespace Game.Level.State
{
    /// <summary>
    /// 关卡暂停状态
    /// 暂停游戏，显示暂停菜单
    /// </summary>
    internal class StateLevelPause : LevelStateBase
    {
        public StateLevelPause(FSM<LevelState> fsm, LevelStateMachine target) : base(fsm, target) { }

        protected override void OnEnter()
        {
            Debug.Log("[LevelState] LevelPause - Game paused");
            Time.timeScale = 0f;
            LevelEventManager.TriggerEvent(LevelEventType.LevelPaused, Context.LevelNumber);
            
            // TODO: 显示暂停菜单
        }

        protected override void OnExit()
        {
            Time.timeScale = 1f;
            LevelEventManager.TriggerEvent(LevelEventType.LevelResumed, Context.LevelNumber);
            Debug.Log("[LevelState] LevelPause - Game resumed");
        }
    }
}
