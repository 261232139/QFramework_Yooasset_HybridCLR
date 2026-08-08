using System.Collections.Generic;
using Game.Level.Data;
using UnityEngine;

namespace Game.Level.Editor
{
    public class LevelEditorData
    {
        public LevelConfig CurrentConfig { get; private set; }
        public bool IsDirty { get; set; }
        public string CurrentFilePath { get; set; }

        private Stack<LevelConfig> undoStack = new Stack<LevelConfig>();
        private Stack<LevelConfig> redoStack = new Stack<LevelConfig>();
        private const int MaxUndoSize = 20;

        public LevelEditorData()
        {
            CreateNew();
        }

        public void CreateNew()
        {
            CurrentConfig = new LevelConfig
            {
                schemaVersion = 1,
                levelId = $"level_{System.DateTime.Now:yyyyMMdd_HHmmss}",
                sceneType = SceneType.Forest,
                difficulty = LevelDifficulty.Normal,
                board = CreateDefaultBoard(),
                pieces = new List<PieceData>()
            };

            CurrentFilePath = null;
            IsDirty = false;
            ClearHistory();
        }

        public void LoadConfig(LevelConfig config, string filePath)
        {
            CurrentConfig = config;
            CurrentFilePath = filePath;
            IsDirty = false;
            ClearHistory();
        }

        public void RecordUndo()
        {
            if (CurrentConfig == null)
                return;

            var snapshot = CloneConfig(CurrentConfig);
            undoStack.Push(snapshot);

            if (undoStack.Count > MaxUndoSize)
            {
                var temp = new Stack<LevelConfig>();
                for (var i = 0; i < MaxUndoSize; i++)
                {
                    if (undoStack.Count > 0)
                        temp.Push(undoStack.Pop());
                }
                undoStack = temp;
            }

            redoStack.Clear();
            IsDirty = true;
        }

        public bool CanUndo() => undoStack.Count > 0;
        public bool CanRedo() => redoStack.Count > 0;

        public void Undo()
        {
            if (!CanUndo())
                return;

            redoStack.Push(CloneConfig(CurrentConfig));
            CurrentConfig = undoStack.Pop();
            IsDirty = true;
        }

        public void Redo()
        {
            if (!CanRedo())
                return;

            undoStack.Push(CloneConfig(CurrentConfig));
            CurrentConfig = redoStack.Pop();
            IsDirty = true;
        }

        public void ClearHistory()
        {
            undoStack.Clear();
            redoStack.Clear();
        }

        private BoardData CreateDefaultBoard()
        {
            var board = new BoardData
            {
                width = 5,
                height = 5,
                rows = new List<BoardRowData>()
            };

            for (var y = 0; y < board.height; y++)
            {
                var row = new BoardRowData { cells = new List<BoardCellData>() };
                for (var x = 0; x < board.width; x++)
                {
                    row.cells.Add(new BoardCellData { cellType = BoardCellType.Playable });
                }
                board.rows.Add(row);
            }

            return board;
        }

        private LevelConfig CloneConfig(LevelConfig source)
        {
            var json = JsonUtility.ToJson(source);
            return JsonUtility.FromJson<LevelConfig>(json);
        }
    }
}
