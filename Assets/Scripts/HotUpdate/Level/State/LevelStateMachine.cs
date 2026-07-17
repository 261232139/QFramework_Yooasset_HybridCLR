/****************************************************************************
 * Copyright (c) 2024 Game Project UNDER MIT License
 *
 * 关卡状态机
 *
 * 职责: 驱动关卡 FSM，管理关卡生命周期
 ****************************************************************************/

using UnityEngine;
using QFramework;
using Game.Level.Data;

namespace Game.Level.State
{
    /// <summary>
    /// 关卡状态机
    /// 
    /// 管理整个关卡的生命周期：
    /// Enter → Prepare → Ready → Playing ↔ Pause → Win/Lose → Exit
    /// 
    /// 使用方式：
    ///   var fsm = new LevelStateMachine(levelId);
    ///   fsm.StartLevel();
    ///   // 在 Update 中驱动
    ///   fsm.Update();
    /// </summary>
    public class LevelStateMachine : MonoBehaviour
    {
        [SerializeField] private int mLevelId = 1;
        public int LevelId => mLevelId;

        /// <summary>关卡数据上下文</summary>
        public LevelContext Context { get; private set; }

        /// <summary>状态机</summary>
        private FSM<LevelState> mFSM;

        private void Awake()
        {
            Context = new LevelContext();
        }

        private void Start()
        {
            // 初始化状态机
            mFSM = new FSM<LevelState>();

            mFSM.AddState(LevelState.Enter,   new StateEnter(mFSM, this));
            mFSM.AddState(LevelState.Prepare, new StatePrepare(mFSM, this));
            mFSM.AddState(LevelState.Ready,   new StateReady(mFSM, this));
            mFSM.AddState(LevelState.Playing, new StatePlaying(mFSM, this));
            mFSM.AddState(LevelState.Pause,   new StatePause(mFSM, this));
            mFSM.AddState(LevelState.Win,     new StateWin(mFSM, this));
            mFSM.AddState(LevelState.Lose,    new StateLose(mFSM, this));
            mFSM.AddState(LevelState.Exit,    new StateExit(mFSM, this));

            mFSM.OnStateChanged((prev, next) =>
                Debug.Log($"[LevelFSM] State: {prev} → {next}"));

            StartLevel();
        }

        private void Update()
        {
            mFSM?.Update();
        }

        private void OnDestroy()
        {
            mFSM?.Clear();
        }

        /// <summary>启动关卡</summary>
        public void StartLevel()
        {
            Debug.Log($"[LevelFSM] 启动关卡 {mLevelId}");
            mFSM.StartState(LevelState.Enter);
        }

        /// <summary>暂停游戏</summary>
        public void Pause()
        {
            if (mFSM.CurrentStateId == LevelState.Playing)
                mFSM.ChangeState(LevelState.Pause);
        }

        /// <summary>恢复游戏</summary>
        public void Resume()
        {
            if (mFSM.CurrentStateId == LevelState.Pause)
                mFSM.ChangeState(LevelState.Playing);
        }

        /// <summary>退出关卡</summary>
        public void QuitLevel()
        {
            mFSM.ChangeState(LevelState.Exit);
        }

        /// <summary>获取当前状态</summary>
        public LevelState GetCurrentState() => mFSM.CurrentStateId;
    }
}
