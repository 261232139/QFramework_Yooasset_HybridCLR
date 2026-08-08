using System;
using System.Collections;
using UnityEngine;
using YooAsset;

namespace Game.Level.Data
{
    public static class LevelConfigLoader
    {
        public static IEnumerator LoadAsync(string levelId, Action<LevelConfig> completed)
        {
            if (string.IsNullOrWhiteSpace(levelId))
            {
                Debug.LogError("[LevelConfigLoader] Level id cannot be empty.");
                completed?.Invoke(null);
                yield break;
            }

            var package = QFramework.YooAssetBridge.DefaultPackage;
            if (package == null)
            {
                completed?.Invoke(null);
                yield break;
            }

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
            }

            handle.Dispose();
            completed?.Invoke(config);
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
