/****************************************************************************
 * Copyright (c) 2024 Game Project UNDER MIT License
 *
 * 关卡状态机
 *
 * 管理关卡的整个生命周期
 ****************************************************************************/

using UnityEngine;
using QFramework;
using Game.Level.Data;

namespace Game.Level.State
{
    /// <summary>
    /// 关卡状态机
    /// 
    /// 负责管理关卡从加载到结束的整个流程
    /// </summary>
    public class LevelStateMachine : MonoBehaviour
    {
        public LevelContext Context { get; private set; }
        private FSM<LevelState> mFSM;
        private bool mInitialized;

        private void Awake()
        {
            Context = new LevelContext();
            InitializeStateMachine();
        }

        private void Update()
        {
            mFSM?.Update();
        }

        private void OnDestroy()
        {
            LevelEventManager.Clear();
            mFSM?.Clear();
        }

        #region 公共方法

        /// <summary>
        /// 开始关卡（从大厅进入）
        /// </summary>
        /// <param name="config">关卡配置</param>
        /// <param name="levelNumber">关卡编号</param>
        /// <param name="coroutineHost">协程宿主（用于播放动画等异步操作）</param>
        public void Begin(LevelConfig config, int levelNumber, MonoBehaviour coroutineHost = null)
        {
            if (config == null)
            {
                Debug.LogError("[LevelStateMachine] Cannot begin with a null config.");
                return;
            }

            InitializeStateMachine();
            
            Context.Config = config;
            Context.LevelNumber = levelNumber;
            Context.CoroutineHost = coroutineHost ?? this;
            Context.Pieces.Clear();

            Debug.Log($"[LevelStateMachine] Starting level {levelNumber}: {config.levelId}");
            mFSM.StartState(LevelState.LobbyToLevel);
        }

        /// <summary>
        /// 暂停关卡
        /// </summary>
        public void Pause()
        {
            if (mFSM.CurrentStateId == LevelState.LevelRunning)
            {
                mFSM.ChangeState(LevelState.LevelPause);
            }
        }

        /// <summary>
        /// 恢复关卡
        /// </summary>
        public void Resume()
        {
            if (mFSM.CurrentStateId == LevelState.LevelPause)
            {
                mFSM.ChangeState(LevelState.LevelRunning);
            }
        }

        /// <summary>
        /// 关卡胜利
        /// </summary>
        public void Win()
        {
            if (mFSM.CurrentStateId == LevelState.LevelRunning)
            {
                mFSM.ChangeState(LevelState.LevelSuccess);
            }
        }

        /// <summary>
        /// 关卡失败
        /// </summary>
        public void Fail()
        {
            if (mFSM.CurrentStateId == LevelState.LevelRunning)
            {
                mFSM.ChangeState(LevelState.LevelFail);
            }
        }

        /// <summary>
        /// 重试关卡
        /// </summary>
        public void Retry()
        {
            if (Context.Config != null)
            {
                var config = Context.Config;
                var levelNumber = Context.LevelNumber;
                var host = Context.CoroutineHost;
                
                Context.Clear();
                Begin(config, levelNumber, host);
            }
        }

        /// <summary>
        /// 退出关卡（返回大厅）
        /// </summary>
        public void QuitToLobby()
        {
            mFSM.ChangeState(LevelState.LevelToLobby);
        }

        /// <summary>
        /// 获取当前状态
        /// </summary>
        public LevelState GetCurrentState()
        {
            return mFSM.CurrentStateId;
        }

        /// <summary>
        /// 检查是否在游戏中
        /// </summary>
        public bool IsPlaying()
        {
            return mFSM.CurrentStateId == LevelState.LevelRunning;
        }

        /// <summary>
        /// 检查是否已暂停
        /// </summary>
        public bool IsPaused()
        {
            return mFSM.CurrentStateId == LevelState.LevelPause;
        }

        #endregion

        #region 内部方法

        private void InitializeStateMachine()
        {
            if (mInitialized)
                return;

            mFSM = new FSM<LevelState>();
            
            // 注册所有状态
            mFSM.AddState(LevelState.LobbyToLevel, new StateLobbyToLevel(mFSM, this));
            mFSM.AddState(LevelState.LoadLevel, new StateLoadLevel(mFSM, this));
            mFSM.AddState(LevelState.LevelReady, new StateLevelReady(mFSM, this));
            mFSM.AddState(LevelState.LevelRunning, new StateLevelRunning(mFSM, this));
            mFSM.AddState(LevelState.LevelPause, new StateLevelPause(mFSM, this));
            mFSM.AddState(LevelState.LevelSuccess, new StateLevelSuccess(mFSM, this));
            mFSM.AddState(LevelState.LevelFail, new StateLevelFail(mFSM, this));
            mFSM.AddState(LevelState.LevelToLobby, new StateLevelToLobby(mFSM, this));
            
            mInitialized = true;
            Debug.Log("[LevelStateMachine] State machine initialized with 8 states");
        }

        #endregion
    }
}
