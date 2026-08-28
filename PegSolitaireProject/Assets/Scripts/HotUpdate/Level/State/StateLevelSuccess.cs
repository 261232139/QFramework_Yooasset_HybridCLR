/****************************************************************************
 * Copyright (c) 2024 Game Project UNDER MIT License
 *
 * 关卡胜利状态
 *
 * 显示胜利UI，结算奖励等
 ****************************************************************************/

using UnityEngine;
using QFramework;
using Game.Level.Runtime;
using System.Collections;

namespace Game.Level.State
{
    /// <summary>
    /// 关卡胜利状态
    /// 显示胜利UI，结算奖励等
    /// </summary>
    internal class StateLevelSuccess : LevelStateBase
    {
        public StateLevelSuccess(FSM<LevelState> fsm, LevelStateMachine target) : base(fsm, target) { }

        protected override void OnEnter()
        {
            Debug.Log($"[LevelState] LevelSuccess - Level {Context.LevelNumber} completed!");
            LevelEventManager.TriggerEvent(LevelEventType.LevelWon, Context.LevelNumber);
            
            // TODO: 显示胜利UI
            // TODO: 结算奖励（金币、星星等）
            // TODO: 播放胜利音效/动画
            
            // 自动返回大厅（可以改为等待玩家点击）
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
