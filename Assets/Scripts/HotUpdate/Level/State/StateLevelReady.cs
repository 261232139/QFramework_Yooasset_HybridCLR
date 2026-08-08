/****************************************************************************
 * Copyright (c) 2024 Game Project UNDER MIT License
 *
 * 关卡就绪状态
 *
 * 关卡加载完成，初始化游戏数据，可能显示倒计时等
 ****************************************************************************/

using UnityEngine;
using QFramework;
using Game.Level.Runtime;
using System.Collections;

namespace Game.Level.State
{
    /// <summary>
    /// 关卡就绪状态
    /// 关卡加载完成，初始化游戏数据，可能显示倒计时等
    /// </summary>
    internal class StateLevelReady : LevelStateBase
    {
        public StateLevelReady(FSM<LevelState> fsm, LevelStateMachine target) : base(fsm, target) { }

        protected override void OnEnter()
        {
            Debug.Log("[LevelState] LevelReady - Initializing game data");
            
            // 初始化 RuntimePiece（为了兼容性保留）
            Context.Pieces.Clear();
            foreach (var pieceData in Context.Config.pieces)
                Context.Pieces.Add(new RuntimePiece(pieceData));

            Debug.Log($"[LevelState] Board ready: {Context.Config.board.width}x{Context.Config.board.height}; pieces: {Context.Pieces.Count}");
            
            LevelEventManager.TriggerEvent(LevelEventType.LevelReady, Context.LevelNumber);

            if (Context.CoroutineHost != null)
                Context.CoroutineHost.StartCoroutine(ReadyCountdown());
            else
                StartGame();
        }

        private IEnumerator ReadyCountdown()
        {
            // TODO: 显示倒计时 3...2...1...GO!
            yield return new WaitForSeconds(0.5f);
            
            StartGame();
        }

        private void StartGame()
        {
            // 启动 LevelView 的游戏
            var levelView = Object.FindFirstObjectByType<LevelView>();
            if (levelView != null)
            {
                levelView.StartLevel();
                Debug.Log("[LevelState] LevelView game started");
            }
            else
            {
                Debug.LogWarning("[LevelState] LevelView not found!");
            }
            
            mFSM.ChangeState(LevelState.LevelRunning);
        }
    }
}
