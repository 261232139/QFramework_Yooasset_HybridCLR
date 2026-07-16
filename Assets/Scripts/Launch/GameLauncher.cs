/****************************************************************************
 * Copyright (c) 2024 liangxiegame UNDER MIT License
 *
 * GameLauncher — Boot 场景入口 MonoBehaviour
 *
 * 职责（仅限）:
 *   1. 从 Inspector 读取配置并写入 LaunchContext
 *   2. 启动 LaunchFSM
 *   3. 驱动 LaunchFSM.Update()
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
            // 当 LaunchFSM 完成时，动态挂载 HotUpdateRunner（Type.GetType 避免编译期循环依赖）
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

        private void Update() => LaunchFSM?.Update();

        private void OnDestroy() => LaunchFSM?.Clear();
    }
}
