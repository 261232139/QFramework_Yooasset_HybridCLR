/****************************************************************************
 * Copyright (c) 2024 liangxiegame UNDER MIT License
 * 
 * Boot 场景一键设置工具
 * 使用方式: Tools → GameLauncher → Setup Boot Scene
 ****************************************************************************/

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using Launch;

namespace GameLauncher.Editor
{
    public static class BootSceneSetup
    {
        private const string BOOT_SCENE_PATH = "Assets/Scenes/Boot.unity";
        private const string PREFAB_DIR = "Assets/Resources/Prefabs";
        private const string PREFAB_PATH = "Assets/Resources/Prefabs/GameLauncher.prefab";

        [MenuItem("Tools/GameLauncher/Setup Boot Scene", false, 1)]
        public static void SetupBootScene()
        {
            // 1. 确保 Boot 场景存在
            EnsureBootScene();

            // 2. 创建 GameLauncher 预制体
            var prefab = CreateGameLauncherPrefab();

            // 3. 打开 Boot 场景并放置预制体
            EditorSceneManager.OpenScene(BOOT_SCENE_PATH);
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.name = "GameLauncher";
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

            Debug.Log($"[BootSceneSetup] ✅ 设置完成! GameLauncher 已放置到 Boot 场景");
            EditorGUIUtility.PingObject(prefab);
        }

        [MenuItem("Tools/GameLauncher/Create GameLauncher Prefab Only", false, 2)]
        public static void CreatePrefabOnly()
        {
            var prefab = CreateGameLauncherPrefab();
            Debug.Log($"[BootSceneSetup] ✅ 预制体已创建: {PREFAB_PATH}");
            EditorGUIUtility.PingObject(prefab);
        }

        private static void EnsureBootScene()
        {
            if (!File.Exists(BOOT_SCENE_PATH))
            {
                // 创建新场景
                var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                scene.name = "Boot";
                EditorSceneManager.SaveScene(scene, BOOT_SCENE_PATH);
                Debug.Log($"[BootSceneSetup] Boot 场景已创建: {BOOT_SCENE_PATH}");
            }
        }

        private static GameObject CreateGameLauncherPrefab()
        {
            // 检查是否已存在
            if (File.Exists(PREFAB_PATH))
            {
                var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
                if (existing != null && existing.GetComponent<Launch.GameLauncher>() != null)
                {
                    Debug.Log("[BootSceneSetup] 预制体已存在，使用现有预制体");
                    return existing;
                }
            }

            // 创建临时 GameObject
            var tempGO = new GameObject("GameLauncher", typeof(Launch.GameLauncher));

            // 确保目录存在
            if (!Directory.Exists(PREFAB_DIR))
            {
                Directory.CreateDirectory(PREFAB_DIR);
            }

            // 保存为预制体
            var prefab = PrefabUtility.SaveAsPrefabAsset(tempGO, PREFAB_PATH);
            Object.DestroyImmediate(tempGO);

            Debug.Log($"[BootSceneSetup] 预制体已创建: {PREFAB_PATH}");
            return prefab;
        }

        [MenuItem("Tools/GameLauncher/Set Boot as Startup Scene", false, 10)]
        public static void SetBootAsStartupScene()
        {
            if (!File.Exists(BOOT_SCENE_PATH))
            {
                Debug.LogError("[BootSceneSetup] Boot 场景不存在，请先执行 Setup Boot Scene");
                return;
            }

            // 将 Boot 场景添加到 Build Settings
            var scenes = EditorBuildSettings.scenes;
            bool found = false;
            foreach (var s in scenes)
            {
                if (s.path == BOOT_SCENE_PATH)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
                System.Array.Copy(scenes, newScenes, scenes.Length);
                newScenes[scenes.Length] = new EditorBuildSettingsScene(BOOT_SCENE_PATH, true);
                EditorBuildSettings.scenes = newScenes;
                Debug.Log("[BootSceneSetup] Boot 场景已添加到 Build Settings");
            }

            // 设置为 Play Mode 启动场景
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(BOOT_SCENE_PATH);
            Debug.Log("[BootSceneSetup] ✅ Boot 场景已设置为启动场景");
        }
    }
}