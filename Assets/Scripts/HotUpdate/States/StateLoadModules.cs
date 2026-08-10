/****************************************************************************
 * Copyright (c) 2024 liangxiegame UNDER MIT License
 *
 * StateLoadModules — 加载 HybridCLR DLL + 初始化游戏模块
 ****************************************************************************/

using System.Collections;
using UnityEngine;
using QFramework;
using HotUpdate.Game;

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
            var context = Launch.LaunchContext.Instance;
            yield return LoadHotUpdateDll(context.HotUpdateDllLocation);

            if (!YooAssetBridge.IsInitialized)
                YooAssetBridge.BindInitializedPackage(context.DefaultPackage);

            if (!YooAssetBridge.IsInitialized)
            {
                Debug.LogError("[LoadModules] YooAsset 默认资源包未就绪，无法初始化游戏模块。");
                yield break;
            }

            if (!ResMgr.ResMgrInited)
                ResKit.Init();

            GameManager.Instance.Init();

            Debug.Log("[LoadModules] ResKit、UIKit、AudioKit 就绪");
            context.Progress = 0.9f;
            mFSM.ChangeState(HotUpdateState.EnterLobby);
        }

        private IEnumerator LoadHotUpdateDll(string dllLocation)
        {
#if !ENABLE_HYBRIDCLR || UNITY_EDITOR
            Debug.Log("[LoadModules] HybridCLR 未启用，跳过 DLL 加载");
            yield break;
#else
            var loader = ResLoader.Allocate();
            var searchKeys = ResSearchKeys.Allocate(dllLocation, null, typeof(TextAsset));
            var dllRes = loader.LoadResSync(searchKeys);
            searchKeys.Recycle2Cache();
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
#endif
        }
    }
}
