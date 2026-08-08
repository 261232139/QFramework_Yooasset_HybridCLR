using Game.Level.Data;
using UnityEditor;
using UnityEngine;

namespace Game.Level.Editor
{
    public class BoardEditorView
    {
        private LevelEditorData editorData;
        private Vector2 viewOffset = Vector2.zero;
        private float cellDisplaySize = 40f;
        private const float MinCellSize = 20f;
        private const float MaxCellSize = 80f;

        public GridPosition? SelectedCell { get; private set; }
        public PieceData SelectedPiece { get; set; }
        public EditMode CurrentMode { get; set; } = EditMode.Board;
        public BoardCellType PaintCellType { get; set; } = BoardCellType.Playable;
        public PieceType SelectedPieceType { get; set; } = PieceType.Peg;
        public bool NewPieceMovable { get; set; } = true;

        public enum EditMode { Board, Piece }

        public BoardEditorView(LevelEditorData editorData)
        {
            this.editorData = editorData;
        }

        public void ClearSelection()
        {
            SelectedCell = null;
            SelectedPiece = null;
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
                        cellDisplaySize - 2
                    );

                    DrawCell(cellRect, x, y);
                }
            }

            DrawPiecesOnBoard(startX, startY);

            GUILayout.Label($"Mode: {CurrentMode} | Zoom: {cellDisplaySize:F0}px", EditorStyles.miniLabel);
        }

        private void DrawCell(Rect rect, int x, int y)
        {
            var cell = editorData.CurrentConfig.board.GetCell(x, y);
            var position = new GridPosition(x, y);

            var isSelected = SelectedCell.HasValue && SelectedCell.Value.Equals(position);
            var isHovered = rect.Contains(Event.current.mousePosition);

            Color cellColor;
            if (cell.IsPlayable)
                cellColor = isSelected ? new Color(0.3f, 0.7f, 1f) : (isHovered ? new Color(0.9f, 0.9f, 0.9f) : Color.white);
            else
                cellColor = isSelected ? new Color(0.5f, 0.5f, 0.7f) : (isHovered ? new Color(0.6f, 0.6f, 0.6f) : new Color(0.4f, 0.4f, 0.4f));

            EditorGUI.DrawRect(rect, cellColor);

            if (cellDisplaySize > 25f)
            {
                var labelStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = cell.IsPlayable ? Color.black : Color.gray }
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
                    cellDisplaySize - 2
                );

                var isSelected = SelectedPiece == piece;
                var pieceColor = GetPieceColor(piece.pieceType, isSelected);

                EditorGUI.DrawRect(pieceRect, pieceColor);

                var labelStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.white }
                };

                var label = piece.pieceType.ToString()[0].ToString();
                if (piece.isMovable)
                    label = $">{label}<";

                GUI.Label(pieceRect, label, labelStyle);
            }
        }

        private void HandleBoardInput(Rect rect)
        {
            var e = Event.current;

            if (rect.Contains(e.mousePosition))
            {
                if (e.type == EventType.ScrollWheel)
                {
                    cellDisplaySize -= e.delta.y * 2f;
                    cellDisplaySize = Mathf.Clamp(cellDisplaySize, MinCellSize, MaxCellSize);
                    e.Use();
                }

                if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
                {
                    var board = editorData.CurrentConfig.board;
                    var totalWidth = board.width * cellDisplaySize;
                    var totalHeight = board.height * cellDisplaySize;
                    var startX = rect.x + (rect.width - totalWidth) * 0.5f + viewOffset.x;
                    var startY = rect.y + (rect.height - totalHeight) * 0.5f + viewOffset.y;

                    var cellX = Mathf.FloorToInt((e.mousePosition.x - startX) / cellDisplaySize);
                    var cellY = Mathf.FloorToInt((e.mousePosition.y - startY) / cellDisplaySize);

                    if (board.IsInside(cellX, cellY))
                    {
                        if (e.button == 0)
                        {
                            HandleCellClick(cellX, cellY);
                            e.Use();
                        }
                    }
                }
            }
        }

        private void HandleCellClick(int x, int y)
        {
            SelectedCell = new GridPosition(x, y);

            if (CurrentMode == EditMode.Board)
            {
                editorData.RecordUndo();
                var cell = editorData.CurrentConfig.board.GetCell(x, y);
                cell.cellType = PaintCellType;
            }
            else if (CurrentMode == EditMode.Piece)
            {
                var existingPiece = FindPieceAt(x, y);
                if (existingPiece != null)
                {
                    SelectedPiece = existingPiece;
                }
                else
                {
                    AddPieceAt(x, y);
                }
            }
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

        private void AddPieceAt(int x, int y)
        {
            if (!editorData.CurrentConfig.board.IsPlayable(x, y))
            {
                EditorUtility.DisplayDialog("Invalid Position", "Cannot place piece on non-playable cell", "OK");
                return;
            }

            editorData.RecordUndo();

            var typePrefix = SelectedPieceType.ToString().ToLower();
            var id = $"{typePrefix}_{editorData.CurrentConfig.pieces.Count:D3}";

            var piece = new PieceData
            {
                id = id,
                pieceType = SelectedPieceType,
                isMovable = NewPieceMovable,
                position = new GridPosition(x, y)
            };

            editorData.CurrentConfig.pieces.Add(piece);
            SelectedPiece = piece;
        }

        public void FillBoard(BoardCellType cellType)
        {
            var board = editorData.CurrentConfig.board;
            for (var y = 0; y < board.height; y++)
            {
                for (var x = 0; x < board.width; x++)
                {
                    board.GetCell(x, y).cellType = cellType;
                }
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
