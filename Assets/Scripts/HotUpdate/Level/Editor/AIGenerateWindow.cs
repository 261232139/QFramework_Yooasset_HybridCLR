using Game.Level.Data;
using UnityEditor;
using UnityEngine;

namespace Game.Level.Editor
{
    public class AIGenerateWindow : EditorWindow
    {
        private LevelEditorData editorData;
        private LevelGenerationRequest request;
        private Vector2 scrollPosition;
        private string generatedJson = "";
        private bool showJsonInput = false;

        public static void ShowWindow(LevelEditorData editorData)
        {
            var window = GetWindow<AIGenerateWindow>("AI Generate Level");
            window.editorData = editorData;
            window.InitializeRequest();
            window.minSize = new Vector2(500, 600);
            window.Show();
        }

        private void InitializeRequest()
        {
            request = new LevelGenerationRequest
            {
                levelId = $"ai_gen_{System.DateTime.Now:yyyyMMdd_HHmmss}",
                targetWidth = editorData.CurrentConfig.board.width,
                targetHeight = editorData.CurrentConfig.board.height,
                targetDifficulty = editorData.CurrentConfig.difficulty,
                sceneType = editorData.CurrentConfig.sceneType
            };
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("AI Level Generation", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DrawRequestParameters();
            EditorGUILayout.Space();

            DrawActionButtons();
            EditorGUILayout.Space();

            DrawJsonInterface();

            EditorGUILayout.EndScrollView();
        }

        private void DrawRequestParameters()
        {
            GUILayout.Label("Generation Parameters", EditorStyles.boldLabel);

            request.levelId = EditorGUILayout.TextField("Level ID", request.levelId);
            request.sceneType = (SceneType)EditorGUILayout.EnumPopup("Scene Type", request.sceneType);
            request.targetDifficulty = (LevelDifficulty)EditorGUILayout.EnumPopup("Difficulty", request.targetDifficulty);

            EditorGUILayout.Space();
            GUILayout.Label("Board Size", EditorStyles.boldLabel);

            request.targetWidth = EditorGUILayout.IntSlider("Width", request.targetWidth, request.minWidth, request.maxWidth);
            request.targetHeight = EditorGUILayout.IntSlider("Height", request.targetHeight, request.minHeight, request.maxHeight);

            EditorGUILayout.Space();
            GUILayout.Label("Constraints", EditorStyles.boldLabel);

            var constraints = request.constraints;
            constraints.minPieceCount = EditorGUILayout.IntField("Min Piece Count", constraints.minPieceCount);
            constraints.maxPieceCount = EditorGUILayout.IntField("Max Piece Count", constraints.maxPieceCount);
            constraints.movablePieceRatio = EditorGUILayout.Slider("Movable Piece Ratio", constraints.movablePieceRatio, 0f, 1f);
        }

        private void DrawActionButtons()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Copy Parameters to Clipboard", GUILayout.Height(30)))
            {
                CopyParametersToClipboard();
            }

            if (GUILayout.Button("Use Random Generator", GUILayout.Height(30)))
            {
                UseRandomGenerator();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(showJsonInput ? "Hide JSON Input" : "Show JSON Input", GUILayout.Height(25)))
            {
                showJsonInput = !showJsonInput;
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawJsonInterface()
        {
            if (!showJsonInput)
                return;

            GUILayout.Label("Generated Level JSON", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Paste the generated LevelConfig JSON below and click 'Load from JSON'",
                MessageType.Info
            );

            generatedJson = EditorGUILayout.TextArea(generatedJson, GUILayout.Height(200));

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Load from JSON", GUILayout.Height(30)))
            {
                LoadFromJson();
            }

            if (GUILayout.Button("Clear", GUILayout.Height(30)))
            {
                generatedJson = "";
            }

            EditorGUILayout.EndHorizontal();
        }

        private void CopyParametersToClipboard()
        {
            var json = JsonUtility.ToJson(request, true);
            EditorGUIUtility.systemCopyBuffer = json;

            var message = "Generation parameters copied to clipboard!\n\n" +
                         "Instructions for Cursor AI:\n" +
                         "1. Use these parameters to generate a LevelConfig\n" +
                         "2. Return the complete LevelConfig JSON\n" +
                         "3. User will paste it into 'Show JSON Input' area\n\n" +
                         "Parameters:\n" + json;

            EditorUtility.DisplayDialog("Parameters Copied", message, "OK");
        }

        private void UseRandomGenerator()
        {
            if (!request.Validate(out var error))
            {
                EditorUtility.DisplayDialog("Invalid Request", error, "OK");
                return;
            }

            var generator = new RandomLevelGenerator();
            var config = generator.Generate(request);

            if (config != null)
            {
                editorData.LoadConfig(config, null);
                editorData.IsDirty = true;
                EditorUtility.DisplayDialog("Success", "Random level generated successfully!", "OK");
                Close();
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Failed to generate level", "OK");
            }
        }

        private void LoadFromJson()
        {
            if (string.IsNullOrWhiteSpace(generatedJson))
            {
                EditorUtility.DisplayDialog("Error", "JSON input is empty", "OK");
                return;
            }

            try
            {
                var config = JsonUtility.FromJson<LevelConfig>(generatedJson);

                if (config == null)
                {
                    EditorUtility.DisplayDialog("Error", "Failed to parse JSON", "OK");
                    return;
                }

                if (!config.Validate(out var error))
                {
                    EditorUtility.DisplayDialog("Validation Error", $"Generated config is invalid:\n{error}", "OK");
                    return;
                }

                editorData.LoadConfig(config, null);
                editorData.IsDirty = true;
                EditorUtility.DisplayDialog("Success", "Level loaded successfully from JSON!", "OK");
                Close();
            }
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to parse JSON:\n{ex.Message}", "OK");
            }
        }
    }
}
