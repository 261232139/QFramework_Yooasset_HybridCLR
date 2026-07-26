/****************************************************************************
 * Copyright (c) 2024 liangxiegame UNDER MIT License
 *
 * StateEnterLobby — 调用热更侧入口方法 / 打开大厅 UI
 ****************************************************************************/

using UnityEngine;
using QFramework;

namespace HotUpdate
{
    internal class StateEnterLobby : AbstractState<HotUpdateState, HotUpdateRunner>
    {
        public StateEnterLobby(FSM<HotUpdateState> fsm, HotUpdateRunner target) : base(fsm, target) { }

        protected override void OnEnter()
        {
            var ctx = Launch.LaunchContext.Instance;
            ctx.Progress = 1f;
            Debug.Log("[EnterLobby] 启动完成，进入大厅");

            if (Launch.LoadingUI.Instance != null)
            {
                Launch.LoadingUI.Instance.Hide();
                Debug.Log("[EnterLobby] Loading UI 已隐藏");
            }

            // 优先尝试调用热更侧入口（HybridCLR 场景）
            try
            {
                var entryType = System.Type.GetType(ctx.HotUpdateEntryMethod);
                if (entryType != null)
                {
                    var mainMethod = entryType.GetMethod("Main");
                    if (mainMethod != null)
                    {
                        mainMethod.Invoke(null, null);
                        Debug.Log("[EnterLobby] 热更新入口 Main() 调用成功");
                        return;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[EnterLobby] 热更新入口调用失败: {e.Message}");
            }

            // 无热更 DLL 时直接打开大厅 UI
            // UIKit.OpenPanel<UILobbyPanel>();
        }
    }
}
