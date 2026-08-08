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

            var targetPlayableCells = Random.Range(
                Mathf.Max(request.constraints.minPlayableCells, request.targetWidth * request.targetHeight / 2),
                Mathf.Min(request.constraints.maxPlayableCells, request.targetWidth * request.targetHeight)
            );

            for (var y = 0; y < board.height; y++)
            {
                var row = new BoardRowData { cells = new List<BoardCellData>() };
                for (var x = 0; x < board.width; x++)
                {
                    var cell = new BoardCellData
                    {
                        cellType = Random.value > 0.3f ? BoardCellType.Playable : BoardCellType.Void
                    };
                    row.cells.Add(cell);
                }
                board.rows.Add(row);
            }

            var playableCount = CountPlayableCells(board);
            if (playableCount < request.constraints.minPlayableCells)
            {
                FillRandomCells(board, request.constraints.minPlayableCells - playableCount);
            }

            return board;
        }

        private void GeneratePieces(LevelConfig config, LevelGenerationRequest request)
        {
            var playablePositions = GetPlayablePositions(config.board);
            if (playablePositions.Count == 0)
                return;

            var pieceCount = Mathf.Min(
                Random.Range(request.constraints.minPieceCount, request.constraints.maxPieceCount + 1),
                playablePositions.Count
            );

            var movableCount = Mathf.Max(1, Mathf.RoundToInt(pieceCount * request.constraints.movablePieceRatio));
            var usedPositions = new HashSet<GridPosition>();

            for (var i = 0; i < pieceCount; i++)
            {
                var position = GetRandomUnusedPosition(playablePositions, usedPositions);
                if (!position.HasValue)
                    break;

                var pieceType = GetRandomPieceType(request.constraints.pieceTypeDistribution);
                var piece = new PieceData
                {
                    id = $"{pieceType.ToString().ToLower()}_{i:D3}",
                    pieceType = pieceType,
                    isMovable = i < movableCount,
                    position = position.Value
                };

                config.pieces.Add(piece);
                usedPositions.Add(position.Value);
            }
        }

        private int CountPlayableCells(BoardData board)
        {
            var count = 0;
            for (var y = 0; y < board.height; y++)
            {
                for (var x = 0; x < board.width; x++)
                {
                    if (board.IsPlayable(x, y))
                        count++;
                }
            }
            return count;
        }

        private void FillRandomCells(BoardData board, int count)
        {
            var filled = 0;
            var attempts = 0;
            var maxAttempts = board.width * board.height * 2;

            while (filled < count && attempts < maxAttempts)
            {
                var x = Random.Range(0, board.width);
                var y = Random.Range(0, board.height);
                var cell = board.GetCell(x, y);

                if (cell != null && !cell.IsPlayable)
                {
                    cell.cellType = BoardCellType.Playable;
                    filled++;
                }

                attempts++;
            }
        }

        private List<GridPosition> GetPlayablePositions(BoardData board)
        {
            var positions = new List<GridPosition>();
            for (var y = 0; y < board.height; y++)
            {
                for (var x = 0; x < board.width; x++)
                {
                    if (board.IsPlayable(x, y))
                        positions.Add(new GridPosition(x, y));
                }
            }
            return positions;
        }

        private GridPosition? GetRandomUnusedPosition(List<GridPosition> positions, HashSet<GridPosition> used)
        {
            var available = new List<GridPosition>();
            foreach (var pos in positions)
            {
                if (!used.Contains(pos))
                    available.Add(pos);
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

            foreach (var kvp in distribution)
            {
                cumulative += kvp.Value;
                if (random < cumulative)
                    return kvp.Key;
            }

            return PieceType.Peg;
        }
    }
}
