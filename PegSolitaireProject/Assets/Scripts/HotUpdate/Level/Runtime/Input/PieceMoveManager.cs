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

        public PieceMoveEventArgs(IPiece piece, GridPosition from, GridPosition to,
            GridPosition jumped, bool isValid, string error = null)
        {
            Piece = piece;
            From = from;
            To = to;
            JumpedPosition = jumped;
            IsValid = isValid;
            ErrorMessage = error;
        }
    }

    /// <summary>正式输入入口，只负责选择、输入和转发移动结果。</summary>
    public class PieceMoveManager : MonoBehaviour
    {
        [SerializeField] private Board board;
        [SerializeField] private float dragThreshold = 10f;
        [SerializeField] private bool showDebugInfo = true;

        private LevelController mController;
        private IPiece mSelectedPiece;
        private Vector2 mDragStartPosition;
        private bool mIsDragging;
        private GridPosition mDragStartGridPosition;
        private bool mInputEnabled;

        public event System.Action<IPiece> OnPieceSelected;
        public event System.Action<IPiece> OnPieceDeselected;
        public event System.Action<PieceMoveEventArgs> OnPieceMoved;
        public event System.Action<PieceMoveEventArgs> OnMoveAttempted;
        public event System.Action<MoveExecutionResult> OnMoveExecuted;

        public void Initialize(LevelController controller, Board boardRef = null)
        {
            mController = controller;
            mInputEnabled = controller != null;

            if (boardRef != null)
                board = boardRef;

            Debug.Log("[PieceMoveManager] Initialized");
        }

        public void EnableInput(bool enabled)
        {
            mInputEnabled = enabled && mController != null;
            if (!mInputEnabled)
                DeselectPiece();
        }

        private void Update()
        {
            if (mInputEnabled && mController != null)
                HandleInput();
        }

        private void HandleInput()
        {
            if (Input.GetMouseButtonDown(0))
                OnPointerDown(Input.mousePosition);

            if (Input.GetMouseButton(0) && mSelectedPiece != null)
                OnPointerDrag(Input.mousePosition);

            if (Input.GetMouseButtonUp(0) && mSelectedPiece != null)
                OnPointerUp(Input.mousePosition);
        }

        private void OnPointerDown(Vector2 screenPosition)
        {
            var gridPosition = ScreenToGridPosition(screenPosition);
            if (!gridPosition.HasValue)
                return;

            var piece = mController.BoardState.GetPieceAt(gridPosition.Value);
            if (piece != null && piece.IsMovable)
                SelectPiece(piece, screenPosition, gridPosition.Value);
        }

        private void OnPointerDrag(Vector2 screenPosition)
        {
            if (mSelectedPiece == null || mIsDragging)
                return;

            if (Vector2.Distance(screenPosition, mDragStartPosition) > dragThreshold)
            {
                mIsDragging = true;
                if (showDebugInfo)
                    Debug.Log($"[PieceMoveManager] Started dragging piece {mSelectedPiece.Id}");
            }
        }

        private void OnPointerUp(Vector2 screenPosition)
        {
            if (mSelectedPiece == null)
                return;

            var targetGridPosition = ScreenToGridPosition(screenPosition);
            if (targetGridPosition.HasValue)
                TryMovePiece(mSelectedPiece, mDragStartGridPosition, targetGridPosition.Value);

            DeselectPiece();
        }

        private void SelectPiece(IPiece piece, Vector2 screenPosition, GridPosition gridPosition)
        {
            DeselectPiece();

            mSelectedPiece = piece;
            mSelectedPiece.IsSelected = true;
            mDragStartPosition = screenPosition;
            mDragStartGridPosition = gridPosition;
            mIsDragging = false;
            OnPieceSelected?.Invoke(piece);
        }

        private void DeselectPiece()
        {
            if (mSelectedPiece == null)
                return;

            var piece = mSelectedPiece;
            piece.IsSelected = false;
            mSelectedPiece = null;
            mIsDragging = false;
            OnPieceDeselected?.Invoke(piece);
        }

        private void TryMovePiece(IPiece piece, GridPosition from, GridPosition to)
        {
            var result = mController.ExecuteMove(piece, to);
            var jumpedPosition = result.Success && result.JumpedPiece != null
                ? result.JumpedPiece.Position
                : GridPosition.Invalid;
            var eventArgs = new PieceMoveEventArgs(
                piece, from, to, jumpedPosition, result.Success, result.ErrorMessage);

            OnMoveAttempted?.Invoke(eventArgs);
            if (!result.Success)
            {
                if (showDebugInfo)
                    Debug.LogWarning($"[PieceMoveManager] Invalid move: {result.ErrorMessage}");
                return;
            }

            OnMoveExecuted?.Invoke(result);
            OnPieceMoved?.Invoke(eventArgs);
        }

        private GridPosition? ScreenToGridPosition(Vector2 screenPosition)
        {
            if (board == null || EventSystem.current == null)
                return null;

            var results = new List<RaycastResult>();
            var eventData = new PointerEventData(EventSystem.current) { position = screenPosition };
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var result in results)
            {
                var mapGrid = result.gameObject.GetComponentInParent<MapGrid>();
                if (mapGrid != null)
                    return mapGrid.Position;
            }

            return null;
        }

        public IPiece GetSelectedPiece() => mSelectedPiece;
        public bool HasSelection() => mSelectedPiece != null;
    }
}
