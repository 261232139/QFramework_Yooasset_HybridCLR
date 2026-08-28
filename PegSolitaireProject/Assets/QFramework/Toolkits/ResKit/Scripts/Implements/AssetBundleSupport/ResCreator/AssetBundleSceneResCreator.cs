using System;

namespace QFramework
{
    [Obsolete("已废弃，ResKit 已改用 YooAsset v3。Deprecated, use YooAsset instead.")]
    public class AssetBundleSceneResCreator : IResCreator
    {
        public bool Match(ResSearchKeys resSearchKeys)
        {
            var assetData =  AssetBundleSettings.AssetBundleConfigFile.GetAssetData(resSearchKeys);

            if (assetData != null)
            {
                return assetData.AssetType == ResLoadType.ABScene;
            }

            return false;
        }

        public IRes Create(ResSearchKeys resSearchKeys)
        {
            return AssetBundleSceneRes.Allocate(resSearchKeys.AssetName);
        }
    }
}