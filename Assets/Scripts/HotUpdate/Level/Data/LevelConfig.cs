using System;
using System.Collections.Generic;

namespace Game.Level.Data
{
    public enum SceneType { Forest = 0, Desert = 1, Ice = 2, Castle = 3 }
    public enum LevelDifficulty { Easy = 0, Normal = 1, Hard = 2, Expert = 3 }

    [Serializable]
    public class LevelConfig
    {
        public int schemaVersion = 1;
        public string levelId;
        public SceneType sceneType = SceneType.Forest;
        public LevelDifficulty difficulty = LevelDifficulty.Easy;
        public BoardData board = new BoardData();
        public List<PieceData> pieces = new List<PieceData>();

        public bool Validate(out string error)
        {
            if (schemaVersion != 1)
            {
                error = $"Unsupported level schema version: {schemaVersion}.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(levelId))
            {
                error = "Level id cannot be empty.";
                return false;
            }

            if (board == null)
            {
                error = "Board cannot be null.";
                return false;
            }

            if (!board.Validate(out error))
                return false;

            if (pieces == null)
            {
                error = "Pieces cannot be null.";
                return false;
            }

            var pieceIds = new HashSet<string>();
            var occupiedPositions = new HashSet<GridPosition>();
            var movablePieceCount = 0;

            foreach (var piece in pieces)
            {
                if (piece == null || string.IsNullOrWhiteSpace(piece.id))
                {
                    error = "Every piece must have an id.";
                    return false;
                }

                if (!pieceIds.Add(piece.id))
                {
                    error = $"Duplicate piece id: {piece.id}.";
                    return false;
                }

                if (!board.IsPlayable(piece.position.x, piece.position.y))
                {
                    error = $"Piece {piece.id} is on a non-playable cell at {piece.position}.";
                    return false;
                }

                if (!occupiedPositions.Add(piece.position))
                {
                    error = $"Multiple pieces occupy {piece.position}.";
                    return false;
                }

                if (piece.isMovable)
                    movablePieceCount++;
            }

            if (movablePieceCount == 0)
            {
                error = "At least one piece must be movable.";
                return false;
            }

            error = null;
            return true;
        }
    }
}
