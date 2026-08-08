/****************************************************************************
 * Copyright (c) 2024 liangxiegame UNDER MIT License
 *
 * StateLaunch — YooAsset 全局初始化 + 资源包就绪
 ****************************************************************************/

using System.Collections;
using UnityEngine;
using QFramework;
using YooAsset;

namespace Launch
{
    internal class StateLaunch : AbstractState<LaunchState, GameLauncher>
    {
        public StateLaunch(FSM<LaunchState> fsm, GameLauncher target) : base(fsm, target) { }

        protected override void OnEnter()
        {
            Debug.Log("[Launch] 初始化 YooAsset...");
            mTarget.StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            var context = LaunchContext.Instance;

            if (!YooAssets.IsInitialized)
                YooAssets.Initialize();

            if (!YooAssets.TryGetPackage(context.PackageName, out var package))
                package = YooAssets.CreatePackage(context.PackageName);

            if (package.InitializeStatus == EOperationStatus.None)
            {
                var operation = package.InitializePackageAsync(
                    YooAssetBridge.CreateDefaultInitOptions(context.PackageName));
                yield return operation;

                if (operation.Status != EOperationStatus.Succeeded)
                {
                    Debug.LogError($"[Launch] 资源包初始化失败: {operation.Error}");
                    yield break;
                }
            }
            else
            {
                while (package.InitializeStatus == EOperationStatus.Processing)
                    yield return null;

                if (package.InitializeStatus != EOperationStatus.Succeeded)
                {
                    Debug.LogError("[Launch] 资源包初始化失败（外部发起）");
                    yield break;
                }
            }

            context.DefaultPackage = package;
            YooAssetBridge.BindInitializedPackage(package);
            context.Progress = 0.1f;
            Debug.Log("[Launch] 资源包就绪");
            mFSM.ChangeState(LaunchState.HotCheckVersion);
        }
    }
}
