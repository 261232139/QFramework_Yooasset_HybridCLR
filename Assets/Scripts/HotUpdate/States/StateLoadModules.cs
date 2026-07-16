/****************************************************************************
 * Copyright (c) 2024 liangxiegame UNDER MIT License
 *
 * StateLoadModules — 加载 HybridCLR DLL + 初始化游戏模块
 *
 * 约束: 热更完成后，首次允许调用游戏框架代码（ResKit / UIKit / AudioKit）。
 ****************************************************************************/

using System.Collections;
using UnityEngine;
using QFramework;

namespace HotUpdate
{
    internal class StateLoadModules : AbstractState<HotUpdateState, HotUpdateRunner>
    {
        public StateLoadModules(FSM<HotUpdateState> fsm, HotUpdateRunner target) : base(fsm, target) { }

        protected override void OnEnter()
        {
            Debug.Log("[LoadModules] 加载游戏模块...");
            mTarget.StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            var ctx = Launch.LaunchContext.Instance;

            // 1. 加载 HybridCLR 热更新 DLL（热更资源已就绪，此处才安全加载）
            yield return LoadHotUpdateDll(ctx.HotUpdateDllLocation);

            // 2. 初始化 ResKit
            if (!YooAssetBridge.IsInitialized)
            {
                ResKit.Init();
                Debug.Log("[LoadModules] ResKit 初始化完成");
            }

            // 3. UIKit PanelLoaderPool 由 SupportOldQF.UIKitWithResKitInit 在场景加载前自动设置，无需手动初始化
            Debug.Log("[LoadModules] UIKit 就绪");

            // 4. AudioKit 依赖 ResKit，无需额外初始化
            Debug.Log("[LoadModules] AudioKit 就绪");

            ctx.Progress = 0.9f;
            Debug.Log("[LoadModules] 游戏模块加载完成");
            mFSM.ChangeState(HotUpdateState.EnterLobby);
        }

        private IEnumerator LoadHotUpdateDll(string dllLocation)
        {
#if !ENABLE_HYBRIDCLR || UNITY_EDITOR
            Debug.Log("[LoadModules] HybridCLR 未启用，跳过 DLL 加载");
            yield break;
#endif
            var loader   = ResLoader.Allocate();
            var dllRes   = loader.LoadResSync(
                ResSearchKeys.Allocate(dllLocation, null, typeof(TextAsset)));
            var dllAsset = dllRes?.Asset as TextAsset;

            if (dllAsset == null)
            {
                Debug.LogError($"[LoadModules] 热更新 DLL 加载失败: {dllLocation}");
                loader.Recycle2Cache();
                yield break;
            }

            System.Reflection.Assembly.Load(dllAsset.bytes);
            Debug.Log("[LoadModules] HybridCLR DLL 加载成功");
            loader.Recycle2Cache();
        }
    }
}
