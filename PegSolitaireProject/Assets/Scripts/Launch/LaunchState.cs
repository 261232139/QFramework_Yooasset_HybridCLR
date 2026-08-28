/****************************************************************************
 * Copyright (c) 2024 liangxiegame UNDER MIT License
 *
 * LaunchState — LaunchFSM 的状态枚举
 * （独立文件方便两套 FSM 枚举各自演进）
 ****************************************************************************/

namespace Launch
{
    public enum LaunchState
    {
        /// <summary>YooAsset 全局初始化 + 资源包就绪（纯基础设施，零游戏代码）</summary>
        Launch,
        /// <summary>请求远端版本号 + 加载远端清单</summary>
        HotCheckVersion,
        /// <summary>下载差量资源（无差量时直接穿透）</summary>
        HotDownload,
    }
}
