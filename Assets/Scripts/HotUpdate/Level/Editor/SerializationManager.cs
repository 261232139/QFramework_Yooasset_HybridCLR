using System.IO;
using Game.Level.Data;
using UnityEditor;
using UnityEngine;

namespace Game.Level.Editor
{
    public static class SerializationManager
    {
        private const string ConfigPath = "Assets/Game/Config/Level/";

        public static bool Save(LevelConfig config, string fileName, bool createBackup = true)
        {
            var error = config == null ? "config is null" : null;
            if (config == null || !config.Validate(out error))
            {
                Debug.LogError($"[SerializationManager] Cannot save config: {error ?? "config is null"}");
                return false;
            }

            if (!Directory.Exists(ConfigPath))
                Directory.CreateDirectory(ConfigPath);

            var filePath = Path.Combine(ConfigPath, $"{fileName}.json");
            if (createBackup && File.Exists(filePath))
                File.Copy(filePath, Path.Combine(ConfigPath, $"{fileName}.backup"), true);

            try
            {
                File.WriteAllText(filePath, JsonUtility.ToJson(config, true));
                AssetDatabase.Refresh();
                Debug.Log($"[SerializationManager] Saved YooAsset level config to {filePath}");
                return true;
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"[SerializationManager] Failed to save config: {exception.Message}");
                return false;
            }
        }

        public static LevelConfig Load(string fileName)
        {
            var filePath = GetFullPath(fileName);
            if (!File.Exists(filePath))
            {
                Debug.LogError($"[SerializationManager] File not found: {filePath}");
                return null;
            }

            try
            {
                var config = JsonUtility.FromJson<LevelConfig>(File.ReadAllText(filePath));
                var error = config == null ? "config is null" : null;
            if (config == null || !config.Validate(out error))
                {
                    Debug.LogError($"[SerializationManager] Invalid config: {error ?? "deserialized to null"}");
                    return null;
                }
                return config;
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"[SerializationManager] Failed to load config: {exception.Message}");
                return null;
            }
        }

        public static string[] GetAllLevelFiles()
        {
            if (!Directory.Exists(ConfigPath))
                return new string[0];

            var files = Directory.GetFiles(ConfigPath, "*.json");
            var levelIds = new string[files.Length];
            for (var i = 0; i < files.Length; i++)
                levelIds[i] = Path.GetFileNameWithoutExtension(files[i]);
            return levelIds;
        }

        public static string GetFullPath(string fileName) => Path.Combine(ConfigPath, $"{fileName}.json");
        public static bool Exists(string fileName) => File.Exists(GetFullPath(fileName));

    }
}
