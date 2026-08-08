using System.Collections.Generic;
using Game.Level.Data;

namespace Game.Level.Editor
{
    public enum ValidationLevel
    {
        Error,
        Warning,
        Info
    }

    public class ValidationMessage
    {
        public ValidationLevel Level { get; set; }
        public string Message { get; set; }

        public ValidationMessage(ValidationLevel level, string message)
        {
            Level = level;
            Message = message;
        }
    }

    public static class ValidationSystem
    {
        public static List<ValidationMessage> Validate(LevelConfig config)
        {
            var messages = new List<ValidationMessage>();

            if (config == null)
            {
                messages.Add(new ValidationMessage(ValidationLevel.Error, "Config is null"));
                return messages;
            }

            ValidateBasicInfo(config, messages);
            ValidateBoard(config, messages);
            ValidatePieces(config, messages);
            ValidateGameplay(config, messages);

            return messages;
        }

        private static void ValidateBasicInfo(LevelConfig config, List<ValidationMessage> messages)
        {
            if (string.IsNullOrWhiteSpace(config.levelId))
                messages.Add(new ValidationMessage(ValidationLevel.Error, "Level ID cannot be empty"));

            if (config.schemaVersion != 1)
                messages.Add(new ValidationMessage(ValidationLevel.Error, $"Unsupported schema version: {config.schemaVersion}"));
        }

        private static void ValidateBoard(LevelConfig config, List<ValidationMessage> messages)
        {
            var board = config.board;
            if (board == null)
            {
                messages.Add(new ValidationMessage(ValidationLevel.Error, "Board is null"));
                return;
            }

            if (board.width < 4 || board.width > 7)
                messages.Add(new ValidationMessage(ValidationLevel.Error, $"Board width {board.width} must be between 4 and 7"));

            if (board.height < 4 || board.height > 9)
                messages.Add(new ValidationMessage(ValidationLevel.Error, $"Board height {board.height} must be between 4 and 9"));

            if (board.rows == null || board.rows.Count != board.height)
            {
                messages.Add(new ValidationMessage(ValidationLevel.Error, "Board row count does not match height"));
                return;
            }

            var playableCells = 0;
            for (var y = 0; y < board.height; y++)
            {
                var row = board.rows[y];
                if (row?.cells == null || row.cells.Count != board.width)
                {
                    messages.Add(new ValidationMessage(ValidationLevel.Error, $"Row {y} cell count does not match width"));
                    continue;
                }

                for (var x = 0; x < board.width; x++)
                {
                    if (row.cells[x] == null)
                        messages.Add(new ValidationMessage(ValidationLevel.Error, $"Cell ({x}, {y}) is null"));
                    else if (row.cells[x].IsPlayable)
                        playableCells++;
                }
            }

            if (playableCells == 0)
                messages.Add(new ValidationMessage(ValidationLevel.Error, "Board has no playable cells"));
            else if (playableCells < 10)
                messages.Add(new ValidationMessage(ValidationLevel.Warning, $"Only {playableCells} playable cells (recommended: 10+)"));

            var totalCells = board.width * board.height;
            var voidRatio = 1.0f - (playableCells / (float)totalCells);
            if (voidRatio > 0.5f)
                messages.Add(new ValidationMessage(ValidationLevel.Warning, $"High void ratio ({voidRatio:P0}), board may feel empty"));
        }

        private static void ValidatePieces(LevelConfig config, List<ValidationMessage> messages)
        {
            if (config.pieces == null)
            {
                messages.Add(new ValidationMessage(ValidationLevel.Error, "Pieces list is null"));
                return;
            }

            if (config.pieces.Count == 0)
            {
                messages.Add(new ValidationMessage(ValidationLevel.Error, "No pieces defined"));
                return;
            }

            var pieceIds = new HashSet<string>();
            var positions = new HashSet<GridPosition>();
            var movableCount = 0;

            foreach (var piece in config.pieces)
            {
                if (piece == null)
                {
                    messages.Add(new ValidationMessage(ValidationLevel.Error, "Piece is null"));
                    continue;
                }

                if (string.IsNullOrWhiteSpace(piece.id))
                    messages.Add(new ValidationMessage(ValidationLevel.Error, "Piece has empty ID"));
                else if (!pieceIds.Add(piece.id))
                    messages.Add(new ValidationMessage(ValidationLevel.Error, $"Duplicate piece ID: {piece.id}"));

                if (config.board != null && !config.board.IsPlayable(piece.position.x, piece.position.y))
                    messages.Add(new ValidationMessage(ValidationLevel.Error, $"Piece {piece.id} at {piece.position} is on non-playable cell"));

                if (!positions.Add(piece.position))
                    messages.Add(new ValidationMessage(ValidationLevel.Error, $"Multiple pieces at {piece.position}"));

                if (piece.isMovable)
                    movableCount++;
            }

            if (movableCount == 0)
                messages.Add(new ValidationMessage(ValidationLevel.Error, "No movable pieces (game cannot be played)"));
            else if (movableCount == 1)
                messages.Add(new ValidationMessage(ValidationLevel.Info, "1 movable piece (single piece control)"));
            else
                messages.Add(new ValidationMessage(ValidationLevel.Info, $"{movableCount} movable pieces (player can control multiple pieces)"));

            if (config.pieces.Count < 3)
                messages.Add(new ValidationMessage(ValidationLevel.Info, "Few pieces, level may be too simple"));
        }

        private static void ValidateGameplay(LevelConfig config, List<ValidationMessage> messages)
        {
            var pegCount = 0;
            var gemCount = 0;
            var stoneCount = 0;

            foreach (var piece in config.pieces)
            {
                if (piece == null) continue;

                switch (piece.pieceType)
                {
                    case PieceType.Peg: pegCount++; break;
                    case PieceType.Gem: gemCount++; break;
                    case PieceType.Stone: stoneCount++; break;
                }
            }

            if (gemCount == 0)
                messages.Add(new ValidationMessage(ValidationLevel.Warning, "No gems defined (may affect gameplay goals)"));

            if (stoneCount > pegCount + gemCount)
                messages.Add(new ValidationMessage(ValidationLevel.Info, "Many stones relative to other pieces"));
        }

        public static bool HasErrors(List<ValidationMessage> messages)
        {
            foreach (var msg in messages)
            {
                if (msg.Level == ValidationLevel.Error)
                    return true;
            }
            return false;
        }
    }
}
