/****************************************************************************
 * Copyright (c) 2024 liangxiegame UNDER MIT License
 * 
 * 游戏启动管理器 — 使用 QFramework FSM 驱动启动流程
 * 挂载在 Boot.scene 的 GameObject 上
 * 
 * 启动流程: Launch → HotFix → Init → EnterLobby
 ****************************************************************************/

using System.Collections;
using UnityEngine;
using QFramework;
using YooAsset;

namespace GameLauncher
{
    /// <summary>
    /// 游戏启动阶段枚举
    /// </summary>
    public enum GameLaunchState
    {
        /// <summary>环境检测 & YooAsset 初始化</summary>
        Launch,
        /// <summary>热更新检测 & 资源下载</summary>
        HotFix,
        /// <summary>关键模块初始化（ResKit、UIKit、AudioKit、HybridCLR）</summary>
        Init,
        /// <summary>启动完成，进入游戏大厅</summary>
        EnterLobby
    }

    /// <summary>
    /// 游戏启动管理器 — 使用 QFramework FSM 驱动启动流程
    /// </summary>
    public class GameLauncher : MonoBehaviour
    {
        [Header("YooAsset 配置")]
        [SerializeField] private string packageName = "DefaultPackage";
        [SerializeField] private string packageVersion = "1.0.0";

        [Header("HybridCLR 热更新配置")]
        [SerializeField] private string hotUpdateDllName = "HotUpdate";
        [SerializeField] private string hotUpdateDllLocation = "Assets/Res/HotUpdate/HotUpdate.dll";
        [SerializeField] private string hotUpdateEntryMethod = "HotUpdate.GameEntry, HotUpdate";

        [Header("启动进度")]
        [SerializeField] private float progress;
        public float Progress => progress;

        /// <summary>
        /// 启动状态机
        /// </summary>
        public FSM<GameLaunchState> LaunchFSM { get; private set; }

        /// <summary>
        /// 默认资源包
        /// </summary>
        public ResourcePackage DefaultPackage { get; private set; }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            LaunchFSM = new FSM<GameLaunchState>();

            // ──── 阶段 1: Launch ────
            LaunchFSM.State(GameLaunchState.Launch)
                .OnCondition(() => true)
                .OnEnter(() =>
                {
                    Debug.Log("[GameLauncher] ▶ Launch: 初始化 YooAsset...");
                    StartCoroutine(LaunchRoutine());
                })
                .OnUpdate(() => { })
                .OnExit(() => { });

            // ──── 阶段 2: HotFix ────
            LaunchFSM.State(GameLaunchState.HotFix)
                .OnCondition(() => LaunchFSM.CurrentStateId == GameLaunchState.Launch)
                .OnEnter(() =>
                {
                    Debug.Log("[GameLauncher] ▶ HotFix: 检查热更新...");
                    StartCoroutine(HotFixRoutine());
                })
                .OnUpdate(() => { })
                .OnExit(() => { });

            // ──── 阶段 3: Init ────
            LaunchFSM.State(GameLaunchState.Init)
                .OnCondition(() => LaunchFSM.CurrentStateId == GameLaunchState.HotFix)
                .OnEnter(() =>
                {
                    Debug.Log("[GameLauncher] ▶ Init: 初始化游戏模块...");
                    StartCoroutine(InitRoutine());
                })
                .OnUpdate(() => { })
                .OnExit(() => { });

            // ──── 阶段 4: EnterLobby ────
            LaunchFSM.State(GameLaunchState.EnterLobby)
                .OnCondition(() => LaunchFSM.CurrentStateId == GameLaunchState.Init)
                .OnEnter(() =>
                {
                    Debug.Log("[GameLauncher] ▶ EnterLobby: 启动完成，进入大厅!");
                    EnterLobby();
                })
                .OnUpdate(() => { })
                .OnExit(() => { });

            // 注册状态切换回调
            LaunchFSM.OnStateChanged((prev, next) =>
            {
                Debug.Log($"[GameLauncher] 状态: {prev} → {next}");
            });

