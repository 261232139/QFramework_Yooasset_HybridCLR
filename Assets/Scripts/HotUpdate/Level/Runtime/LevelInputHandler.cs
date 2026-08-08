/****************************************************************************
 * Copyright (c) 2024 Game Project UNDER MIT License
 *
 * 关卡输入处理器
 *
 * 专门负责处理玩家输入和棋子交互
 ****************************************************************************/

using UnityEngine;
using UnityEngine.EventSystems;
using Game.Level.Data;
using System.Collections;

namespace Game.Level.Runtime
{
    /// <summary>
    /// 关卡输入处理器
    /// 负责处理玩家的拖拽、点击等输入操作
    /// </summary>
    public class LevelInputHandler : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float dragThreshold = 10f;
        [SerializeField] private bool enableDebugLog = false;

        private LevelView levelView;
        private Board board;
        private Camera uiCamera;
        
        private IPiece selectedPiece;
        private GridPosition selectedGridPosition;
        private Vector2 dragStartPosition;
        private bool isDragging;
        private bool inputEnabled;

        // 事件
        public event System.Action<IPiece, GridPosition, GridPosition> OnMoveRequested;
        public event System.Action<IPiece> OnPieceSelected;
        public event System.Action OnPieceDeselected;

        private void Awake()
        {
            uiCamera = Camera.main;
        }

        public void Initialize(LevelView view, Board boardRef)
        {
            levelView = view;
            board = boardRef;
            
            if (enableDebugLog)
                Debug.Log("[LevelInputHandler] Initialized");
        }

        public void EnableInput(bool enabled)
        {
            inputEnabled = enabled;
            
            if (!enabled && selectedPiece != null)
            {
                DeselectPiece();
            }
        }

        private void Update()
        {
            if (!inputEnabled || levelView == null || !levelView.IsPlaying)
                return;

            HandleInput();
        }

        #region 输入处理

        private void HandleInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                OnPointerDown(Input.mousePosition);
            }

            if (Input.GetMouseButton(0) && selectedPiece != null)
            {
                OnPointerDrag(Input.mousePosition);
            }

            if (Input.GetMouseButtonUp(0) && selectedPiece != null)
            {
                OnPointerUp(Input.mousePosition);
            }
        }

        private void OnPointerDown(Vector2 screenPosition)
        {
            var gridPosition = ScreenToGridPosition(screenPosition);
            if (!gridPosition.HasValue)
                return;

            var piece = levelView.BoardState?.GetPieceAt(gridPosition.Value);
            if (piece != null && piece.IsMovable)
            {
                SelectPiece(piece, screenPosition, gridPosition.Value);
            }
        }

        private void OnPointerDrag(Vector2 screenPosition)
        {
            if (selectedPiece == null)
                return;

            if (!isDragging)
            {
                var distance = Vector2.Distance(screenPosition, dragStartPosition);
                if (distance > dragThreshold)
                {
                    isDragging = true;
                    if (enableDebugLog)
                        Debug.Log($"[LevelInputHandler] Started dragging piece {selectedPiece.Id}");
                }
            }

            // 更新棋子视觉对象位置（如果需要跟随鼠标）
            // 这里可以添加拖拽时的视觉反馈
        }

        private void OnPointerUp(Vector2 screenPosition)
        {
            if (selectedPiece == null)
                return;

            var targetGridPosition = ScreenToGridPosition(screenPosition);
            
            if (targetGridPosition.HasValue && isDragging)
            {
                TryMovePiece(selectedPiece, selectedGridPosition, targetGridPosition.Value);
            }

            DeselectPiece();
        }

        #endregion

        #region 棋子选择

        private void SelectPiece(IPiece piece, Vector2 screenPos, GridPosition gridPos)
        {
            if (selectedPiece != null)
                DeselectPiece();

            selectedPiece = piece;
            selectedPiece.IsSelected = true;
            selectedGridPosition = gridPos;
            dragStartPosition = screenPos;
            isDragging = false;

            OnPieceSelected?.Invoke(piece);
            
            if (enableDebugLog)
                Debug.Log($"[LevelInputHandler] Selected piece {piece.Id} at {gridPos}");
        }

        private void DeselectPiece()
        {
            if (selectedPiece == null)
                return;

            selectedPiece.IsSelected = false;
            var piece = selectedPiece;
            selectedPiece = null;
            isDragging = false;

            OnPieceDeselected?.Invoke();
            
            if (enableDebugLog)
                Debug.Log($"[LevelInputHandler] Deselected piece {piece.Id}");
        }

        #endregion

        #region 移动处理

        private void TryMovePiece(IPiece piece, GridPosition from, GridPosition to)
        {
            if (levelView == null)
                return;

            // 通知 LevelView 处理移动请求
            OnMoveRequested?.Invoke(piece, from, to);

            if (enableDebugLog)
            {
                Debug.Log($"[LevelInputHandler] Move requested: {piece.Id} from {from} to {to}");
            }
        }

        #endregion

        #region 工具方法

        private GridPosition? ScreenToGridPosition(Vector2 screenPosition)
        {
            if (board == null)
                return null;

            // 通过射线检测获取 MapGrid
            var results = new System.Collections.Generic.List<RaycastResult>();
            var eventData = new PointerEventData(EventSystem.current)
            {
                position = screenPosition
            };
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var result in results)
            {
                var mapGrid = result.gameObject.GetComponent<MapGrid>();
                if (mapGrid != null)
                    return mapGrid.Position;
            }

            return null;
        }

        #endregion
    }
}
