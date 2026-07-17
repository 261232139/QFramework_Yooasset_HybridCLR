/****************************************************************************
 * Copyright (c) 2024 Game Project UNDER MIT License
 *
 * 关卡状态实现
 *
 * 职责: 实现各个关卡状态的具体逻辑
 ****************************************************************************/

using UnityEngine;
using QFramework;
using Game.Level.Data;

namespace Game.Level.State
{
    // ─────────────────────────────────────────────────────────────────────
    // Enter: 加载关卡配置
    // ─────────────────────────────────────────────────────────────────────

    internal class StateEnter : LevelStateBase
    {
        public StateEnter(FSM<LevelState> fsm, LevelStateMachine target) : base(fsm, target) { }

        protected override void OnEnter()
        {
            Debug.Log("[LevelState] Enter: 加载关卡配置...");
            
            var config = LevelConfigLoader.Load(mTarget.LevelId);
            if (config == null)
            {
                Debug.LogError($"[LevelState] 加载关卡 {mTarget.LevelId} 失败");
                mFSM.ChangeState(LevelState.Exit);
                return;
            }

            Context.Config = config;
            Debug.Log($"[LevelState] 关卡配置加载成功: {config.levelId}");
            mFSM.ChangeState(LevelState.Prepare);
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // Prepare: 准备阶段（预留道具逻辑）
    // ─────────────────────────────────────────────────────────────────────

    internal class StatePrepare : LevelStateBase
    {
        public StatePrepare(FSM<LevelState> fsm, LevelStateMachine target) : base(fsm, target) { }

        protected override void OnEnter()
        {
            Debug.Log("[LevelState] Prepare: 准备阶段");
            // TODO: 道具系统初始化
            mFSM.ChangeState(LevelState.Ready);
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // Ready: 初始化棋盘
    // ─────────────────────────────────────────────────────────────────────

    internal class StateReady : LevelStateBase
    {
        public StateReady(FSM<LevelState> fsm, LevelStateMachine target) : base(fsm, target) { }

        protected override void OnEnter()
        {
            Debug.Log("[LevelState] Ready: 初始化棋盘");

            var config = Context.Config;
            Context.BoardSlots.Clear();

            foreach (var slotData in config.cardSlots)
            {
                var runtimeSlot = new RuntimeCardSlot(slotData);
                Context.BoardSlots.Add(runtimeSlot);
            }

            Context.RemainingTime = config.duration;
            Context.Score = 0;

            Debug.Log($"[LevelState] 棋盘初始化完成: {config.mapWidth}x{config.mapHeight}");
            mFSM.ChangeState(LevelState.Playing);
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // Playing: 游戏进行中
    // ─────────────────────────────────────────────────────────────────────

    internal class StatePlaying : LevelStateBase
    {
        private float mDeltaTime;

        public StatePlaying(FSM<LevelState> fsm, LevelStateMachine target) : base(fsm, target) { }

        protected override void OnEnter()
        {
            Debug.Log("[LevelState] Playing: 游戏开始");
            mDeltaTime = 0;
        }

        protected override void OnUpdate()
        {
            mDeltaTime += Time.deltaTime;
            Context.RemainingTime -= Time.deltaTime;

            if (Context.RemainingTime <= 0)
            {
                Context.RemainingTime = 0;
                mFSM.ChangeState(LevelState.Lose);
                return;
            }

            // 每 0.1 秒更新一次时间显示
            if (mDeltaTime >= 0.1f)
            {
                Context.RaiseTimeChanged(Context.RemainingTime);
                mDeltaTime = 0;
            }

            // TODO: 处理玩家拖拽替换、分数计算、胜利判定
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // Pause: 暂停
    // ─────────────────────────────────────────────────────────────────────

    internal class StatePause : LevelStateBase
    {
        public StatePause(FSM<LevelState> fsm, LevelStateMachine target) : base(fsm, target) { }

        protected override void OnEnter()
        {
            Debug.Log("[LevelState] Pause: 游戏暂停");
            Time.timeScale = 0;
        }

        protected override void OnExit()
        {
            Time.timeScale = 1;
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // Win: 胜利
    // ─────────────────────────────────────────────────────────────────────

    internal class StateWin : LevelStateBase
    {
        public StateWin(FSM<LevelState> fsm, LevelStateMachine target) : base(fsm, target) { }

        protected override void OnEnter()
        {
            Debug.Log($"[LevelState] Win: 关卡胜利! 分数: {Context.Score}");
            // TODO: 显示胜利 UI、保存成绩
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // Lose: 失败
    // ─────────────────────────────────────────────────────────────────────

    internal class StateLose : LevelStateBase
    {
        public StateLose(FSM<LevelState> fsm, LevelStateMachine target) : base(fsm, target) { }

        protected override void OnEnter()
        {
            Debug.Log("[LevelState] Lose: 关卡失败");
            // TODO: 显示失败 UI、重试选项
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // Exit: 退出关卡
    // ─────────────────────────────────────────────────────────────────────

    internal class StateExit : LevelStateBase
    {
        public StateExit(FSM<LevelState> fsm, LevelStateMachine target) : base(fsm, target) { }

        protected override void OnEnter()
        {
            Debug.Log("[LevelState] Exit: 退出关卡");
            Context.Reset();
            // TODO: 清理资源、返回菜单
        }
    }
}
