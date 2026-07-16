/****************************************************************************
 * Copyright (c) 2024 liangxiegame UNDER MIT License
 *
 * LaunchContext — 启动流程跨程序集共享数据
 *
 * Launch assembly 写入，HotUpdate assembly 只读。
 * 通过静态单例传递，避免两套 FSM 之间直接依赖 MonoBehaviour 引用。
 ****************************************************************************/

using UnityEngine;
using YooAsset;

namespace Launch
{
    public class LaunchContext
    {
        private static LaunchContext sInstance;
        public static LaunchContext Instance => sInstance ??= new LaunchContext();

        internal static void Reset() => sInstance = null;

        // ── Launch 阶段写入 ──────────────────────────────────────────────
        /// <summary>YooAsset 初始化完毕、就绪的资源包</summary>
        public ResourcePackage DefaultPackage { get; internal set; }

        /// <summary>HotCheckVersion 阶段获取到的远端版本号</summary>
        public string RemoteVersion { get; internal set; }

        // ── 配置（由 GameLauncher Inspector 写入）───────────────────────
        public string PackageName          { get; internal set; } = "DefaultPackage";
        public string HotUpdateDllLocation { get; internal set; } = "Assets/Res/HotUpdate/HotUpdate.dll";
        public string HotUpdateEntryMethod { get; internal set; } = "HotUpdate.GameEntry, HotUpdate";

        // ── 进度（两套 FSM 共同写入，UI 只读）──────────────────────────
        private float mProgress;
        public float Progress
        {
            get => mProgress;
            set
            {
                mProgress = Mathf.Clamp01(value);
                OnProgressChanged?.Invoke(mProgress);
            }
        }

        /// <summary>进度变更事件，供 UI 层订阅</summary>
        public event System.Action<float> OnProgressChanged;

        // ── 阶段移交（Launch → HotUpdate，避免程序集循环依赖）────────
        /// <summary>
        /// LaunchFSM 完成时触发（热更检查结束 or 下载完成）。
        /// GameLauncher 订阅此事件并动态启动 HotUpdateRunner，
        /// 使 Launch assembly 无需引用 HotUpdate assembly。
        /// </summary>
        public event System.Action OnLaunchComplete;

        internal void RaiseLaunchComplete() => OnLaunchComplete?.Invoke();
    }
}
