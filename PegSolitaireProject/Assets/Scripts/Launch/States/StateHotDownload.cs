/****************************************************************************
 * Copyright (c) 2024 liangxiegame UNDER MIT License
 *
 * StateHotDownload — 下载差量资源
 *
 * 约束: 热更域，不得调用任何游戏业务代码。
 *       无差量资源时直接穿透，移交 HotUpdateFSM。
 ****************************************************************************/

using System.Collections;
using UnityEngine;
using QFramework;
using YooAsset;

namespace Launch
{
    internal class StateHotDownload : AbstractState<LaunchState, GameLauncher>
    {
        public StateHotDownload(FSM<LaunchState> fsm, GameLauncher target) : base(fsm, target) { }

        protected override void OnEnter()
        {
            Debug.Log("[HotDownload] 计算差量资源...");
            mTarget.StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            var ctx        = LaunchContext.Instance;
            var package    = ctx.DefaultPackage;
            var downloader = package.CreateResourceDownloader(new ResourceDownloaderOptions(8, 3));

            if (downloader.TotalDownloadCount == 0)
            {
                Debug.Log("[HotDownload] 无需下载");
                ctx.Progress = 0.6f;
                StartHotUpdateFSM();
                yield break;
            }

            Debug.Log($"[HotDownload] 下载 {downloader.TotalDownloadCount} 个文件 " +
                      $"({downloader.TotalDownloadBytes / 1024} KB)");

            downloader.StartDownload();
            while (!downloader.IsDone)
            {
                ctx.Progress = 0.2f + 0.4f * downloader.Progress;
                yield return null;
            }

            if (downloader.Status != EOperationStatus.Succeeded)
            {
                Debug.LogError($"[HotDownload] 下载失败: {downloader.Error}");
                yield break;
            }

            Debug.Log("[HotDownload] 下载完成");
            ctx.Progress = 0.6f;
            StartHotUpdateFSM();
        }

        private void StartHotUpdateFSM()
        {
            LaunchContext.Instance.RaiseLaunchComplete();
        }
    }
}
