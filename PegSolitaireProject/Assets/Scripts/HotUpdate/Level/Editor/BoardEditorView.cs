using System.Collections.Generic;
using Game.Level.Data;
using UnityEditor;
using UnityEngine;

namespace Game.Level.Editor
{
    public class BoardEditorView
    {
        private readonly LevelEditorData editorData;
        private Vector2 viewOffset = Vector2.zero;
        private float cellDisplaySize = 40f;
        private const float MinCellSize = 20f;
        private const float MaxCellSize = 80f;
        private static readonly Color EditorEmptyCellColor = new Color(0.31f, 0.31f, 0.31f);
        private static readonly Color EditorEmptyCellHoverColor = new Color(0.40f, 0.40f, 0.40f);
        private static readonly Color BoardCellColor = new Color(0.30f, 0.65f, 0.45f);
        private static readonly Color BoardCellHoverColor = new Color(0.42f, 0.78f, 0.57f);
        private static readonly Color SelectedEmptyCellColor = new Color(0.95f, 0.72f, 0.28f);
        private static readonly Color SelectedBoardCellColor = new Color(0.25f, 0.62f, 1f);

        public GridPosition? SelectedCell { get; private set; }
        public PieceData SelectedPiece { get; private set; }
        public PieceType SelectedPieceType { get; set; } = PieceType.Normal;
        public bool NewPieceMovable { get; set; } = true;
        public bool NewPieceCanBeJumped { get; set; } = true;
        public bool NewPieceIsRescueTarget { get; set; }
        public bool HasSelectedCell => SelectedCell.HasValue;
        public bool HasSelectedBoardCell => SelectedCell.HasValue &&
                                            editorData.CurrentConfig.board.HasCell(
                                                SelectedCell.Value.x,
                                                SelectedCell.Value.y);

        public BoardEditorView(LevelEditorData editorData)
        {
            this.editorData = editorData;
        }

        public void ClearSelection()
        {
            SelectedCell = null;
            SelectedPiece = null;
        }

        public void SelectPiece(PieceData piece)
        {
            SelectedPiece = piece;
            SelectedCell = piece == null ? (GridPosition?)null : piece.position;
        }

        public void Draw()
        {
            EditorGUILayout.BeginVertical();
            DrawBoardView();
            EditorGUILayout.EndVertical();
        }

        private void DrawBoardView()
        {
            var rect = GUILayoutUtility.GetRect(400, 400, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f));

            HandleBoardInput(rect);

            var board = editorData.CurrentConfig.board;
            var totalWidth = board.width * cellDisplaySize;
            var totalHeight = board.height * cellDisplaySize;
            var startX = rect.x + (rect.width - totalWidth) * 0.5f + viewOffset.x;
            var startY = rect.y + (rect.height - totalHeight) * 0.5f + viewOffset.y;

            for (var y = 0; y < board.height; y++)
            {
                for (var x = 0; x < board.width; x++)
                {
                    var cellRect = new Rect(
                        startX + x * cellDisplaySize,
                        startY + y * cellDisplaySize,
                        cellDisplaySize - 2,
                        cellDisplaySize - 2);
                    DrawCell(cellRect, x, y);
                }
            }

            DrawPiecesOnBoard(startX, startY);
            GUILayout.Label($"Zoom: {cellDisplaySize:F0}px", EditorStyles.miniLabel);
        }

