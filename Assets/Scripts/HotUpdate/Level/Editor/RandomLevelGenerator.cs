using System.Collections.Generic;
using Game.Level.Data;
using UnityEngine;

namespace Game.Level.Editor
{
    public class RandomLevelGenerator : ILevelGenerator
    {
        public string GeneratorName => "Random Generator";

        public bool Validate(LevelGenerationRequest request, out string error)
        {
            return request.Validate(out error);
        }

        public LevelConfig Generate(LevelGenerationRequest request)
        {
            if (!Validate(request, out var error))
            {
                Debug.LogError($"[RandomLevelGenerator] Invalid request: {error}");
                return null;
            }

            var config = new LevelConfig
            {
                schemaVersion = 1,
                levelId = request.levelId,
                sceneType = request.sceneType,
                difficulty = request.targetDifficulty,
                board = GenerateBoard(request),
                pieces = new List<PieceData>()
            };

            GeneratePieces(config, request);

            if (!config.Validate(out error))
            {
                Debug.LogError($"[RandomLevelGenerator] Generated invalid config: {error}");
                return null;
            }

            return config;
        }

        private BoardData GenerateBoard(LevelGenerationRequest request)
        {
            var board = new BoardData
            {
                width = request.targetWidth,
                height = request.targetHeight,
                rows = new List<BoardRowData>()
            };

            for (var y = 0; y < board.height; y++)
            {
                var row = new BoardRowData { cells = new List<BoardCellData>() };
                for (var x = 0; x < board.width; x++)
                    row.cells.Add(new BoardCellData());
                board.rows.Add(row);
            }

            return board;
        }

        private void GeneratePieces(LevelConfig config, LevelGenerationRequest request)
        {
            var availablePositions = GetBoardPositions(config.board);
            var pieceCount = Mathf.Min(
                Random.Range(request.constraints.minPieceCount, request.constraints.maxPieceCount + 1),
                availablePositions.Count
            );
            var movableCount = Mathf.Max(1, Mathf.RoundToInt(pieceCount * request.constraints.movablePieceRatio));
            var usedPositions = new HashSet<GridPosition>();

            for (var i = 0; i < pieceCount; i++)
            {
                var position = GetRandomUnusedPosition(availablePositions, usedPositions);
                if (!position.HasValue)
                    break;

                var pieceType = GetRandomPieceType(request.constraints.pieceTypeDistribution);
                config.pieces.Add(new PieceData
                {
                    id = $"{pieceType.ToString().ToLower()}_{i:D3}",
                    pieceType = pieceType,
                    isMovable = i < movableCount,
                    position = position.Value
                });
                usedPositions.Add(position.Value);
            }
        }

        private List<GridPosition> GetBoardPositions(BoardData board)
        {
            var positions = new List<GridPosition>(board.width * board.height);
            for (var y = 0; y < board.height; y++)
            {
                for (var x = 0; x < board.width; x++)
                    positions.Add(new GridPosition(x, y));
            }
            return positions;
        }

        private GridPosition? GetRandomUnusedPosition(List<GridPosition> positions, HashSet<GridPosition> used)
        {
            var available = new List<GridPosition>();
            foreach (var position in positions)
            {
                if (!used.Contains(position))
                    available.Add(position);
            }

            return available.Count > 0 ? available[Random.Range(0, available.Count)] : (GridPosition?)null;
        }

        private PieceType GetRandomPieceType(Dictionary<PieceType, int> distribution)
        {
            var totalWeight = 0;
            foreach (var weight in distribution.Values)
                totalWeight += weight;

            var random = Random.Range(0, totalWeight);
            var cumulative = 0;
            foreach (var entry in distribution)
            {
                cumulative += entry.Value;
                if (random < cumulative)
                    return entry.Key;
            }

            return PieceType.Peg;
        }
    }
}
