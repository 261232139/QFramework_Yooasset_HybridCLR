/****************************************************************************
 * Copyright (c) 2024 liangxiegame UNDER MIT License
 *
 * HotUpdateState — HotUpdateFSM 的状态枚举
 ****************************************************************************/

namespace HotUpdate
{
    public enum HotUpdateState
    {
        /// <summary>加载 HybridCLR DLL + 初始化 ResKit / UIKit / AudioKit</summary>
        LoadModules,
        /// <summary>调用热更侧入口 / 打开大厅 UI</summary>
        EnterLobby,
    }
}
