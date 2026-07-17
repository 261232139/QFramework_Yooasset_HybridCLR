/****************************************************************************
 * Copyright (c) 2024 Game Project UNDER MIT License
 *
 * 关卡配置加载器
 *
 * 职责: 从 JSON 文件加载关卡配置
 * 路径: Assets/Game/LevelConfig/level{id}.json
 ****************************************************************************/

using UnityEngine;
using System.IO;

namespace Game.Level.Data
{
    /// <summary>
    /// 关卡配置加载器
    /// 
    /// 从 Assets/Game/LevelConfig/level{id}.json 加载并解析关卡配置。
    /// </summary>
    public static class LevelConfigLoader
    {
        private const string CONFIG_DIR = "Assets/Game/LevelConfig";
        private const string CONFIG_FILE_PATTERN = "level{0}.json";

        /// <summary>
        /// 加载指定 ID 的关卡配置
        /// 
        /// 返回 null 表示加载失败（文件不存在或 JSON 格式错误）
        /// </summary>
        public static LevelConfig Load(int levelId)
        {
            string fileName = string.Format(CONFIG_FILE_PATTERN, levelId);
            string filePath = Path.Combine(CONFIG_DIR, fileName);

            // 尝试从 Resources 加载（编辑器和运行时都支持）
            string resourcePath = Path.Combine(CONFIG_DIR, Path.GetFileNameWithoutExtension(fileName));
            TextAsset jsonAsset = Resources.Load<TextAsset>(resourcePath);

            if (jsonAsset == null)
            {
                Debug.LogError($"[LevelConfigLoader] 找不到关卡配置: {filePath}");
                return null;
            }

            try
            {
                LevelConfig config = JsonUtility.FromJson<LevelConfig>(jsonAsset.text);

                if (config == null || !config.Validate())
                {
                    Debug.LogError($"[LevelConfigLoader] 关卡配置无效: {levelId}");
                    return null;
                }

                Debug.Log($"[LevelConfigLoader] 加载关卡配置成功: level{levelId}");
                return config;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[LevelConfigLoader] JSON 解析失败: {e.Message}");
                return null;
            }
        }
    }
}
