using System;
using System.Collections.Generic;

namespace Game.Level.Data
{
    [Serializable]
    public class BoardCellData
    {
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

        public bool HasCell(int x, int y) => GetCell(x, y) != null;

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

            var cellCount = 0;
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
                    if (row.cells[x] != null)
                        cellCount++;
                }
            }

            if (cellCount == 0)
            {
                error = "Board must contain at least one cell.";
                return false;
            }

            error = null;
            return true;
        }
    }
}
