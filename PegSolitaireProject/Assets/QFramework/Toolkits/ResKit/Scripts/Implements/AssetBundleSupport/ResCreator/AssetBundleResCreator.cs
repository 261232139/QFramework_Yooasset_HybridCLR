using UnityEngine;
using System;

namespace QFramework
{
    [Obsolete("已废弃，ResKit 已改用 YooAsset v3。Deprecated, use YooAsset instead.")]
    public class AssetBundleResCreator : IResCreator
    {
        public bool Match(ResSearchKeys resSearchKeys)
        {
            return resSearchKeys.AssetType == typeof(AssetBundle);
        }

        public IRes Create(ResSearchKeys resSearchKeys)
        {
            return AssetBundleRes.Allocate(resSearchKeys.AssetName);
        }
    }
}