/****************************************************************************
 * Copyright (c) 2024 Game Project UNDER MIT License
 *
 * 棋盘状态管理器
 *
 * 管理棋盘上的所有棋子状态
 ****************************************************************************/

using System.Collections.Generic;
using Game.Level.Data;
using UnityEngine;

namespace Game.Level.Runtime
{
    /// <summary>
    /// 棋盘状态管理器
    /// 实现 IBoardState 接口，供棋子验证移动时查询棋盘信息
    /// </summary>
    public class BoardStateManager : IBoardState
    {
        private readonly LevelConfig mConfig;
        private readonly Dictionary<GridPosition, IPiece> mPiecesByPosition;
        private readonly List<IPiece> mAllPieces;

        public int Width => mConfig.board.width;
        public int Height => mConfig.board.height;

        public IReadOnlyList<IPiece> AllPieces => mAllPieces;

        public BoardStateManager(LevelConfig config)
        {
            mConfig = config;
            mPiecesByPosition = new Dictionary<GridPosition, IPiece>();
            mAllPieces = new List<IPiece>();
            
            InitializePieces();
        }

        private void InitializePieces()
        {
            foreach (var pieceData in mConfig.pieces)
            {
                var piece = PieceFactory.CreatePiece(pieceData);
                mAllPieces.Add(piece);
                mPiecesByPosition[piece.Position] = piece;
            }

            Debug.Log($"[BoardState] Initialized {mAllPieces.Count} pieces");
        }

        #region IBoardState 实现

        public bool IsInBounds(GridPosition position)
        {
            return position.x >= 0 && position.x < Width &&
                   position.y >= 0 && position.y < Height;
        }

        public bool IsPlayable(GridPosition position)
        {
            if (!IsInBounds(position))
                return false;

            return mConfig.board.IsPlayable(position.x, position.y);
        }

        public IPiece GetPieceAt(GridPosition position)
        {
            mPiecesByPosition.TryGetValue(position, out var piece);
            return piece;
        }

        public bool HasPieceAt(GridPosition position)
        {
            return mPiecesByPosition.ContainsKey(position);
        }

        #endregion

        #region 棋子管理

        /// <summary>
        /// 移动棋子（更新内部状态）
        /// </summary>
        public void MovePiece(IPiece piece, GridPosition newPosition)
        {
            // 从旧位置移除
            mPiecesByPosition.Remove(piece.Position);
            
            // 更新棋子位置
            piece.MoveTo(newPosition);
            
            // 添加到新位置
            mPiecesByPosition[newPosition] = piece;
        }

        /// <summary>
        /// 移除棋子（被跨越时）
        /// </summary>
        public void RemovePiece(GridPosition position)
        {
            if (mPiecesByPosition.TryGetValue(position, out var piece))
            {
                mPiecesByPosition.Remove(position);
                mAllPieces.Remove(piece);
                Debug.Log($"[BoardState] Removed piece at {position}");
            }
        }

        /// <summary>
        /// 重置所有棋子到初始位置
        /// </summary>
        public void ResetAllPieces()
        {
            mPiecesByPosition.Clear();
            
            foreach (var piece in mAllPieces)
            {
                piece.Reset();
                mPiecesByPosition[piece.Position] = piece;
            }
            
            Debug.Log("[BoardState] All pieces reset to initial positions");
        }

        /// <summary>
        /// 检查是否还有可移动的棋子
        /// </summary>
        public bool HasMovablePieces()
        {
            foreach (var piece in mAllPieces)
            {
                if (piece.IsMovable && CanPieceMove(piece))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 检查指定棋子是否有合法移动
        /// </summary>
        private bool CanPieceMove(IPiece piece)
        {
            var pos = piece.Position;
            
            // 检查四个方向
            var directions = new[]
            {
                new GridPosition(pos.x, pos.y - 2), // Up
                new GridPosition(pos.x, pos.y + 2), // Down
                new GridPosition(pos.x - 2, pos.y), // Left
                new GridPosition(pos.x + 2, pos.y)  // Right
            };

            foreach (var target in directions)
            {
                var result = piece.ValidateMove(pos, target, this);
                if (result.IsValid)
                    return true;
            }

            return false;
        }

        #endregion
    }
}
