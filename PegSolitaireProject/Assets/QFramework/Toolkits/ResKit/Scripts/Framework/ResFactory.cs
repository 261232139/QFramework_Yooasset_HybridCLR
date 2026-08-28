/****************************************************************************
 * Copyright (c) 2017 snowcold
 * Copyright (c) 2017 ~ 2022 liangxie UNDER MIT LICENSE
 * 
 * https://qframework.cn
 * https://github.com/liangxiegame/QFramework
 * https://gitee.com/liangxiegame/QFramework
 *
 * Refactored 2024 — 底层替换为 YooAsset v3
 ****************************************************************************/

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QFramework
{
    public static class ResFactory
    {
        public static IRes Create(ResSearchKeys resSearchKeys)
        {
            // 所有资源统一通过 YooAssetRes 加载
            var res = YooAssetRes.Allocate(resSearchKeys.AssetName, resSearchKeys.AssetType);
            return res;
        }
    }
}