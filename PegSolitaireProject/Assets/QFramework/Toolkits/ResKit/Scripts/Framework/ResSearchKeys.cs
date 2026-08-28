/****************************************************************************
 * Copyright (c) 2015 ~ 2024 liangxiegame UNDER MIT LICENSE
 * 
 * https://qframework.cn
 * https://github.com/liangxiegame/QFramework
 * https://gitee.com/liangxiegame/QFramework
 *
 * Refactored 2024 — AssetName 作为 YooAsset Location
 ****************************************************************************/

using System;

namespace QFramework
{
    public class ResSearchKeys : IPoolable,IPoolType
    {   
        public string AssetName { get; set; }

        public string OwnerBundle { get;  set; }

        public Type AssetType { get; set; }

        public string OriginalAssetName { get; set; }
        
        
        public static ResSearchKeys Allocate(string assetName, string ownerBundleName = null, Type assetType = null)
        {
            var resSearchRule = SafeObjectPool<ResSearchKeys>.Instance.Allocate();
            resSearchRule.AssetName = assetName; // YooAsset 使用原始 Location，不转为小写
            resSearchRule.OwnerBundle = null;     // YooAsset 不需要 OwnerBundle
            resSearchRule.AssetType = assetType;
            resSearchRule.OriginalAssetName = assetName;
            return resSearchRule;
        }
        
        public void Recycle2Cache()
        {
            SafeObjectPool<ResSearchKeys>.Instance.Recycle(this);
        }

        public bool Match(IRes res)
        {
            return res.AssetName == AssetName;
        }

        public override string ToString()
        {
            return string.Format("AssetName:{0} TypeName:{1}", AssetName,
                AssetType);
        }

        void IPoolable.OnRecycled()
        {
            AssetName = null;

            OwnerBundle = null;

            AssetType = null;
        }

        bool IPoolable.IsRecycled { get; set; }
    }
}