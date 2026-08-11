using Game.Level.Data;
using UnityEditor;
using UnityEngine;

namespace Game.Level.Editor
{
    public class ToolPanelView
    {
        private readonly LevelEditorData editorData;
        private readonly BoardEditorView boardView;
        private Vector2 scrollPosition;

        public ToolPanelView(LevelEditorData editorData, BoardEditorView boardView)
        {
            this.editorData = editorData;
            this.boardView = boardView;
        }

        public void Draw()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(200));
            DrawPieceTools();
            EditorGUILayout.EndVertical();
        }

        private void DrawPieceTools()
        {
            GUILayout.Label("Piece Tools", EditorStyles.boldLabel);
            boardView.SelectedPieceType = (PieceType)EditorGUILayout.EnumPopup("Type", boardView.SelectedPieceType);
            boardView.NewPieceMovable = EditorGUILayout.Toggle("Movable", boardView.NewPieceMovable);

            EditorGUILayout.Space();
            DrawSelectedCellTools();
            EditorGUILayout.Space();

            GUILayout.Label("All Pieces", EditorStyles.boldLabel);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            for (var i = editorData.CurrentConfig.pieces.Count - 1; i >= 0; i--)
            {
                var piece = editorData.CurrentConfig.pieces[i];
                var style = boardView.SelectedPiece == piece ? EditorStyles.helpBox : GUI.skin.box;
                EditorGUILayout.BeginVertical(style);
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button($"{piece.id} ({piece.pieceType})", EditorStyles.miniButton))
                    boardView.SelectPiece(piece);

                var originalColor = GUI.backgroundColor;
                GUI.backgroundColor = new Color(1f, 0.6f, 0.6f);
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    boardView.SelectPiece(piece);
                    boardView.RemovePieceFromSelectedCell();
                }
                GUI.backgroundColor = originalColor;

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Clear All Pieces") &&
                EditorUtility.DisplayDialog("Clear All Pieces", "Remove all pieces?", "Yes", "No"))
            {
                editorData.RecordUndo();
                editorData.CurrentConfig.pieces.Clear();
                boardView.ClearSelection();
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Tips:\n• Left-click blank position to create a cell\n• Left-click cell to select it\n• Right-click cell to delete cell and piece\n• Movable off means fixed piece",
                MessageType.Info);
        }

        private void DrawSelectedCellTools()
        {
            if (!boardView.HasSelectedBoardCell)
            {
                EditorGUILayout.HelpBox("Select a board cell to edit its piece.", MessageType.Info);
                return;
            }

            GUILayout.Label("Selected Cell", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Position", boardView.SelectedCell.Value.ToString());

            if (boardView.SelectedPiece == null)
            {
                if (GUILayout.Button("Add Piece"))
                    boardView.AddPieceToSelectedCell();
            }
            else
            {
                EditorGUILayout.LabelField("Piece ID", boardView.SelectedPiece.id);
                EditorGUI.BeginChangeCheck();
                boardView.SelectedPiece.pieceType =
                    (PieceType)EditorGUILayout.EnumPopup("Type", boardView.SelectedPiece.pieceType);
                boardView.SelectedPiece.isMovable =
                    EditorGUILayout.Toggle("Movable", boardView.SelectedPiece.isMovable);
                if (EditorGUI.EndChangeCheck())
                    editorData.IsDirty = true;

                var originalColor = GUI.backgroundColor;
                GUI.backgroundColor = new Color(1f, 0.5f, 0.5f);
                if (GUILayout.Button("Remove Piece"))
                    boardView.RemovePieceFromSelectedCell();
                GUI.backgroundColor = originalColor;
            }

            EditorGUILayout.EndVertical();
        }
    }
}
