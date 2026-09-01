using System;
using System.Collections.Generic;
using Game.Level.Data;

namespace Game.Level.Editor
{
    [Serializable]
    public class GenerationConstraints
    {
        public int minPieceCount = 3;
        public int maxPieceCount = 20;
        public float movablePieceRatio = 0.3f;
        public Dictionary<PieceType, int> pieceTypeDistribution = new Dictionary<PieceType, int>
        {
            { PieceType.Normal, 1 }
        };
    }

    [Serializable]
    public class LevelGenerationRequest
    {
        public string levelId = "generated_level";
        public int minWidth = 4;
        public int maxWidth = 7;
        public int minHeight = 4;
        public int maxHeight = 9;
        public int targetWidth = 5;
        public int targetHeight = 6;
        public LevelDifficulty targetDifficulty = LevelDifficulty.Normal;
        public SceneType sceneType = SceneType.Forest;
        public GenerationConstraints constraints = new GenerationConstraints();

        public bool Validate(out string error)
        {
            if (targetWidth < minWidth || targetWidth > maxWidth)
            {
                error = $"Target width {targetWidth} must be between {minWidth} and {maxWidth}.";
                return false;
            }

            if (targetHeight < minHeight || targetHeight > maxHeight)
            {
                error = $"Target height {targetHeight} must be between {minHeight} and {maxHeight}.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(levelId))
            {
                error = "Level ID cannot be empty.";
                return false;
            }

            error = null;
            return true;
        }
    }
}
