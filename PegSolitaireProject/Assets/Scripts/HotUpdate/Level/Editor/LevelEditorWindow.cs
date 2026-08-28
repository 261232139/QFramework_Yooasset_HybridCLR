using Game.Level.Data;
using UnityEditor;
using UnityEngine;

namespace Game.Level.Editor
{
    public class LevelEditorWindow : EditorWindow
    {
        private LevelEditorData editorData;
        private BoardEditorView boardView;
        private ConfigPanelView configPanel;
        private ToolPanelView toolPanel;

        [MenuItem("Tools/Level Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<LevelEditorWindow>("Level Editor");
            window.minSize = new Vector2(800, 600);
            window.Show();
        }

        private void OnEnable()
        {
            editorData = new LevelEditorData();
            boardView = new BoardEditorView(editorData);
            configPanel = new ConfigPanelView(editorData);
            toolPanel = new ToolPanelView(editorData, boardView);
        }

        private void OnGUI()
        {
            DrawToolbar();
            DrawMainContent();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("New", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                if (ConfirmDiscardChanges())
                {
                    editorData.CreateNew();
                    boardView.ClearSelection();
                }
            }

            if (GUILayout.Button("Open", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                if (ConfirmDiscardChanges())
                    OpenLevel();
            }

            if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(50)))
                SaveLevel(false);

            if (GUILayout.Button("Save As", EditorStyles.toolbarButton, GUILayout.Width(60)))
                SaveLevel(true);

            GUILayout.Space(20);

            EditorGUI.BeginDisabledGroup(!editorData.CanUndo());
            if (GUILayout.Button("Undo", EditorStyles.toolbarButton, GUILayout.Width(50)))
                editorData.Undo();
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(!editorData.CanRedo());
            if (GUILayout.Button("Redo", EditorStyles.toolbarButton, GUILayout.Width(50)))
                editorData.Redo();
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(20);

            if (GUILayout.Button("AI Generate", EditorStyles.toolbarButton, GUILayout.Width(80)))
                ShowAIGenerateDialog();

            GUILayout.FlexibleSpace();

            if (editorData.IsDirty)
                GUILayout.Label("*", EditorStyles.toolbarButton);

            EditorGUILayout.EndHorizontal();
        }

        private void DrawMainContent()
        {
            EditorGUILayout.BeginHorizontal();
            configPanel.Draw();
            boardView.Draw();
            toolPanel.Draw();
            EditorGUILayout.EndHorizontal();
        }

        private void ShowAIGenerateDialog()
        {
            AIGenerateWindow.ShowWindow(editorData);
        }

        private bool ConfirmDiscardChanges()
        {
            if (!editorData.IsDirty)
                return true;
            return EditorUtility.DisplayDialog("Unsaved Changes", "Discard unsaved changes?", "Yes", "No");
        }

        private void OpenLevel()
        {
            var levelFiles = SerializationManager.GetAllLevelFiles();
            if (levelFiles.Length == 0)
            {
                EditorUtility.DisplayDialog("No Levels", "No level files found", "OK");
                return;
            }

            var menu = new GenericMenu();
            foreach (var levelId in levelFiles)
            {
                menu.AddItem(new GUIContent(levelId), false, () =>
                {
                    var config = SerializationManager.Load(levelId);
                    if (config != null)
                    {
                        editorData.LoadConfig(config, SerializationManager.GetFullPath(levelId));
                        boardView.ClearSelection();
                        Repaint();
                    }
                });
            }
            menu.ShowAsContext();
        }

        private void SaveLevel(bool saveAs)
        {
            var messages = ValidationSystem.Validate(editorData.CurrentConfig);
            if (ValidationSystem.HasErrors(messages))
            {
                EditorUtility.DisplayDialog("Validation Failed", "Cannot save: level has errors", "OK");
                return;
            }

            var fileName = editorData.CurrentConfig.levelId;
            if (saveAs || string.IsNullOrEmpty(editorData.CurrentFilePath))
            {
                fileName = EditorUtility.SaveFilePanel("Save Level", "", fileName, "json");
                if (string.IsNullOrEmpty(fileName))
                    return;
                fileName = System.IO.Path.GetFileNameWithoutExtension(fileName);
            }

            if (SerializationManager.Save(editorData.CurrentConfig, fileName))
            {
                editorData.CurrentFilePath = SerializationManager.GetFullPath(fileName);
                editorData.IsDirty = false;
                EditorUtility.DisplayDialog("Success", $"Level saved: {fileName}", "OK");
            }
        }
    }
}
