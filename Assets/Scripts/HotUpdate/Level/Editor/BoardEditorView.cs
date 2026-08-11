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

        public GridPosition? SelectedCell { get; private set; }
        public PieceData SelectedPiece { get; private set; }
        public PieceType SelectedPieceType { get; set; } = PieceType.Peg;
        public bool NewPieceMovable { get; set; } = true;
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

            Color color;
            if (!hasCell)
                color = isHovered ? new Color(0.35f, 0.35f, 0.35f) : new Color(0.25f, 0.25f, 0.25f);
            else if (isSelected)
                color = new Color(0.3f, 0.7f, 1f);
            else
                color = isHovered ? new Color(0.9f, 0.9f, 0.9f) : Color.white;

            EditorGUI.DrawRect(rect, color);

            if (hasCell && cellDisplaySize > 25f)
            {
                var labelStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.black }
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
                GUI.Label(pieceRect, piece.isMovable ? $">{label}<" : label, labelStyle);
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
                            HandleLeftClick(cellX, cellY);
                        else if (e.button == 1)
                            HandleRightClick(cellX, cellY);
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

        private void HandleLeftClick(int x, int y)
        {
            var board = editorData.CurrentConfig.board;
            if (!board.HasCell(x, y))
            {
                editorData.RecordUndo();
                board.rows[y].cells[x] = new BoardCellData();
            }

            SelectedCell = new GridPosition(x, y);
            SelectedPiece = FindPieceAt(x, y);
        }

        private void HandleRightClick(int x, int y)
        {
            var board = editorData.CurrentConfig.board;
            if (!board.HasCell(x, y))
                return;

            editorData.RecordUndo();
            var removedPiece = FindPieceAt(x, y);
            if (removedPiece != null)
                editorData.CurrentConfig.pieces.Remove(removedPiece);
            board.rows[y].cells[x] = null;

            if (SelectedCell.HasValue && SelectedCell.Value.Equals(new GridPosition(x, y)))
                ClearSelection();
            else if (SelectedPiece == removedPiece)
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
                isMovable = NewPieceMovable,
                position = position
            };

            editorData.CurrentConfig.pieces.Add(piece);
            SelectedPiece = piece;
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
            return type switch
            {
                PieceType.Peg => new Color(1f, 0.5f, 0f, alpha),
                PieceType.Gem => new Color(0f, 0.8f, 1f, alpha),
                PieceType.Stone => new Color(0.6f, 0.6f, 0.6f, alpha),
                _ => Color.magenta
            };
        }
    }
}
