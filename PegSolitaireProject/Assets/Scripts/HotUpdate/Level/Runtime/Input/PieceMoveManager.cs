using System.Collections.Generic;
using Game.Level.Data;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Level.Runtime
{
    public class PieceMoveEventArgs
    {
        public IPiece Piece { get; }
        public GridPosition From { get; }
        public GridPosition To { get; }
        public GridPosition JumpedPosition { get; }
        public bool IsValid { get; }
        public string ErrorMessage { get; }

        public PieceMoveEventArgs(IPiece piece, GridPosition from, GridPosition to, GridPosition jumped, bool isValid, string error = null)
        {
            Piece = piece; From = from; To = to; JumpedPosition = jumped; IsValid = isValid; ErrorMessage = error;
        }
    }

    /// <summary>正式输入入口：点击棋子选中，再点击空格尝试移动。</summary>
    public class PieceMoveManager : MonoBehaviour
    {
        [SerializeField] private Board board;
        [SerializeField] private bool showDebugInfo = true;

        private LevelController mController;
        private IPiece mSelectedPiece;
        private bool mInputEnabled;
        private bool mIsExecutingMove;

        public event System.Action<IPiece> OnPieceSelected;
        public event System.Action<IPiece> OnPieceDeselected;
        public event System.Action<PieceMoveEventArgs> OnPieceMoved;
        public event System.Action<PieceMoveEventArgs> OnMoveAttempted;
        public event System.Action<MoveExecutionResult> OnMoveExecuted;

        public void Initialize(LevelController controller, Board boardRef = null)
        {
            mController = controller;
            mInputEnabled = controller != null;
            if (boardRef != null) board = boardRef;
        }

        public void EnableInput(bool enabled)
        {
            mInputEnabled = enabled && mController != null;
            if (!mInputEnabled) DeselectPiece();
        }

        private void Update()
        {
            if (mInputEnabled && mController != null && Input.GetMouseButtonDown(0))
                HandleClick(Input.mousePosition);
        }

        private void HandleClick(Vector2 screenPosition)
        {
            var gridPosition = ScreenToGridPosition(screenPosition);
            if (!gridPosition.HasValue) { DeselectPiece(); return; }

            var clickedPiece = mController.BoardState.GetPieceAt(gridPosition.Value);
            if (mSelectedPiece == null)
            {
                if (clickedPiece != null && clickedPiece.IsMovable) SelectPiece(clickedPiece);
                return;
            }

            if (clickedPiece == mSelectedPiece) { DeselectPiece(); return; }
            if (clickedPiece != null && clickedPiece.IsMovable) { SelectPiece(clickedPiece); return; }

            if (clickedPiece == null)
                TryMovePiece(mSelectedPiece, mSelectedPiece.Position, gridPosition.Value);
            DeselectPiece();
        }

        private void SelectPiece(IPiece piece)
        {
            DeselectPiece();
            mSelectedPiece = piece;
            mSelectedPiece.IsSelected = true;
            OnPieceSelected?.Invoke(piece);
        }

        private void DeselectPiece()
        {
            if (mSelectedPiece == null) return;
            var piece = mSelectedPiece;
            piece.IsSelected = false;
            mSelectedPiece = null;
            OnPieceDeselected?.Invoke(piece);
        }

        private void TryMovePiece(IPiece piece, GridPosition from, GridPosition to)
        {
            if (mIsExecutingMove) return;
            mIsExecutingMove = true;
            try
            {
                var result = mController.ExecuteMove(piece, to);
                var jumpedPosition = result.Success && result.JumpedPiece != null ? result.JumpedPiece.Position : GridPosition.Invalid;
                var eventArgs = new PieceMoveEventArgs(piece, from, to, jumpedPosition, result.Success, result.ErrorMessage);
                OnMoveAttempted?.Invoke(eventArgs);
                if (!result.Success)
                {
                    if (showDebugInfo) Debug.LogWarning($"[PieceMoveManager] Invalid move: {result.ErrorMessage}");
                    return;
                }
                OnMoveExecuted?.Invoke(result);
                OnPieceMoved?.Invoke(eventArgs);
            }
            finally { mIsExecutingMove = false; }
        }

        private GridPosition? ScreenToGridPosition(Vector2 screenPosition)
        {
            if (board == null || EventSystem.current == null) return null;
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = screenPosition }, results);
            foreach (var result in results)
            {
                var mapGrid = result.gameObject.GetComponentInParent<MapGrid>();
                if (mapGrid != null) return mapGrid.Position;
            }
            return null;
        }

        public IPiece GetSelectedPiece() => mSelectedPiece;
        public bool HasSelection() => mSelectedPiece != null;
    }
}
