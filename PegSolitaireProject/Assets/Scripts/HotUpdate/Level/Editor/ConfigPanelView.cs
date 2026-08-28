using Game.Level.Data;
using UnityEditor;
using UnityEngine;

namespace Game.Level.Editor
{
    public class ConfigPanelView
    {
        private LevelEditorData editorData;
        private Vector2 scrollPosition;

        public ConfigPanelView(LevelEditorData editorData)
        {
            this.editorData = editorData;
        }

        public void Draw()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(250));
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawConfigPanel();
            DrawValidationPanel();

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawConfigPanel()
        {
            GUILayout.Label("Level Configuration", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            var config = editorData.CurrentConfig;

            config.levelId = EditorGUILayout.TextField("Level ID", config.levelId);
            config.sceneType = (SceneType)EditorGUILayout.EnumPopup("Scene Type", config.sceneType);
            config.difficulty = (LevelDifficulty)EditorGUILayout.EnumPopup("Difficulty", config.difficulty);

            EditorGUILayout.Space();
            GUILayout.Label("Board Size", EditorStyles.boldLabel);

            var newWidth = EditorGUILayout.IntSlider("Width", config.board.width, 4, 7);
            var newHeight = EditorGUILayout.IntSlider("Height", config.board.height, 4, 9);

            if (newWidth != config.board.width || newHeight != config.board.height)
            {
                editorData.RecordUndo();
                ResizeBoard(newWidth, newHeight);
            }

            EditorGUILayout.Space();
            GUILayout.Label("Statistics", EditorStyles.boldLabel);

            var movableCount = CountMovablePieces();

            EditorGUILayout.LabelField("Board Cells", CountBoardCells().ToString());
            EditorGUILayout.LabelField("Total Pieces", config.pieces.Count.ToString());
            EditorGUILayout.LabelField("Movable Pieces", movableCount.ToString());

            if (EditorGUI.EndChangeCheck())
                editorData.IsDirty = true;
        }

        private void DrawValidationPanel()
        {
            EditorGUILayout.Space();
            GUILayout.Label("Validation", EditorStyles.boldLabel);

            var messages = ValidationSystem.Validate(editorData.CurrentConfig);

            if (messages.Count == 0)
            {
                EditorGUILayout.HelpBox("No issues found", MessageType.Info);
            }
            else
            {
                foreach (var msg in messages)
                {
                    var messageType = msg.Level switch
                    {
                        ValidationLevel.Error => MessageType.Error,
                        ValidationLevel.Warning => MessageType.Warning,
                        _ => MessageType.Info
                    };
                    EditorGUILayout.HelpBox(msg.Message, messageType);
                }
            }
        }

        private void ResizeBoard(int newWidth, int newHeight)
        {
            var board = editorData.CurrentConfig.board;

            board.width = newWidth;
            board.height = newHeight;

            for (var y = board.rows.Count; y < newHeight; y++)
            {
                var row = new BoardRowData { cells = new System.Collections.Generic.List<BoardCellData>() };
                for (var x = 0; x < newWidth; x++)
                    row.cells.Add(new BoardCellData { isActive = false });
                board.rows.Add(row);
            }

            while (board.rows.Count > newHeight)
                board.rows.RemoveAt(board.rows.Count - 1);

            for (var y = 0; y < board.rows.Count; y++)
            {
                var row = board.rows[y];
                for (var x = row.cells.Count; x < newWidth; x++)
                    row.cells.Add(new BoardCellData { isActive = false });

                while (row.cells.Count > newWidth)
                    row.cells.RemoveAt(row.cells.Count - 1);
            }

            editorData.CurrentConfig.pieces.RemoveAll(p =>
                p.position.x >= newWidth || p.position.y >= newHeight
            );
        }

        private int CountBoardCells()
        {
            var count = 0;
            var board = editorData.CurrentConfig.board;
            for (var y = 0; y < board.height; y++)
            {
                for (var x = 0; x < board.width; x++)
                {
                    if (board.HasCell(x, y))
                        count++;
                }
            }
            return count;
        }

        private int CountMovablePieces()
        {
            var count = 0;
            foreach (var piece in editorData.CurrentConfig.pieces)
            {
                if (piece.isMovable)
                    count++;
            }
            return count;
        }
    }
}
