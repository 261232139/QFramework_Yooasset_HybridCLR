using System.Collections.Generic;
using Game.Level.Data;
using UnityEngine;

namespace Game.Level.Runtime
{
    /// <summary>缁存姢鍏冲崱鍐呮瀛愮殑杩愯鏃朵綅缃笌瑙ｆ晳鐩爣璁℃暟銆?/summary>
    public class BoardStateManager : IBoardState
    {
        private readonly LevelConfig mConfig;
        private readonly Dictionary<GridPosition, IPiece> mPiecesByPosition;
        private readonly List<IPiece> mAllPieces;
        private readonly List<IPiece> mInitialPieces;

        public int Width => mConfig.board.width;
        public int Height => mConfig.board.height;
        public IReadOnlyList<IPiece> AllPieces => mAllPieces;
        public int InitialRescueTargetCount { get; private set; }
        public int RemainingRescueTargetCount { get; private set; }
        public bool HasRescueTargets => InitialRescueTargetCount > 0;

        public BoardStateManager(LevelConfig config)
        {
            mConfig = config;
            mPiecesByPosition = new Dictionary<GridPosition, IPiece>();
            mAllPieces = new List<IPiece>();
            mInitialPieces = new List<IPiece>();
            InitializePieces();
        }

        private void InitializePieces()
        {
            foreach (var pieceData in mConfig.pieces)
            {
                var piece = PieceFactory.CreatePiece(pieceData);
                mAllPieces.Add(piece);
                mInitialPieces.Add(piece);
                mPiecesByPosition[piece.Position] = piece;
                if (piece.IsRescueTarget)
                {
                    InitialRescueTargetCount++;
                    RemainingRescueTargetCount++;
                }
            }

            Debug.Log($"[BoardState] Initialized {mAllPieces.Count} pieces");
        }

        public bool IsInBounds(GridPosition position) =>
            position.x >= 0 && position.x < Width && position.y >= 0 && position.y < Height;

        public bool HasCell(GridPosition position) =>
            IsInBounds(position) && mConfig.board.HasCell(position.x, position.y);

        public IPiece GetPieceAt(GridPosition position)
        {
            mPiecesByPosition.TryGetValue(position, out var piece);
            return piece;
        }

        public bool HasPieceAt(GridPosition position) => mPiecesByPosition.ContainsKey(position);

        public void MovePiece(IPiece piece, GridPosition newPosition)
        {
            mPiecesByPosition.Remove(piece.Position);
            piece.MoveTo(newPosition);
            mPiecesByPosition[newPosition] = piece;
        }

        public void RemovePiece(GridPosition position)
        {
            if (!mPiecesByPosition.TryGetValue(position, out var piece))
                return;

            mPiecesByPosition.Remove(position);
            mAllPieces.Remove(piece);
            if (piece.IsRescueTarget)
                RemainingRescueTargetCount--;

            Debug.Log($"[BoardState] Removed piece at {position}");
        }

        public void ResetAllPieces()
        {
            mPiecesByPosition.Clear();
            mAllPieces.Clear();
            foreach (var piece in mInitialPieces)
            {
                piece.Reset();
                mAllPieces.Add(piece);
                mPiecesByPosition[piece.Position] = piece;
            }

            RemainingRescueTargetCount = 0;
            foreach (var piece in mAllPieces)
            {
                if (piece.IsRescueTarget)
                    RemainingRescueTargetCount++;
            }

            Debug.Log("[BoardState] All pieces reset to initial positions");
        }

        public bool HasMovablePieces()
        {
            foreach (var piece in mAllPieces)
            {
                if (piece.IsMovable && CanPieceMove(piece))
                    return true;
            }

            return false;
        }

        private bool CanPieceMove(IPiece piece)
        {
            foreach (var _ in piece.GetValidMoves(this))
                return true;

            return false;
        }
    }
}



