/****************************************************************************
 * Copyright (c) 2024 Game Project UNDER MIT License
 *
 * 关卡到大厅过场状态
 *
 * 播放过场动画，清理关卡资源
 ****************************************************************************/

using UnityEngine;
using QFramework;
using Game.Level.Runtime;
using System.Collections;

namespace Game.Level.State
{
    /// <summary>
    /// 关卡→大厅过场状态
    /// 播放过场动画，清理关卡资源
    /// </summary>
    internal class StateLevelToLobby : LevelStateBase
    {
        public StateLevelToLobby(FSM<LevelState> fsm, LevelStateMachine target) : base(fsm, target) { }

        protected override void OnEnter()
        {
            Debug.Log("[LevelState] LevelToLobby - Returning to lobby");
            LevelEventManager.TriggerEvent(LevelEventType.ReturnToLobby, Context.LevelNumber);
            
            // TODO: 播放过场动画
            // TODO: 卸载关卡资源
            
            if (Context.CoroutineHost != null)
                Context.CoroutineHost.StartCoroutine(PlayTransitionAndCleanup());
            else
                CleanupAndReturn();
        }

        private IEnumerator PlayTransitionAndCleanup()
        {
            // TODO: 播放过场动画
            yield return new WaitForSeconds(0.5f);
            
            CleanupAndReturn();
        }

        private void CleanupAndReturn()
        {
            // 清理关卡数据
            Context.ResetPieces();
            Context.Clear();
            
            Debug.Log("[LevelState] LevelToLobby - Cleanup complete");
            
            // 触发返回大厅事件（LobbyController 会监听这个事件）
            LevelEventManager.TriggerEvent(LevelEventType.ReturnedToLobby);
        }
    }
}
