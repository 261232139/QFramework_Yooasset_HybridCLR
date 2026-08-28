/****************************************************************************
 * Copyright (c) 2016 - 2023 liangxiegame UNDER MIT License
 * 
 * https://qframework.cn
 * https://github.com/liangxiegame/QFramework
 * https://gitee.com/liangxiegame/QFramework
 ****************************************************************************/

using System;
using System.Collections;
using UnityEngine;
using YooAsset;

namespace QFramework
{
#if UNITY_EDITOR
    [ClassAPI("07.ResKit", "ResKit", 0, "ResKit")]
    [APIDescriptionCN("资源管理方案 (YooAsset v3)")]
    [APIDescriptionEN("Resource Managements Solution (YooAsset v3)")]
#endif
    public class ResKit
    {
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod]
        public static void CheckAutoInit()
        {
            // 编辑器下自动初始化 YooAsset（使用 EditorSimulateMode）
            if (!YooAssetBridge.IsInitialized)
            {
                Init();
            }
        }
#endif

#if UNITY_EDITOR
        [MethodAPI]
        [APIDescriptionCN("初始化 ResKit（底层使用 YooAsset v3）")]
        [APIDescriptionEN("initialise ResKit (YooAsset v3)")]
        [APIExampleCode(@"
ResKit.Init();
")]
#endif
        public static void Init()
        {
            ResMgr.Init();
        }

#if UNITY_EDITOR
        [MethodAPI]
        [APIDescriptionCN("异步初始化 ResKit")]
        [APIDescriptionEN("initialise ResKit async")]
        [APIExampleCode(@"
IEnumerator Start()
{
    yield return ResKit.InitAsync();
}

// Or With ActionKit
ResKit.InitAsync().ToAction().Start(this,()=>
{

});
")]
#endif
        public static IEnumerator InitAsync()
        {
            yield return ResMgr.InitAsync();
        }

        private static readonly Lazy<ResKit> mInstance = new Lazy<ResKit>(() => new ResKit().InternalInit());
        internal static ResKit Get => mInstance.Value;

        internal IOCContainer Container = new IOCContainer();

        ResKit InternalInit()
        {
            return this;
        }
    }
}