/****************************************************************************
 * LevelConfigLoader — 关卡配置加载器（带缓存）
 * 
 * 职责：
 * 1. 通过 YooAsset 异步加载关卡配置
 * 2. 缓存已加载的配置，避免重复加载
 * 3. 提供缓存管理功能（清除、预加载、重载）
 ****************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace Game.Level.Data
{
    public static class LevelConfigLoader
    {
        private static readonly Dictionary<string, LevelConfig> sConfigCache = new Dictionary<string, LevelConfig>();

        public static bool HasCache(string levelId)
        {
            return sConfigCache.ContainsKey(levelId);
        }

        public static LevelConfig GetCached(string levelId)
        {
            return sConfigCache.TryGetValue(levelId, out var config) ? config : null;
        }

        public static IEnumerator LoadAsync(string levelId, Action<LevelConfig> completed, bool forceReload = false)
        {
            if (string.IsNullOrWhiteSpace(levelId))
            {
                Debug.LogError("[LevelConfigLoader] Level id cannot be empty.");
                completed?.Invoke(null);
                yield break;
            }

            if (!forceReload && sConfigCache.TryGetValue(levelId, out var cachedConfig))
            {
                Debug.Log($"[LevelConfigLoader] 从缓存加载配置: {levelId}");
                completed?.Invoke(cachedConfig);
                yield break;
            }

            var package = QFramework.YooAssetBridge.DefaultPackage;
            if (package == null)
            {
                Debug.LogError("[LevelConfigLoader] YooAsset DefaultPackage is null.");
                completed?.Invoke(null);
                yield break;
            }

            Debug.Log($"[LevelConfigLoader] 从资源包加载配置: {levelId}");
            var handle = package.LoadAssetAsync<TextAsset>(levelId);
            yield return handle;

            LevelConfig config = null;
            if (handle.Status != EOperationStatus.Succeeded || handle.AssetObject == null)
            {
                Debug.LogError($"[LevelConfigLoader] YooAsset load failed for '{levelId}': {handle.Error}");
            }
            else
            {
                config = Parse(levelId, ((TextAsset)handle.AssetObject).text);
                
                if (config != null)
                {
                    CacheConfig(levelId, config);
                }
            }

            handle.Dispose();
            completed?.Invoke(config);
        }

        public static IEnumerator PreloadAsync(string levelId, Action<bool> completed = null)
        {
            if (HasCache(levelId))
            {
                Debug.Log($"[LevelConfigLoader] 配置已在缓存中: {levelId}");
                completed?.Invoke(true);
                yield break;
            }

            bool success = false;
            yield return LoadAsync(levelId, config =>
            {
                success = config != null;
            });

            completed?.Invoke(success);
        }

        public static IEnumerator PreloadBatchAsync(string[] levelIds, Action<int, int> onProgress = null, Action completed = null)
        {
            int total = levelIds.Length;
            int loaded = 0;

            foreach (var levelId in levelIds)
            {
                yield return PreloadAsync(levelId, success =>
                {
                    if (success) loaded++;
                    onProgress?.Invoke(loaded, total);
                });
            }

            Debug.Log($"[LevelConfigLoader] 批量预加载完成: {loaded}/{total}");
            completed?.Invoke();
        }

        private static void CacheConfig(string levelId, LevelConfig config)
        {
            if (sConfigCache.ContainsKey(levelId))
            {
                sConfigCache[levelId] = config;
                Debug.Log($"[LevelConfigLoader] 更新缓存: {levelId}");
            }
            else
            {
                sConfigCache.Add(levelId, config);
                Debug.Log($"[LevelConfigLoader] 添加到缓存: {levelId}");
            }
        }

        public static void ClearCache()
        {
            int count = sConfigCache.Count;
            sConfigCache.Clear();
            Debug.Log($"[LevelConfigLoader] 清除缓存: {count} 个配置");
        }

        public static void RemoveCache(string levelId)
        {
            if (sConfigCache.Remove(levelId))
            {
                Debug.Log($"[LevelConfigLoader] 移除缓存: {levelId}");
            }
        }

        public static int GetCacheCount()
        {
            return sConfigCache.Count;
        }

        public static string[] GetCachedLevelIds()
        {
            var ids = new string[sConfigCache.Count];
            sConfigCache.Keys.CopyTo(ids, 0);
            return ids;
        }

        private static LevelConfig Parse(string levelId, string json)
        {
            try
            {
                var config = JsonUtility.FromJson<LevelConfig>(json);
                if (config == null)
                {
                    Debug.LogError($"[LevelConfigLoader] Invalid config '{levelId}': JSON deserialized to null.");
                    return null;
                }

                if (!config.Validate(out var error))
                {
                    Debug.LogError($"[LevelConfigLoader] Invalid config '{levelId}': {error}");
                    return null;
                }

                if (config.levelId != levelId)
                {
                    Debug.LogError($"[LevelConfigLoader] Address '{levelId}' does not match id '{config.levelId}'.");
                    return null;
                }

                return config;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[LevelConfigLoader] Failed to parse '{levelId}': {exception.Message}");
                return null;
            }
        }
    }
}
