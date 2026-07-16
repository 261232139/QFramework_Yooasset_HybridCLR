/****************************************************************************
 * Copyright (c) 2024 liangxiegame UNDER MIT License
 *
 * HotUpdateRunner — HotUpdateFSM 的宿主 MonoBehaviour
 *
 * 由 LaunchFSM 最后一个状态（HotCheckVersion 离线跳过 / HotDownload 完成）
 * 通过 AddComponent 动态挂载到同一 GameObject 上，接管后续流程。
 *
 * 职责（仅限）:
 *   1. 创建并启动 HotUpdateFSM
 *   2. 驱动 HotUpdateFSM.Update()
 *
 * 不包含任何业务逻辑，所有流程细节由各状态类处理。
 ****************************************************************************/

using UnityEngine;
using QFramework;

namespace HotUpdate
{
    public class HotUpdateRunner : MonoBehaviour
    {
        public FSM<HotUpdateState> HotUpdateFSM { get; private set; }

        private void Start()
        {
            HotUpdateFSM = new FSM<HotUpdateState>();

            HotUpdateFSM.AddState(HotUpdateState.LoadModules, new StateLoadModules(HotUpdateFSM, this));
            HotUpdateFSM.AddState(HotUpdateState.EnterLobby,  new StateEnterLobby(HotUpdateFSM, this));

            HotUpdateFSM.OnStateChanged((prev, next) =>
                Debug.Log($"[HotUpdateFSM] {prev} → {next}"));

            HotUpdateFSM.StartState(HotUpdateState.LoadModules);
        }

        private void Update() => HotUpdateFSM?.Update();

        private void OnDestroy() => HotUpdateFSM?.Clear();
    }
}
