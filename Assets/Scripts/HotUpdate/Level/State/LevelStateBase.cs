/****************************************************************************
 * Copyright (c) 2024 Game Project UNDER MIT License
 *
 * 关卡状态基类
 *
 * 职责: 定义所有关卡状态的通用接口
 ****************************************************************************/

using QFramework;

namespace Game.Level.State
{
    /// <summary>
    /// 关卡状态基类
    /// 
    /// 所有关卡状态继承此类，实现具体的状态逻辑。
    /// </summary>
    public abstract class LevelStateBase : AbstractState<LevelState, LevelStateMachine>
    {
        protected LevelContext Context => mTarget.Context;

        public LevelStateBase(FSM<LevelState> fsm, LevelStateMachine target) : base(fsm, target) { }
    }
}
