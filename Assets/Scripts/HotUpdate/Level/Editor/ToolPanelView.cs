using Game.Level.Data;
using UnityEditor;
using UnityEngine;

namespace Game.Level.Editor
{
    public class ToolPanelView
    {
        private LevelEditorData editorData;
        private BoardEditorView boardView;
        private Vector2 scrollPosition;

        public ToolPanelView(LevelEditorData editorData, BoardEditorView boardView)
        {
            this.editorData = editorData;
            this.boardView = boardView;
        }

        public void Draw()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(200));

            GUILayout.Label("Tools", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            var modeIndex = (int)boardView.CurrentMode;
            modeIndex = GUILayout.SelectionGrid(modeIndex, new[] { "Board", "Piece" }, 2);
            boardView.CurrentMode = (BoardEditorView.EditMode)modeIndex;
            if (EditorGUI.EndChangeCheck())
            {
                boardView.ClearSelection();
            }

            EditorGUILayout.Space();

            if (boardView.CurrentMode == BoardEditorView.EditMode.Board)
                DrawBoardTools();
            else
                DrawPieceTools();

            EditorGUILayout.EndVertical();
        }

        private void DrawBoardTools()
        {
            GUILayout.Label("Board Tools", EditorStyles.boldLabel);

            EditorGUILayout.Space();
            GUILayout.Label("Paint Type", EditorStyles.boldLabel);
            var paintIndex = (int)boardView.PaintCellType;
            paintIndex = GUILayout.SelectionGrid(paintIndex, new[] { "Void", "Playable" }, 1);
            boardView.PaintCellType = (BoardCellType)paintIndex;

            EditorGUILayout.Space();

            if (GUILayout.Button("Fill All Playable"))
            {
                editorData.RecordUndo();
                boardView.FillBoard(BoardCellType.Playable);
            }

            if (GUILayout.Button("Clear All"))
            {
                editorData.RecordUndo();
                boardView.FillBoard(BoardCellType.Void);
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Tips:\n• Left-click to paint cell\n• Right-click for menu\n• Delete key to remove cell", MessageType.Info);
        }

        private void DrawPieceTools()
        {
            GUILayout.Label("Piece Tools", EditorStyles.boldLabel);

            boardView.SelectedPieceType = (PieceType)EditorGUILayout.EnumPopup("Type", boardView.SelectedPieceType);
            boardView.NewPieceMovable = EditorGUILayout.Toggle("Movable", boardView.NewPieceMovable);

            EditorGUILayout.Space();

            if (boardView.SelectedPiece != null)
            {
                GUILayout.Label("Selected Piece", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("ID", boardView.SelectedPiece.id);
                EditorGUILayout.LabelField("Position", boardView.SelectedPiece.position.ToString());

                EditorGUI.BeginChangeCheck();
                boardView.SelectedPiece.pieceType = (PieceType)EditorGUILayout.EnumPopup("Type", boardView.SelectedPiece.pieceType);
                boardView.SelectedPiece.isMovable = EditorGUILayout.Toggle("Movable", boardView.SelectedPiece.isMovable);
                if (EditorGUI.EndChangeCheck())
                    editorData.IsDirty = true;

                EditorGUILayout.Space();
                
                var originalColor = GUI.backgroundColor;
                GUI.backgroundColor = new Color(1f, 0.5f, 0.5f);
                if (GUILayout.Button("Delete Piece (Del)"))
                {
                    editorData.RecordUndo();
                    editorData.CurrentConfig.pieces.Remove(boardView.SelectedPiece);
                    boardView.SelectedPiece = null;
                }
                GUI.backgroundColor = originalColor;
                
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space();
            GUILayout.Label("All Pieces", EditorStyles.boldLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (var piece in editorData.CurrentConfig.pieces)
            {
                var style = boardView.SelectedPiece == piece ? EditorStyles.helpBox : GUI.skin.box;
                EditorGUILayout.BeginVertical(style);

                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button($"{piece.id} ({piece.pieceType})", EditorStyles.miniButton))
                {
                    boardView.SelectedPiece = piece;
                }
                
                var originalColor = GUI.backgroundColor;
                GUI.backgroundColor = new Color(1f, 0.6f, 0.6f);
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    editorData.RecordUndo();
                    editorData.CurrentConfig.pieces.Remove(piece);
                    if (boardView.SelectedPiece == piece)
                        boardView.SelectedPiece = null;
                }
                GUI.backgroundColor = originalColor;
                
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Clear All Pieces"))
            {
                if (EditorUtility.DisplayDialog("Clear All Pieces", "Remove all pieces?", "Yes", "No"))
                {
                    editorData.RecordUndo();
                    editorData.CurrentConfig.pieces.Clear();
                    boardView.SelectedPiece = null;
                }
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Tips:\n• Click empty cell to add piece\n• Click piece to select\n• Right-click piece for menu\n• Delete key to remove piece", MessageType.Info);
        }
    }
}