        private void DrawCell(Rect rect, int x, int y)
        {
            var board = editorData.CurrentConfig.board;
            var position = new GridPosition(x, y);
            var hasCell = board.HasCell(x, y);
            var isSelected = SelectedCell.HasValue && SelectedCell.Value.Equals(position);
            var isHovered = rect.Contains(Event.current.mousePosition);

            var color = hasCell
                ? (isSelected ? SelectedBoardCellColor : isHovered ? BoardCellHoverColor : BoardCellColor)
                : (isSelected ? SelectedEmptyCellColor : isHovered ? EditorEmptyCellHoverColor : EditorEmptyCellColor);

            EditorGUI.DrawRect(rect, color);

            if (cellDisplaySize > 25f)
            {
                var labelStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = hasCell ? Color.black : Color.white }
                };
                GUI.Label(rect, $"{x},{y}", labelStyle);
            }
        }

        private void DrawPiecesOnBoard(float startX, float startY)
        {
            foreach (var piece in editorData.CurrentConfig.pieces)
            {
                var pieceRect = new Rect(
                    startX + piece.position.x * cellDisplaySize,
                    startY + piece.position.y * cellDisplaySize,
                    cellDisplaySize - 2,
                    cellDisplaySize - 2);

                EditorGUI.DrawRect(pieceRect, GetPieceColor(piece.pieceType, SelectedPiece == piece));

                var labelStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.white }
                };
                var label = piece.pieceType.ToString()[0].ToString();
                GUI.Label(pieceRect, piece.HasMoveSkills ? $">{label}<" : label, labelStyle);
            }
        }

        private void HandleBoardInput(Rect rect)
        {
            var e = Event.current;

            if (rect.Contains(e.mousePosition))
            {
                if (e.type == EventType.ScrollWheel)
                {
                    cellDisplaySize = Mathf.Clamp(cellDisplaySize - e.delta.y * 2f, MinCellSize, MaxCellSize);
                    e.Use();
                }

                if (e.type == EventType.MouseDown)
                {
                    var board = editorData.CurrentConfig.board;
                    var startX = rect.x + (rect.width - board.width * cellDisplaySize) * 0.5f + viewOffset.x;
                    var startY = rect.y + (rect.height - board.height * cellDisplaySize) * 0.5f + viewOffset.y;
                    var cellX = Mathf.FloorToInt((e.mousePosition.x - startX) / cellDisplaySize);
                    var cellY = Mathf.FloorToInt((e.mousePosition.y - startY) / cellDisplaySize);

                    if (board.IsInside(cellX, cellY))
                    {
                        if (e.button == 0)
                            SelectCell(cellX, cellY);
                        e.Use();
                    }
                }
            }

            if (e.type == EventType.KeyDown &&
                (e.keyCode == KeyCode.Delete || e.keyCode == KeyCode.Backspace) &&
                SelectedPiece != null)
            {
                RemovePieceFromSelectedCell();
                e.Use();
            }
        }

        private void SelectCell(int x, int y)
        {
            SelectedCell = new GridPosition(x, y);
            SelectedPiece = FindPieceAt(x, y);
        }

        public void AddCellAtSelectedPosition()
        {
            if (!SelectedCell.HasValue || HasSelectedBoardCell)
                return;

            var board = editorData.CurrentConfig.board;
            var position = SelectedCell.Value;
            if (!board.IsInside(position.x, position.y))
                return;

            editorData.RecordUndo();
            var cell = board.rows[position.y].cells[position.x];
            if (cell == null)
                board.rows[position.y].cells[position.x] = new BoardCellData { isActive = true };
            else
                cell.isActive = true;
            SelectedPiece = null;
        }

        public void RemoveSelectedCell()
        {
            if (!HasSelectedBoardCell)
                return;

            var position = SelectedCell.Value;
            editorData.RecordUndo();

            var removedPiece = FindPieceAt(position.x, position.y);
            if (removedPiece != null)
                editorData.CurrentConfig.pieces.Remove(removedPiece);
            var cell = editorData.CurrentConfig.board.rows[position.y].cells[position.x];
            if (cell == null)
                editorData.CurrentConfig.board.rows[position.y].cells[position.x] = new BoardCellData { isActive = false };
            else
                cell.isActive = false;

            SelectedPiece = null;
        }

        public void AddPieceToSelectedCell()
        {
            if (!HasSelectedBoardCell || SelectedPiece != null)
                return;

            editorData.RecordUndo();
            var position = SelectedCell.Value;
            var piece = new PieceData
            {
                id = CreateUniquePieceId(SelectedPieceType),
                pieceType = SelectedPieceType,
                moveSkills = GetMoveSkillsForType(SelectedPieceType),
                canBeJumped = NewPieceCanBeJumped,
                isRescueTarget = NewPieceIsRescueTarget,
                position = position
            };

            editorData.CurrentConfig.pieces.Add(piece);
            SelectedPiece = piece;
        }

        private static List<MoveSkillType> GetMoveSkillsForType(PieceType pieceType)
        {
            if (pieceType != PieceType.Normal)
                return new List<MoveSkillType>();

            return new List<MoveSkillType>
            {
                MoveSkillType.JumpUp,
                MoveSkillType.JumpDown,
                MoveSkillType.JumpLeft,
                MoveSkillType.JumpRight
            };
        }

        public void RemovePieceFromSelectedCell()
        {
            if (!HasSelectedBoardCell || SelectedPiece == null)
                return;

            editorData.RecordUndo();
            editorData.CurrentConfig.pieces.Remove(SelectedPiece);
            SelectedPiece = null;
        }

        private PieceData FindPieceAt(int x, int y)
        {
            var position = new GridPosition(x, y);
            foreach (var piece in editorData.CurrentConfig.pieces)
            {
                if (piece.position.Equals(position))
                    return piece;
            }
            return null;
        }

        private string CreateUniquePieceId(PieceType pieceType)
        {
            var prefix = pieceType.ToString().ToLower();
            var index = 0;
            while (true)
            {
                var candidate = $"{prefix}_{index:D3}";
                var exists = editorData.CurrentConfig.pieces.Exists(piece => piece.id == candidate);
                if (!exists)
                    return candidate;
                index++;
            }
        }

        private Color GetPieceColor(PieceType type, bool isSelected)
        {
            var alpha = isSelected ? 1f : 0.8f;
            return new Color(1f, 0.5f, 0f, alpha);
        }
    }
}
