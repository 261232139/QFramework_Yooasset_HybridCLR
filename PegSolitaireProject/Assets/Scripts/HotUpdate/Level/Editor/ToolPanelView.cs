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
            EditorGUILayout.HelpBox(
                "Grid colors: gray = editor-only empty position; green = board cell; orange/blue = selected position.",
                MessageType.None);
            boardView.SelectedPieceType = (PieceType)EditorGUILayout.EnumPopup("Type", boardView.SelectedPieceType);
            boardView.NewPieceMovable = EditorGUILayout.Toggle("Movable", boardView.NewPieceMovable);
            boardView.NewPieceCanBeJumped = EditorGUILayout.Toggle("Can Be Jumped", boardView.NewPieceCanBeJumped);
            boardView.NewPieceIsRescueTarget = EditorGUILayout.Toggle("Rescue Target", boardView.NewPieceIsRescueTarget);

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
                "Tips:\n• Left-click any grid position to select it\n• Select a blank position, then click Add Cell\n• Select a cell, then click Delete Cell\n• Deleting a cell also deletes its piece\n• Movable off means fixed piece",
                MessageType.Info);
        }

        private void DrawSelectedCellTools()
        {
            if (!boardView.HasSelectedCell)
            {
                EditorGUILayout.HelpBox("Select a grid position to edit the board.", MessageType.Info);
                return;
            }

            GUILayout.Label("Selected Cell", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Position", boardView.SelectedCell.Value.ToString());

            if (!boardView.HasSelectedBoardCell)
            {
                EditorGUILayout.HelpBox("This is a blank position.", MessageType.Info);
                if (GUILayout.Button("Add Cell"))
                    boardView.AddCellAtSelectedPosition();

                EditorGUILayout.EndVertical();
                return;
            }

            var originalColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(1f, 0.6f, 0.6f);
            if (GUILayout.Button("Delete Cell"))
                boardView.RemoveSelectedCell();
            GUI.backgroundColor = originalColor;

            EditorGUILayout.Space(4);
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
                boardView.SelectedPiece.canBeJumped =
                    EditorGUILayout.Toggle("Can Be Jumped", boardView.SelectedPiece.canBeJumped);
                boardView.SelectedPiece.isRescueTarget =
                    EditorGUILayout.Toggle("Rescue Target", boardView.SelectedPiece.isRescueTarget);
                DrawMoveSkillToggles(boardView.SelectedPiece);
                if (EditorGUI.EndChangeCheck())
                    editorData.IsDirty = true;

                GUI.backgroundColor = new Color(1f, 0.5f, 0.5f);
                if (GUILayout.Button("Remove Piece"))
                    boardView.RemovePieceFromSelectedCell();
                GUI.backgroundColor = originalColor;
            }

            EditorGUILayout.EndVertical();
        }
        private static void DrawMoveSkillToggles(PieceData piece)
        {
            if (piece.moveSkills == null)
                piece.moveSkills = new System.Collections.Generic.List<MoveSkillType>();

            EditorGUILayout.LabelField("Move Skills", EditorStyles.miniBoldLabel);
            foreach (MoveSkillType skill in System.Enum.GetValues(typeof(MoveSkillType)))
            {
                var enabled = piece.moveSkills.Contains(skill);
                var nextEnabled = EditorGUILayout.Toggle(skill.ToString(), enabled);
                if (nextEnabled == enabled)
                    continue;

                if (nextEnabled)
                    piece.moveSkills.Add(skill);
                else
                    piece.moveSkills.Remove(skill);
            }

            piece.isMovable = piece.moveSkills.Count > 0;
        }
    }
}
