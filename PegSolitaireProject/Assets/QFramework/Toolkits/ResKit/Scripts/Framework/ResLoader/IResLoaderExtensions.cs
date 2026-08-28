/****************************************************************************
 * Copyright (c) 2016 ~ 2022 liangxiegame UNDER MIT LICENSE
 * 
 * https://qframework.cn
 * https://github.com/liangxiegame/QFramework
 * https://gitee.com/liangxiegame/QFramework
 ****************************************************************************/

using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using YooAsset;

namespace QFramework
{
#if UNITY_EDITOR
    [ClassAPI("07.ResKit", "ResLoader API", 2, "ResLoader API")]
    [APIDescriptionCN("资源管理方案")]
    [APIDescriptionEN("Resource Managements Solution")]
#endif
    public static class IResLoaderExtensions
    {
        private static Type ComponentType = typeof(Component);
        private static Type GameObjectType = typeof(GameObject);
        
#if UNITY_EDITOR
        [MethodAPI]
        [APIDescriptionCN("同步加载资源")]
        [APIDescriptionEN("Load Asset Sync")]
        [APIExampleCode(@"

var texture =mResLoader.LoadSync<Texture2D>(""MyAsset"");
// Or
texture = mResLoader.LoadSync<Texture2D>(""MyBundle"",""MyAsset"");
")]
#endif
        public static T LoadSync<T>(this IResLoader self, string assetName) where T : Object
        {
            var type = typeof(T);
            if (ComponentType.IsAssignableFrom(type))
            {
                var resSearchKeys = ResSearchKeys.Allocate(assetName, null, GameObjectType);
                var retAsset = (self.LoadAssetSync(resSearchKeys) as GameObject)?.GetComponent<T>();
                resSearchKeys.Recycle2Cache();
                return retAsset;
            }
            else
            {
                var resSearchKeys = ResSearchKeys.Allocate(assetName, null, type);
                var retAsset = self.LoadAssetSync(resSearchKeys) as T;
                resSearchKeys.Recycle2Cache();
                return retAsset;
            }
        }
        
        
        public static T LoadSync<T>(this IResLoader self, string ownerBundle, string assetName) where T : Object
        {
            var type = typeof(T);
            if (ComponentType.IsAssignableFrom(type))
            {
                var resSearchKeys = ResSearchKeys.Allocate(assetName, ownerBundle, GameObjectType);
                var retAsset = (self.LoadAssetSync(resSearchKeys) as GameObject)?.GetComponent<T>();
                resSearchKeys.Recycle2Cache();
                return retAsset;
            }
            else
            {
                var resSearchKeys = ResSearchKeys.Allocate(assetName, ownerBundle, type);
                var retAsset = self.LoadAssetSync(resSearchKeys) as T;
                resSearchKeys.Recycle2Cache();
                return retAsset;
            }
        }
        
#if UNITY_EDITOR
        [MethodAPI]
        [APIDescriptionCN("异步加载资源")]
        [APIDescriptionEN("Load Asset Async")]
        [APIExampleCode(@"

mResLoader.Add2Load<Texture2D>(""MyAsset"");
// Or
mResLoader.Add2Load<Texture2D>(""MyBundle"",""MyAsset"");

mResLoader.LoadAsync(()=>
{
    // 此时不会触发加载，而是从缓存中获取资源
    // resources are fetched from the cache
    var texture = mResLoader.LoadSync<Texture2D>(""MyAsset"");
});
")]
#endif
        public static void Add2Load(this IResLoader self, string assetName, Action<bool, IRes> listener = null,
            bool lastOrder = true)
        {
            
            var searchRule = ResSearchKeys.Allocate(assetName);
            self.Add2Load(searchRule, listener, lastOrder);
            searchRule.Recycle2Cache();
        }

        public static void Add2Load<T>(this IResLoader self, string assetName, Action<bool, IRes> listener = null,
            bool lastOrder = true)
        {
            var type = typeof(T);
            if (ComponentType.IsAssignableFrom(type))
            {
                var resSearchKeys = ResSearchKeys.Allocate(assetName, null, GameObjectType);
                self.Add2Load(resSearchKeys, listener, lastOrder);
                resSearchKeys.Recycle2Cache();
            }
            else
            {
                var searchRule = ResSearchKeys.Allocate(assetName, null, type);
                self.Add2Load(searchRule, listener, lastOrder);
                searchRule.Recycle2Cache();
            }
        }


        public static void Add2Load(this IResLoader self, string ownerBundle, string assetName,
            Action<bool, IRes> listener = null,
            bool lastOrder = true)
        {
            var searchRule = ResSearchKeys.Allocate(assetName, ownerBundle);
            self.Add2Load(searchRule, listener, lastOrder);
            searchRule.Recycle2Cache();
        }

        public static void Add2Load<T>(this IResLoader self, string ownerBundle, string assetName,
            Action<bool, IRes> listener = null,
            bool lastOrder = true)
        {
            var type = typeof(T);
            if (ComponentType.IsAssignableFrom(type))
            {
                var resSearchKeys = ResSearchKeys.Allocate(assetName, ownerBundle, GameObjectType);
                self.Add2Load(resSearchKeys, listener, lastOrder);
                resSearchKeys.Recycle2Cache();
            }
            else
            {
                var searchRule = ResSearchKeys.Allocate(assetName, ownerBundle, type);
                self.Add2Load(searchRule, listener, lastOrder);
                searchRule.Recycle2Cache();
            }
        }
        

#if UNITY_EDITOR
        [MethodAPI]
        [APIDescriptionCN("同步加载场景")]
        [APIDescriptionEN("Load Scene Sync")]
        [APIExampleCode(@"
mResLoader.LoadSceneSync(""BattleScene"");
// Or 
mResLoader.LoadSceneSync(""BattleSceneBundle"",""BattleScene"");


mResLoader.LoadSceneSync(""BattleScene"",LoadSceneMode.Additive);
//
mResLoader.LoadSceneSync(""BattleScene"",LoadSceneMode.Additive,LocalPhysicsMode.Physics2D);
")]
#endif
        public static void LoadSceneSync(this IResLoader self, string assetName,
            LoadSceneMode mode = LoadSceneMode.Single,
            LocalPhysicsMode physicsMode = LocalPhysicsMode.None)
        {
            var resSearchRule = ResSearchKeys.Allocate(assetName);
            self.LoadSceneSync(resSearchRule, mode, physicsMode);
            resSearchRule.Recycle2Cache();
        }

        public static void LoadSceneSync(this IResLoader self, string ownerBundle, string assetName,
            LoadSceneMode mode = LoadSceneMode.Single,
            LocalPhysicsMode physicsMode = LocalPhysicsMode.None)
        {
            var resSearchRule = ResSearchKeys.Allocate(assetName, ownerBundle);
            self.LoadSceneSync(resSearchRule, mode, physicsMode);
            resSearchRule.Recycle2Cache();
        }

        public static void LoadSceneSync(this IResLoader self, ResSearchKeys resSearchRule,
            LoadSceneMode mode = LoadSceneMode.Single,
            LocalPhysicsMode physicsMode = LocalPhysicsMode.None)
        {
            // 使用 YooAsset 加载场景
            var package = YooAssetBridge.DefaultPackage;
            if (package == null)
            {
                Debug.LogError("[IResLoaderExtensions] YooAsset not initialized!");
                return;
            }

            var handle = package.LoadSceneSync(resSearchRule.AssetName, mode, physicsMode);
            if (handle.Status != YooAsset.EOperationStatus.Succeeded)
            {
                Debug.LogError($"[IResLoaderExtensions] LoadSceneSync failed: {resSearchRule.AssetName}, Error: {handle.Error}");
            }
        }
#if UNITY_EDITOR
        [MethodAPI]
        [APIDescriptionCN("异步加载场景")]
        [APIDescriptionEN("Load Scene Sync")]
        [APIExampleCode(@"
mResLoader.LoadSceneAsync(""BattleScene"");
// Or 
mResLoader.LoadSceneAsync(""BattleSceneBundle"",""BattleScene"");


mResLoader.LoadSceneAsync(""BattleScene"",LoadSceneMode.Additive);
//
mResLoader.LoadSceneAsync(""BattleScene"",LoadSceneMode.Additive,LocalPhysicsMode.Physics2D);


mResLoader.LoadSceneAsync(""BattleScene"",(operation)=>
{
    Debug.Log(operation.isDone);
});
")]
#endif
        public static void LoadSceneAsync(this IResLoader self, string sceneName,
            LoadSceneMode loadSceneMode =
                LoadSceneMode.Single, LocalPhysicsMode physicsMode = LocalPhysicsMode.None,
            Action<AsyncOperation> onStartLoading = null)
        {

            var resSearchKey = ResSearchKeys.Allocate(sceneName);
            self.LoadSceneAsync(resSearchKey,loadSceneMode,physicsMode,onStartLoading);
            resSearchKey.Recycle2Cache();
        }
        
        public static void LoadSceneAsync(this IResLoader self, string bundleName,string sceneName,
            LoadSceneMode loadSceneMode =
                LoadSceneMode.Single, LocalPhysicsMode physicsMode = LocalPhysicsMode.None,
            Action<AsyncOperation> onStartLoading = null)
        {

            var resSearchKey = ResSearchKeys.Allocate(sceneName,bundleName);
            self.LoadSceneAsync(resSearchKey,loadSceneMode,physicsMode,onStartLoading);
            resSearchKey.Recycle2Cache();
        }
        

        public static void LoadSceneAsync(this IResLoader self,ResSearchKeys resSearchKeys,
            LoadSceneMode loadSceneMode =
                LoadSceneMode.Single, LocalPhysicsMode physicsMode = LocalPhysicsMode.None,
            Action<AsyncOperation> onStartLoading = null)
        {
            // 使用 YooAsset 异步加载场景
            var package = YooAssetBridge.DefaultPackage;
            if (package == null)
            {
                Debug.LogError("[IResLoaderExtensions] YooAsset not initialized!");
                return;
            }

            var handle = package.LoadSceneAsync(resSearchKeys.AssetName, loadSceneMode, physicsMode);
            handle.Completed += (op) =>
            {
                if (op.Status == YooAsset.EOperationStatus.Succeeded)
                {
                    onStartLoading?.Invoke(null);
                }
                else
                {
                    Debug.LogError($"[IResLoaderExtensions] LoadSceneAsync failed: {resSearchKeys.AssetName}, Error: {op.Error}");
                }
            };
        }

        [Obsolete("请使用 LoadSync<Sprite>,use LoadSync<Sprite> instead", true)]
        public static Sprite LoadSprite(this IResLoader self, string spriteName) => self.LoadSync<Sprite>(spriteName);

        [Obsolete("请使用 LoadSync<Sprite>,use LoadSync<Sprite> instead", true)]
        public static Sprite LoadSprite(this IResLoader self, string bundleName, string spriteName) =>
            self.LoadSync<Sprite>(bundleName, spriteName);
    }
}