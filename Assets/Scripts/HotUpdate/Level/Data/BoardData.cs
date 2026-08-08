using System;
using System.Collections.Generic;

namespace Game.Level.Data
{
    public enum BoardCellType
    {
        Void = 0,
        Playable = 1,
    }

    [Serializable]
    public class BoardCellData
    {
        public BoardCellType cellType = BoardCellType.Void;

        public bool IsPlayable => cellType == BoardCellType.Playable;
    }

    [Serializable]
    public class BoardRowData
    {
        public List<BoardCellData> cells = new List<BoardCellData>();
    }

    [Serializable]
    public class BoardData
    {
        public int width;
        public int height;
        public List<BoardRowData> rows = new List<BoardRowData>();

        public bool IsInside(int x, int y) => x >= 0 && x < width && y >= 0 && y < height;

        public BoardCellData GetCell(int x, int y)
        {
            if (!IsInside(x, y))
                return null;

            return rows[y].cells[x];
        }

        public bool IsPlayable(int x, int y) => GetCell(x, y)?.IsPlayable == true;

        public bool Validate(out string error)
        {
            if (width <= 0 || height <= 0)
            {
                error = "Board width and height must be greater than zero.";
                return false;
            }

            if (rows == null || rows.Count != height)
            {
                error = "Board row count must match height.";
                return false;
            }

            var playableCellCount = 0;
            for (var y = 0; y < height; y++)
            {
                var row = rows[y];
                if (row?.cells == null || row.cells.Count != width)
                {
                    error = $"Board row {y} cell count must match width.";
                    return false;
                }

                for (var x = 0; x < width; x++)
                {
                    if (row.cells[x] == null)
                    {
                        error = $"Board cell ({x}, {y}) cannot be null.";
                        return false;
                    }

                    if (row.cells[x].IsPlayable)
                        playableCellCount++;
                }
            }

            if (playableCellCount == 0)
            {
                error = "Board must contain at least one playable cell.";
                return false;
            }

            error = null;
            return true;
        }
    }
}
