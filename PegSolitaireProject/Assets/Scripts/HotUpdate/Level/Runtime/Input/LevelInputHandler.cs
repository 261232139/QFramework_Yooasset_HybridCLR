using System.Collections.Generic;
using Game.Level.Data;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Level.Runtime
{
    /// <summary>处理关卡中的点击选择与点击移动。</summary>
    public class LevelInputHandler : MonoBehaviour
    {
        [SerializeField] private bool enableDebugLog;

        private LevelView levelView;
        private Board board;
        private IPiece selectedPiece;
        private GridPosition selectedGridPosition;
        private bool inputEnabled;

        public event System.Action<IPiece, GridPosition, GridPosition> OnMoveRequested;
        public event System.Action<IPiece> OnPieceSelected;
        public event System.Action OnPieceDeselected;

        public void Initialize(LevelView view, Board boardRef)
        {
            levelView = view;
            board = boardRef;
        }

        public void EnableInput(bool enabled)
        {
            inputEnabled = enabled;
            if (!enabled)
                DeselectPiece();
        }

        private void Update()
        {
            if (inputEnabled && levelView != null && levelView.IsPlaying && Input.GetMouseButtonDown(0))
                HandleClick(Input.mousePosition);
        }

        private void HandleClick(Vector2 screenPosition)
        {
            var gridPosition = ScreenToGridPosition(screenPosition);
            if (!gridPosition.HasValue)
            {
                DeselectPiece();
                return;
            }

            var clickedPiece = levelView.BoardState?.GetPieceAt(gridPosition.Value);
            if (selectedPiece == null)
            {
                if (clickedPiece != null && clickedPiece.IsMovable)
                    SelectPiece(clickedPiece, gridPosition.Value);

                return;
            }

            if (clickedPiece != null && clickedPiece.IsMovable)
            {
                SelectPiece(clickedPiece, gridPosition.Value);
                return;
            }

            if (clickedPiece == null)
                OnMoveRequested?.Invoke(selectedPiece, selectedGridPosition, gridPosition.Value);

            DeselectPiece();
        }

        private void SelectPiece(IPiece piece, GridPosition gridPosition)
        {
            if (selectedPiece == piece)
            {
                DeselectPiece();
                return;
            }

            DeselectPiece();
            selectedPiece = piece;
            selectedPiece.IsSelected = true;
            selectedGridPosition = gridPosition;
            OnPieceSelected?.Invoke(piece);

            if (enableDebugLog)
                Debug.Log($"[LevelInputHandler] Selected piece {piece.Id} at {gridPosition}");
        }

        private void DeselectPiece()
        {
            if (selectedPiece == null)
                return;

            selectedPiece.IsSelected = false;
            selectedPiece = null;
            OnPieceDeselected?.Invoke();
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
    }
}
