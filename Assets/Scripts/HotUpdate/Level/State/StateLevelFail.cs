/****************************************************************************
 * Copyright (c) 2024 Game Project UNDER MIT License
 *
 * 关卡失败状态
 *
 * 显示失败UI，提供重试选项
 ****************************************************************************/

using UnityEngine;
using QFramework;
using Game.Level.Runtime;
using System.Collections;

namespace Game.Level.State
{
    /// <summary>
    /// 关卡失败状态
    /// 显示失败UI，提供重试选项
    /// </summary>
    internal class StateLevelFail : LevelStateBase
    {
        public StateLevelFail(FSM<LevelState> fsm, LevelStateMachine target) : base(fsm, target) { }

        protected override void OnEnter()
        {
            Debug.Log($"[LevelState] LevelFail - Level {Context.LevelNumber} failed");
            LevelEventManager.TriggerEvent(LevelEventType.LevelLost, Context.LevelNumber);
            
            // TODO: 显示失败UI
            // TODO: 提供重试选项
            // TODO: 播放失败音效
            
            // 自动返回大厅（可以改为等待玩家选择）
            if (Context.CoroutineHost != null)
                Context.CoroutineHost.StartCoroutine(WaitAndReturnToLobby());
        }

        private IEnumerator WaitAndReturnToLobby()
        {
            // 等待一段时间或等待玩家点击
            yield return new WaitForSeconds(3f);
            
            mFSM.ChangeState(LevelState.LevelToLobby);
        }
    }
}
