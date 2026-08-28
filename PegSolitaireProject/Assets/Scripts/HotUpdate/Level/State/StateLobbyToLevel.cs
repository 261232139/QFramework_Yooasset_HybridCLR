/****************************************************************************
 * Copyright (c) 2024 Game Project UNDER MIT License
 *
 * 大厅到关卡过场状态
 *
 * 播放过场动画，隐藏大厅UI
 ****************************************************************************/

using UnityEngine;
using QFramework;
using System.Collections;

namespace Game.Level.State
{
    /// <summary>
    /// 大厅→关卡过场状态
    /// 播放过场动画，隐藏大厅UI
    /// </summary>
    internal class StateLobbyToLevel : LevelStateBase
    {
        public StateLobbyToLevel(FSM<LevelState> fsm, LevelStateMachine target) : base(fsm, target) { }

        protected override void OnEnter()
        {
            Debug.Log($"[LevelState] LobbyToLevel - Starting transition for Level {Context.LevelNumber}");
            
            // TODO: 播放过场动画
            // 这里可以播放淡入淡出、加载画面等
            
            // 过场完成后进入加载状态
            if (Context.CoroutineHost != null)
                Context.CoroutineHost.StartCoroutine(PlayTransitionAnimation());
            else
                mFSM.ChangeState(LevelState.LoadLevel);
        }

        private IEnumerator PlayTransitionAnimation()
        {
            // TODO: 实际的过场动画逻辑
            // 例如：淡出效果、加载画面等
            yield return new WaitForSeconds(0.5f);
            
            mFSM.ChangeState(LevelState.LoadLevel);
        }
    }
}
