using System.Collections.Generic;
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
                        else if (e.button == 1)
                        {
                            HandleCellRightClick(cellX, cellY);
                            e.Use();
                        }
                    }
                }
            }

            HandleKeyboardInput();
        }

        private void HandleKeyboardInput()
        {
            var e = Event.current;

            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Delete || e.keyCode == KeyCode.Backspace)
                {
                    if (CurrentMode == EditMode.Board && SelectedCell.HasValue)
                    {
                        DeleteSelectedCell();
                        e.Use();
                    }
                    else if (CurrentMode == EditMode.Piece && SelectedPiece != null)
                    {
                        DeleteSelectedPiece();
                        e.Use();
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
        
        private void HandleCellRightClick(int x, int y)
        {
            SelectedCell = new GridPosition(x, y);

            if (CurrentMode == EditMode.Board)
            {
                var menu = new GenericMenu();
                var cell = editorData.CurrentConfig.board.GetCell(x, y);
                var currentType = cell.cellType;
                
                menu.AddItem(new GUIContent("Set Playable"), currentType == BoardCellType.Playable, () =>
                {
                    editorData.RecordUndo();
                    cell.cellType = BoardCellType.Playable;
                });
                
                menu.AddItem(new GUIContent("Set Void"), currentType == BoardCellType.Void, () =>
                {
                    editorData.RecordUndo();
                    cell.cellType = BoardCellType.Void;
                });

                menu.AddSeparator("");
                
                menu.AddItem(new GUIContent("Delete Cell (Set to Void)"), false, () =>
                {
                    editorData.RecordUndo();
                    cell.cellType = BoardCellType.Void;
                    
                    var piecesToRemove = new List<PieceData>();
                    foreach (var piece in editorData.CurrentConfig.pieces)
                    {
                        if (piece.position.x == x && piece.position.y == y)
                            piecesToRemove.Add(piece);
                    }
                    foreach (var piece in piecesToRemove)
                        editorData.CurrentConfig.pieces.Remove(piece);
                });

                menu.ShowAsContext();
            }
            else if (CurrentMode == EditMode.Piece)
            {
                var existingPiece = FindPieceAt(x, y);
                
                if (existingPiece != null)
                {
                    var menu = new GenericMenu();
                    
                    menu.AddItem(new GUIContent($"Select Piece ({existingPiece.id})"), false, () =>
                    {
                        SelectedPiece = existingPiece;
                    });

                    menu.AddSeparator("");

                    menu.AddItem(new GUIContent("Delete Piece"), false, () =>
                    {
                        editorData.RecordUndo();
                        editorData.CurrentConfig.pieces.Remove(existingPiece);
                        if (SelectedPiece == existingPiece)
                            SelectedPiece = null;
                    });

                    menu.AddSeparator("");

                    menu.AddItem(new GUIContent("Toggle Movable"), false, () =>
                    {
                        editorData.RecordUndo();
                        existingPiece.isMovable = !existingPiece.isMovable;
                        editorData.IsDirty = true;
                    });

                    foreach (PieceType type in System.Enum.GetValues(typeof(PieceType)))
                    {
                        var typeCopy = type;
                        menu.AddItem(new GUIContent($"Change Type/{type}"), existingPiece.pieceType == type, () =>
                        {
                            editorData.RecordUndo();
                            existingPiece.pieceType = typeCopy;
                            editorData.IsDirty = true;
                        });
                    }

                    menu.ShowAsContext();
                }
                else
                {
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Add Piece Here"), false, () =>
                    {
                        AddPieceAt(x, y);
                    });
                    menu.ShowAsContext();
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
        
        private void DeleteSelectedCell()
        {
            if (!SelectedCell.HasValue)
                return;

            editorData.RecordUndo();

            var x = SelectedCell.Value.x;
            var y = SelectedCell.Value.y;
            var cell = editorData.CurrentConfig.board.GetCell(x, y);
            
            cell.cellType = BoardCellType.Void;

            var piecesToRemove = new System.Collections.Generic.List<PieceData>();
            foreach (var piece in editorData.CurrentConfig.pieces)
            {
                if (piece.position.x == x && piece.position.y == y)
                    piecesToRemove.Add(piece);
            }
            
            foreach (var piece in piecesToRemove)
            {
                editorData.CurrentConfig.pieces.Remove(piece);
                if (SelectedPiece == piece)
                    SelectedPiece = null;
            }

            Debug.Log($"Deleted cell at ({x}, {y}) and {piecesToRemove.Count} piece(s)");
        }

        private void DeleteSelectedPiece()
        {
            if (SelectedPiece == null)
                return;

            editorData.RecordUndo();
            editorData.CurrentConfig.pieces.Remove(SelectedPiece);
            
            Debug.Log($"Deleted piece: {SelectedPiece.id}");
            SelectedPiece = null;
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
