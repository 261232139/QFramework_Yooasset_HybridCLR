using Game.Level.Data;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Game.Level.Editor
{
    public class BatchGenerateWindow : EditorWindow
    {
        private int generateCount = 10;
        private string levelIdPrefix = "level_batch_";
        private LevelDifficulty[] selectedDifficulties = new[] { LevelDifficulty.Easy, LevelDifficulty.Normal };
        private SceneType[] selectedScenes = new[] { SceneType.Forest };
        private int minWidth = 4;
        private int maxWidth = 7;
        private int minHeight = 4;
        private int maxHeight = 9;
        private Vector2 scrollPosition;
        private List<string> generatedLevels = new List<string>();

        [MenuItem("Tools/Batch Level Generator")]
        public static void ShowWindow()
        {
            var window = GetWindow<BatchGenerateWindow>("Batch Generator");
            window.minSize = new Vector2(450, 500);
            window.Show();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("Batch Level Generation", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Generate multiple random levels at once", MessageType.Info);

            EditorGUILayout.Space();

            generateCount = EditorGUILayout.IntField("Generate Count", generateCount);
            levelIdPrefix = EditorGUILayout.TextField("Level ID Prefix", levelIdPrefix);

            EditorGUILayout.Space();
            GUILayout.Label("Board Size Range", EditorStyles.boldLabel);
            minWidth = EditorGUILayout.IntSlider("Min Width", minWidth, 4, 7);
            maxWidth = EditorGUILayout.IntSlider("Max Width", maxWidth, minWidth, 7);
            minHeight = EditorGUILayout.IntSlider("Min Height", minHeight, 4, 9);
            maxHeight = EditorGUILayout.IntSlider("Max Height", maxHeight, minHeight, 9);

            EditorGUILayout.Space();

            if (GUILayout.Button("Generate", GUILayout.Height(40)))
            {
                BatchGenerate();
            }

            if (generatedLevels.Count > 0)
            {
                EditorGUILayout.Space();
                GUILayout.Label($"Generated {generatedLevels.Count} Levels:", EditorStyles.boldLabel);

                foreach (var levelId in generatedLevels)
                {
                    EditorGUILayout.LabelField(levelId);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void BatchGenerate()
        {
            generatedLevels.Clear();
            var generator = new RandomLevelGenerator();
            var successCount = 0;

            for (var i = 0; i < generateCount; i++)
            {
                var request = new LevelGenerationRequest
                {
                    levelId = $"{levelIdPrefix}{i:D3}",
                    targetWidth = Random.Range(minWidth, maxWidth + 1),
                    targetHeight = Random.Range(minHeight, maxHeight + 1),
                    targetDifficulty = selectedDifficulties[Random.Range(0, selectedDifficulties.Length)],
                    sceneType = selectedScenes[Random.Range(0, selectedScenes.Length)]
                };

                var config = generator.Generate(request);
                if (config != null && SerializationManager.Save(config, config.levelId, false))
                {
                    generatedLevels.Add(config.levelId);
                    successCount++;
                }
            }

            EditorUtility.DisplayDialog("Batch Generation Complete", $"Successfully generated {successCount}/{generateCount} levels", "OK");
        }
    }
}