            // 启动第一个状态
            LaunchFSM.StartState(GameLaunchState.Launch);
        }

        private void Update()
        {
            LaunchFSM?.Update();
        }

        #region Launch 阶段

        private IEnumerator LaunchRoutine()
        {
            // 1. 初始化 YooAsset 全局系统
            if (!YooAssets.IsInitialized)
            {
                YooAssets.Initialize();
                Debug.Log("[GameLauncher] YooAsset 初始化完成");
            }

            // 2. 检查是否已有包（ResKit.CheckAutoInit 可能已创建）
            ResourcePackage package;
            if (YooAssets.TryGetPackage(packageName, out package))
            {
                Debug.Log($"[GameLauncher] 使用已有资源包: {packageName}");
                // 等待已有包初始化完成（异步初始化可能还在进行中）
                if (package.InitializeStatus == EOperationStatus.None)
                {
                    // 包已创建但未开始初始化，需要初始化
                    var initOptions = YooAssetBridge.CreateDefaultInitOptions();
                    var initOp = package.InitializePackageAsync(initOptions);
                    yield return initOp;
                    if (initOp.Status != EOperationStatus.Succeeded)
                    {
                        Debug.LogError($"[GameLauncher] 资源包初始化失败: {initOp.Error}");
                        yield break;
                    }
                }
                else if (package.InitializeStatus == EOperationStatus.Processing)
                {
                    // 包正在初始化中，等待完成
                    Debug.Log("[GameLauncher] 等待资源包初始化完成...");
                    while (package.InitializeStatus == EOperationStatus.Processing)
                    {
                        yield return null;
                    }
                    if (package.InitializeStatus != EOperationStatus.Succeeded)
                    {
                        Debug.LogError($"[GameLauncher] 资源包初始化失败");
                        yield break;
                    }
                }
                else if (package.InitializeStatus != EOperationStatus.Succeeded)
                {
                    Debug.LogError($"[GameLauncher] 资源包初始化状态异常: {package.InitializeStatus}");
                    yield break;
                }
            }
            else
            {
                // 创建并初始化资源包
                package = YooAssets.CreatePackage(packageName);

                var initOptions = YooAssetBridge.CreateDefaultInitOptions();
                var initOp = package.InitializePackageAsync(initOptions);
                yield return initOp;

                if (initOp.Status != EOperationStatus.Succeeded)
                {
                    Debug.LogError($"[GameLauncher] YooAsset 包初始化失败: {initOp.Error}");
                    yield break;
                }
            }

            DefaultPackage = package;
            progress = 0.15f;

            Debug.Log("[GameLauncher] YooAsset 资源包就绪");

            // 进入下一阶段
            LaunchFSM.ChangeState(GameLaunchState.HotFix);
        }

        #endregion

        #region HotFix 阶段

        private IEnumerator HotFixRoutine()
        {
            var package = DefaultPackage;

            // 1. 获取远端版本
            var versionOp = package.RequestPackageVersionAsync(new RequestPackageVersionOptions(true, 60));
            yield return versionOp;

            if (versionOp.Status != EOperationStatus.Succeeded)
            {
                Debug.LogWarning("[GameLauncher] 获取远端版本失败，可能无网络，使用本地版本");
                progress = 0.3f;
                LaunchFSM.ChangeState(GameLaunchState.Init);
                yield break;
            }

            var remoteVersion = versionOp.PackageVersion;
            Debug.Log($"[GameLauncher] 远端版本: {remoteVersion}, 本地版本: {packageVersion}");

            // 2. 加载远端清单
            var manifestOp = package.LoadPackageManifestAsync(new LoadPackageManifestOptions(remoteVersion, 60));
            yield return manifestOp;

            if (manifestOp.Status != EOperationStatus.Succeeded)
            {
                Debug.LogError($"[GameLauncher] 加载远端清单失败: {manifestOp.Error}");
                yield break;
            }

            // 3. 计算需要下载的资源大小
            var downloader = package.CreateResourceDownloader(new ResourceDownloaderOptions(8, 3));

            if (downloader.TotalDownloadCount > 0)
            {
                Debug.Log($"[GameLauncher] 需要下载 {downloader.TotalDownloadCount} 个资源，总计 {downloader.TotalDownloadBytes} 字节");

                downloader.StartDownload();
                while (!downloader.IsDone)
                {
                    progress = 0.3f + 0.5f * downloader.Progress;
                    yield return null;
                }

                if (downloader.Status != EOperationStatus.Succeeded)
                {
                    Debug.LogError($"[GameLauncher] 资源下载失败: {downloader.Error}");
                    yield break;
                }

                Debug.Log("[GameLauncher] 资源下载完成");
            }
            else
            {
                Debug.Log("[GameLauncher] 无需下载资源");
            }

            progress = 0.8f;
            LaunchFSM.ChangeState(GameLaunchState.Init);
        }

