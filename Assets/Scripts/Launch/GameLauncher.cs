/****************************************************************************
 * Copyright (c) 2024 liangxiegame UNDER MIT License
 *
 * GameLauncher — Boot 场景入口 MonoBehaviour
 *
 * 职责（仅限）:
 *   1. 从 Inspector 读取配置并写入 LaunchContext
 *   2. 启动 LaunchFSM
 *   3. 驱动 LaunchFSM.Update()
 *   4. 自动挂载 LoadingUI 到 LaunchUI
 *
 * 不包含任何业务逻辑，所有流程细节由各状态类处理。
 ****************************************************************************/

using UnityEngine;
using QFramework;

namespace Launch
{
    public class GameLauncher : MonoBehaviour
    {
        [Header("YooAsset 配置")]
        [SerializeField] private string packageName = "DefaultPackage";

        [Header("HybridCLR 配置")]
        [SerializeField] private string hotUpdateDllLocation = "Assets/Res/HotUpdate/HotUpdate.dll";
        [SerializeField] private string hotUpdateEntryMethod = "HotUpdate.GameEntry, HotUpdate";

        [Header("Loading UI")]
        [SerializeField] private GameObject launchUI;

        public FSM<LaunchState> LaunchFSM { get; private set; }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            var ctx = LaunchContext.Instance;
            ctx.PackageName          = packageName;
            ctx.HotUpdateDllLocation = hotUpdateDllLocation;
            ctx.HotUpdateEntryMethod = hotUpdateEntryMethod;
        }

        private void Start()
        {
            InitLoadingUI();

            LaunchContext.Instance.OnLaunchComplete += () =>
            {
                var runnerType = System.Type.GetType("HotUpdate.HotUpdateRunner, HotUpdate");
                if (runnerType != null)
                    gameObject.AddComponent(runnerType);
                else
                    Debug.LogError("[GameLauncher] 找不到 HotUpdate.HotUpdateRunner，请确认 hotupdate.asmdef 存在");
            };

            LaunchFSM = new FSM<LaunchState>();

            LaunchFSM.AddState(LaunchState.Launch,          new StateLaunch(LaunchFSM, this));
            LaunchFSM.AddState(LaunchState.HotCheckVersion, new StateHotCheckVersion(LaunchFSM, this));
            LaunchFSM.AddState(LaunchState.HotDownload,     new StateHotDownload(LaunchFSM, this));

            LaunchFSM.OnStateChanged((prev, next) =>
                Debug.Log($"[LaunchFSM] {prev} → {next}"));

            LaunchFSM.StartState(LaunchState.Launch);
        }

        private void InitLoadingUI()
        {
            if (launchUI == null)
            {
                // 在场景中查找 LaunchUI
                var canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
                foreach (var canvas in canvases)
                {
                    var t = canvas.transform.Find("LaunchUI");
                    if (t != null)
                    {
                        launchUI = t.gameObject;
                        break;
                    }
                }
            }

            if (launchUI == null)
            {
                Debug.LogWarning("[GameLauncher] 找不到 LaunchUI，跳过 Loading UI 初始化");
                return;
            }

            if (launchUI.GetComponent<LoadingUI>() == null)
            {
                launchUI.AddComponent<LoadingUI>();
                Debug.Log("[GameLauncher] 已自动挂载 LoadingUI 到 LaunchUI");
            }
        }

        private void Update() => LaunchFSM?.Update();

        private void OnDestroy() => LaunchFSM?.Clear();
    }
}
