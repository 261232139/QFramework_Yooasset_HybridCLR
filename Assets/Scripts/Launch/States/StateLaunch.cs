/****************************************************************************
 * Copyright (c) 2024 liangxiegame UNDER MIT License
 *
 * StateLaunch — YooAsset 全局初始化 + 资源包就绪
 *
 * 约束: 此状态内不得调用任何游戏业务代码。
 *       只允许使用 YooAsset / YooAssetBridge API。
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
            var ctx = LaunchContext.Instance;

            if (!YooAssets.IsInitialized)
                YooAssets.Initialize();

            // 复用已有包（editor CheckAutoInit 可能抢先创建）
            if (!YooAssets.TryGetPackage(ctx.PackageName, out var package))
                package = YooAssets.CreatePackage(ctx.PackageName);

            if (package.InitializeStatus == EOperationStatus.None)
            {
                var op = package.InitializePackageAsync(
                    YooAssetBridge.CreateDefaultInitOptions(ctx.PackageName));
                yield return op;

                if (op.Status != EOperationStatus.Succeeded)
                {
                    Debug.LogError($"[Launch] 资源包初始化失败: {op.Error}");
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

            ctx.DefaultPackage = package;
            ctx.Progress = 0.1f;
            Debug.Log("[Launch] 资源包就绪");

            mFSM.ChangeState(LaunchState.HotCheckVersion);
        }
    }
}