        #endregion

        #region Init 阶段

        private IEnumerator InitRoutine()
        {
            // 1. 初始化 ResKit（YooAsset 桥接器）
            if (!YooAssetBridge.IsInitialized)
            {
                ResKit.Init();
                Debug.Log("[GameLauncher] ResKit 初始化完成");
            }

            // 2. 初始化 UIKit — 设置 PanelLoader 为 ResKitPanelLoaderPool
            UIKit.Config.PanelLoaderPool = new ResKitPanelLoaderPool();
            Debug.Log("[GameLauncher] UIKit 初始化完成");

            // 3. 初始化 AudioKit（AudioKit 依赖 ResKit，无需额外初始化）
            Debug.Log("[GameLauncher] AudioKit 就绪");

            // 4. 加载 HybridCLR 热更新 DLL
            yield return LoadHotUpdateDLL();

            progress = 0.9f;
            yield return null;

            Debug.Log("[GameLauncher] 游戏模块初始化完成");
            LaunchFSM.ChangeState(GameLaunchState.EnterLobby);
        }

        /// <summary>
        /// 加载 HybridCLR 热更新 DLL
        /// </summary>
        private IEnumerator LoadHotUpdateDLL()
        {
#if !ENABLE_HYBRIDCLR || UNITY_EDITOR
            // 编辑器下或未启用 HybridCLR 时跳过
            Debug.Log("[GameLauncher] HybridCLR 未启用，跳过热更新 DLL 加载");
            yield break;
#endif
            // 使用 ResKit 加载热更新 DLL
            var loader = ResLoader.Allocate();
            var dllRes = loader.LoadResSync(ResSearchKeys.Allocate(hotUpdateDllLocation, null, typeof(TextAsset)));
            var dllAsset = dllRes?.Asset as TextAsset;

            if (dllAsset == null)
            {
                Debug.LogError($"[GameLauncher] 热更新 DLL 加载失败: {hotUpdateDllLocation}");
                loader.Recycle2Cache();
                yield break;
            }

            // 通过 HybridCLR RuntimeApi 加载 DLL
            var dllBytes = dllAsset.bytes;
            var dllName = hotUpdateDllName;
            System.Reflection.Assembly.Load(dllBytes);

            Debug.Log($"[GameLauncher] HybridCLR 热更新 DLL 加载完成: {dllName}");

            loader.Recycle2Cache();
            yield return null;
        }

        #endregion

        #region EnterLobby 阶段

        private void EnterLobby()
        {
            progress = 1.0f;
            Debug.Log("[GameLauncher] 🎮 游戏启动完成，进入大厅!");

            // 尝试调用热更新侧的入口方法
            try
            {
                var entryType = System.Type.GetType(hotUpdateEntryMethod);
                if (entryType != null)
                {
                    var mainMethod = entryType.GetMethod("Main");
                    if (mainMethod != null)
                    {
                        mainMethod.Invoke(null, null);
                        Debug.Log("[GameLauncher] 热更新入口 Main() 调用成功");
                        return;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[GameLauncher] 热更新入口调用失败: {e.Message}");
            }

            // 如果没有热更新 DLL，直接打开大厅 UI
            // UIKit.OpenPanel<UILobbyPanel>();
        }

        #endregion
    }
}