/****************************************************************************
 * Copyright (c) 2024 liangxiegame UNDER MIT License
 * 
 * ResKit → YooAsset v3 桥接器
 * 提供全局访问 YooAsset ResourcePackage 的静态入口
 ****************************************************************************/

using System.Collections;
using UnityEngine;
using YooAsset;

namespace QFramework
{
    /// <summary>
    /// YooAsset 桥接器 — 管理 YooAsset 的初始化与默认资源包
    /// </summary>
    public static class YooAssetBridge
    {
        private static ResourcePackage sDefaultPackage;

        /// <summary>
        /// 默认资源包
        /// </summary>
        public static ResourcePackage DefaultPackage
        {
            get
            {
                if (sDefaultPackage == null)
                {
                    Debug.LogError("[YooAssetBridge] DefaultPackage not initialized! Call ResKit.Init() first.");
                }
                return sDefaultPackage;
            }
            private set => sDefaultPackage = value;
        }

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public static bool IsInitialized => sDefaultPackage != null;

        /// <summary>
        /// 初始化 YooAsset 并创建默认资源包（异步启动，不阻塞主线程）
        /// </summary>
        public static void Initialize(string packageName = "DefaultPackage", InitializePackageOptions packageOptions = null)
        {
            if (sDefaultPackage != null)
            {
                Debug.LogWarning("[YooAssetBridge] Already initialized.");
                return;
            }

            // 1. 初始化 YooAsset 全局系统
            if (!YooAssets.IsInitialized)
            {
                YooAssets.Initialize();
            }

            // 2. 创建资源包
            var package = YooAssets.CreatePackage(packageName);

            // 3. 初始化资源包（异步，不阻塞）
            if (packageOptions == null)
            {
                packageOptions = CreateDefaultInitOptions();
            }

            var initOp = package.InitializePackageAsync(packageOptions);
            sDefaultPackage = package;

            // 注册完成回调，不阻塞等待
            initOp.Completed += (op) =>
            {
                if (op.Status == EOperationStatus.Succeeded)
                {
                    Debug.Log($"[YooAssetBridge] YooAsset initialized. Package: {packageName}");
                }
                else
                {
                    Debug.LogError($"[YooAssetBridge] Package init failed: {op.Error}");
                }
            };
        }

        /// <summary>
        /// 异步初始化 YooAsset 并创建默认资源包
        /// </summary>
        public static IEnumerator InitializeAsync(string packageName = "DefaultPackage", InitializePackageOptions packageOptions = null)
        {
            if (sDefaultPackage != null)
            {
                Debug.LogWarning("[YooAssetBridge] Already initialized.");
                yield break;
            }

            // 1. 初始化 YooAsset 全局系统
            if (!YooAssets.IsInitialized)
            {
                YooAssets.Initialize();
            }

            // 2. 创建资源包
            var package = YooAssets.CreatePackage(packageName);

            // 3. 初始化资源包
            if (packageOptions == null)
            {
                packageOptions = CreateDefaultInitOptions();
            }

            var initOp = package.InitializePackageAsync(packageOptions);
            yield return initOp;

            if (initOp.Status != EOperationStatus.Succeeded)
            {
                Debug.LogError($"[YooAssetBridge] Package init failed: {initOp.Error}");
                yield break;
            }

            sDefaultPackage = package;
            Debug.Log($"[YooAssetBridge] YooAsset initialized. Package: {packageName}");
        }

        /// <summary>
        /// 创建默认的初始化参数（编辑器模拟模式 / 运行时内置文件系统）
        /// </summary>
        public static InitializePackageOptions CreateDefaultInitOptions()
        {
#if UNITY_EDITOR
            // 编辑器下使用 EditorSimulateMode
            return new EditorSimulateModeOptions
            {
                EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(null)
            };
#else
            // 运行时使用 OfflinePlayMode（从 StreamingAssets 读取）
            return new OfflinePlayModeOptions
            {
                BuiltinFileSystemParameters = FileSystemParameters.CreateDefaultBuiltinFileSystemParameters()
            };
#endif
        }

        /// <summary>
        /// 销毁资源包
        /// </summary>
        public static void Destroy()
        {
            if (sDefaultPackage != null)
            {
                var package = sDefaultPackage;
                sDefaultPackage = null;
                var op = package.DestroyPackageAsync();
                op.WaitForCompletion();
                YooAssets.RemovePackage(package.PackageName);
            }
        }
    }
}