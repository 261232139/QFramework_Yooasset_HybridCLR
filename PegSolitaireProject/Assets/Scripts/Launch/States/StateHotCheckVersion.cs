/****************************************************************************
 * Copyright (c) 2024 liangxiegame UNDER MIT License
 *
 * StateHotCheckVersion — 请求远端版本号 + 加载远端清单
 *
 * 约束: 热更域，不得调用任何游戏业务代码。
 *       离线或失败时跳过热更，直接进入 HotUpdate 流程。
 ****************************************************************************/

using System.Collections;
using UnityEngine;
using QFramework;
using YooAsset;

namespace Launch
{
    internal class StateHotCheckVersion : AbstractState<LaunchState, GameLauncher>
    {
        public StateHotCheckVersion(FSM<LaunchState> fsm, GameLauncher target) : base(fsm, target) { }

        protected override void OnEnter()
        {
            Debug.Log("[HotCheckVersion] 请求远端版本...");
            mTarget.StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            var ctx     = LaunchContext.Instance;
            var package = ctx.DefaultPackage;

            var versionOp = package.RequestPackageVersionAsync(
                new RequestPackageVersionOptions(appendTimeTicks: true, timeout: 60));
            yield return versionOp;

            if (versionOp.Status != EOperationStatus.Succeeded)
            {
                Debug.LogWarning("[HotCheckVersion] 获取远端版本失败（离线？），跳过热更");
                ctx.Progress = 0.3f;
                StartHotUpdateFSM();
                yield break;
            }

            ctx.RemoteVersion = versionOp.PackageVersion;
            Debug.Log($"[HotCheckVersion] 远端版本: {ctx.RemoteVersion}");

            var manifestOp = package.LoadPackageManifestAsync(
                new LoadPackageManifestOptions(ctx.RemoteVersion, timeout: 60));
            yield return manifestOp;

            if (manifestOp.Status != EOperationStatus.Succeeded)
            {
                Debug.LogError($"[HotCheckVersion] 加载远端清单失败: {manifestOp.Error}");
                yield break;
            }

            ctx.Progress = 0.2f;
            Debug.Log("[HotCheckVersion] 远端清单已加载");
            mFSM.ChangeState(LaunchState.HotDownload);
        }

        private void StartHotUpdateFSM()
        {
            LaunchContext.Instance.RaiseLaunchComplete();
        }
    }
}
