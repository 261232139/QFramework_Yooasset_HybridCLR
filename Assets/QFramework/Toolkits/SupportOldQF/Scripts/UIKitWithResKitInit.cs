using System;
using UnityEngine;
using YooAsset;

namespace QFramework
{
    public class UIKitWithResKitInit
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            UIKit.Config.PanelLoaderPool = new ResKitPanelLoaderPool();
        }
    }

    public class ResKitPanelLoaderPool : AbstractPanelLoaderPool
    {
        public class ResKitPanelLoader : IPanelLoader
        {
            private AssetHandle mHandle;

            public GameObject LoadPanelPrefab(PanelSearchKeys panelSearchKeys)
            {
                EnsureInitialized();
                var location = ResolveLocation(panelSearchKeys);
                
                var package = YooAssetBridge.DefaultPackage;
                mHandle = package.LoadAssetSync<GameObject>(location);
                
                var prefab = mHandle?.AssetObject as GameObject;
                ValidatePrefab(prefab, location, panelSearchKeys.AssetBundleName);
                return prefab;
            }

            public void LoadPanelPrefabAsync(PanelSearchKeys panelSearchKeys, Action<GameObject> onLoad)
            {
                EnsureInitialized();
                var location = ResolveLocation(panelSearchKeys);
                var package = YooAssetBridge.DefaultPackage;

                var handle = package.LoadAssetAsync<GameObject>(location);
                handle.Completed += (op) =>
                {
                    mHandle = handle;
                    var prefab = op.AssetObject as GameObject;
                    ValidatePrefab(prefab, location, panelSearchKeys.AssetBundleName);
                    onLoad?.Invoke(prefab);
                };
            }

            public void Unload()
            {
                if (mHandle != null)
                {
                    mHandle.Dispose();
                    mHandle = null;
                }
            }

            private static void EnsureInitialized()
            {
                if (!ResMgr.ResMgrInited)
                    throw new InvalidOperationException("[UIKit] ResKit is not initialized.");

                if (!YooAssetBridge.IsInitialized)
                    throw new InvalidOperationException("[UIKit] YooAsset default package is not ready.");
            }

            private static string ResolveLocation(PanelSearchKeys panelSearchKeys)
            {
                var location = panelSearchKeys.GameObjName;
                if (location.IsNullOrEmpty() && panelSearchKeys.PanelType.IsNotNull())
                    location = panelSearchKeys.PanelType.Name;

                if (location.IsNullOrEmpty())
                    throw new InvalidOperationException("[UIKit] Panel asset location is empty.");

                return location;
            }

            private static void ValidatePrefab(
                GameObject prefab, string location, string assetBundleName)
            {
                if (prefab == null)
                {
                    throw new InvalidOperationException(
                        $"[UIKit] YooAsset failed to load panel '{location}'. " +
                        $"Bundle='{assetBundleName ?? "<address-only>"}'.");
                }

                if (prefab.GetComponent<UIPanel>() == null)
                {
                    throw new InvalidOperationException(
                        $"[UIKit] Prefab '{location}' does not contain a UIPanel component.");
                }
            }
        }

        protected override IPanelLoader CreatePanelLoader()
        {
            return new ResKitPanelLoader();
        }
    }
}
