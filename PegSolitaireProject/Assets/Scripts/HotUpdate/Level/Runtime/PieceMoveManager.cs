/****************************************************************************
 * Copyright (c) 2024 Game Project UNDER MIT License
 *
 * 棋子移动管理器
 *
 * 处理玩家的点击、拖拽、松开操作，验证并执行棋子移动
 ****************************************************************************/

using UnityEngine;
using UnityEngine.EventSystems;
using Game.Level.Data;

namespace Game.Level.Runtime
{
    /// <summary>
    /// 移动事件参数
    /// </summary>
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

    /// <summary>
    /// 棋子移动管理器
    /// 处理玩家交互：点击选择、拖拽、松开移动
    /// </summary>
    public class PieceMoveManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Board board;
        [SerializeField] private Camera uiCamera;

        [Header("Settings")]
        [SerializeField] private float dragThreshold = 10f;
        [SerializeField] private bool showDebugInfo = true;

        private BoardStateManager mBoardState;
        private IPiece mSelectedPiece;
        private Vector2 mDragStartPosition;
        private bool mIsDragging;
        private GridPosition mDragStartGridPosition;

        // 事件
        public event System.Action<IPiece> OnPieceSelected;
        public event System.Action<IPiece> OnPieceDeselected;
        public event System.Action<PieceMoveEventArgs> OnPieceMoved;
        public event System.Action<PieceMoveEventArgs> OnMoveAttempted;

        private void Awake()
        {
            if (uiCamera == null)
                uiCamera = Camera.main;
        }

        /// <summary>
        /// 初始化移动管理器
        /// </summary>
        public void Initialize(BoardStateManager boardState, Board boardRef = null)
        {
            mBoardState = boardState;
            
            if (boardRef != null)
                board = boardRef;

            Debug.Log("[PieceMoveManager] Initialized");
        }

        private void Update()
        {
            if (mBoardState == null)
                return;

            HandleInput();
        }

        #region 输入处理

        private void HandleInput()
        {
            // 鼠标/触摸按下
            if (Input.GetMouseButtonDown(0))
            {
                OnPointerDown(Input.mousePosition);
            }

            // 拖拽中
            if (Input.GetMouseButton(0) && mSelectedPiece != null)
            {
                OnPointerDrag(Input.mousePosition);
            }

            // 鼠标/触摸松开
            if (Input.GetMouseButtonUp(0) && mSelectedPiece != null)
            {
                OnPointerUp(Input.mousePosition);
            }
        }

        private void OnPointerDown(Vector2 screenPosition)
        {
            var gridPosition = ScreenToGridPosition(screenPosition);
            if (!gridPosition.HasValue)
                return;

            var piece = mBoardState.GetPieceAt(gridPosition.Value);
            if (piece != null && piece.IsMovable)
            {
                SelectPiece(piece, screenPosition, gridPosition.Value);
            }
        }

        private void OnPointerDrag(Vector2 screenPosition)
        {
            if (mSelectedPiece == null)
                return;

            // 检查是否超过拖拽阈值
            if (!mIsDragging)
            {
                var distance = Vector2.Distance(screenPosition, mDragStartPosition);
                if (distance > dragThreshold)
                {
                    mIsDragging = true;
                    Debug.Log($"[PieceMoveManager] Started dragging piece {mSelectedPiece.Id}");
                }
            }

            // TODO: 更新棋子视觉对象的位置跟随鼠标
            // var visualObj = mSelectedPiece.GetVisualObject();
            // if (visualObj != null)
            //     UpdateVisualPosition(visualObj, screenPosition);
        }

        private void OnPointerUp(Vector2 screenPosition)
        {
            if (mSelectedPiece == null)
                return;

            var targetGridPosition = ScreenToGridPosition(screenPosition);
            
            if (targetGridPosition.HasValue)
            {
                TryMovePiece(mSelectedPiece, mDragStartGridPosition, targetGridPosition.Value);
            }
            else
            {
                Debug.Log("[PieceMoveManager] Released outside valid grid");
            }

            DeselectPiece();
        }

        #endregion

        #region 棋子选择

        private void SelectPiece(IPiece piece, Vector2 screenPos, GridPosition gridPos)
        {
            // 取消之前的选择
            if (mSelectedPiece != null)
                DeselectPiece();

            mSelectedPiece = piece;
            mSelectedPiece.IsSelected = true;
            mDragStartPosition = screenPos;
            mDragStartGridPosition = gridPos;
            mIsDragging = false;

            OnPieceSelected?.Invoke(piece);
            
            if (showDebugInfo)
                Debug.Log($"[PieceMoveManager] Selected piece {piece.Id} at {gridPos}");
        }

        private void DeselectPiece()
        {
            if (mSelectedPiece == null)
                return;

            mSelectedPiece.IsSelected = false;
            var piece = mSelectedPiece;
            mSelectedPiece = null;
            mIsDragging = false;

            OnPieceDeselected?.Invoke(piece);
            
            if (showDebugInfo)
                Debug.Log($"[PieceMoveManager] Deselected piece {piece.Id}");
        }

        #endregion

        #region 移动执行

        /// <summary>
        /// 尝试移动棋子
        /// </summary>
        private void TryMovePiece(IPiece piece, GridPosition from, GridPosition to)
        {
            // 验证移动
            var result = piece.ValidateMove(from, to, mBoardState);

            // 触发移动尝试事件
            var eventArgs = new PieceMoveEventArgs(
                piece, from, to, result.JumpedPosition, result.IsValid, result.ErrorMessage);
            OnMoveAttempted?.Invoke(eventArgs);

            if (!result.IsValid)
            {
                if (showDebugInfo)
                    Debug.LogWarning($"[PieceMoveManager] Invalid move: {result.ErrorMessage}");
                return;
            }

            // 执行移动
            ExecuteMove(piece, from, to, result.JumpedPosition);
        }

        /// <summary>
        /// 执行移动
        /// </summary>
        private void ExecuteMove(IPiece piece, GridPosition from, GridPosition to, GridPosition jumped)
        {
            // 1. 更新棋盘状态
            mBoardState.MovePiece(piece, to);

            // 2. 移除被跨越的棋子
            mBoardState.RemovePiece(jumped);

            // 3. 触发移动成功事件
            var eventArgs = new PieceMoveEventArgs(piece, from, to, jumped, true);
            OnPieceMoved?.Invoke(eventArgs);

            if (showDebugInfo)
            {
                Debug.Log($"[PieceMoveManager] Moved {piece.Id} from {from} to {to}, jumped over {jumped}");
            }

            // 4. TODO: 播放移动动画
            // 5. TODO: 检查游戏结束条件
        }

        #endregion

        #region 工具方法

        /// <summary>
        /// 屏幕坐标转网格坐标
        /// </summary>
        private GridPosition? ScreenToGridPosition(Vector2 screenPosition)
        {
            if (board == null)
                return null;

            // TODO: 实现屏幕坐标到网格坐标的转换
            // 这需要根据你的UI布局和Board的坐标系统来实现
            // 可以通过Raycast或者直接计算

            // 临时实现：通过Raycast检测MapGrid
            var ray = uiCamera.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out var hit))
            {
                var mapGrid = hit.collider.GetComponent<MapGrid>();
                if (mapGrid != null)
                    return mapGrid.Position;
            }

            return null;
        }

        /// <summary>
        /// 获取当前选中的棋子
        /// </summary>
        public IPiece GetSelectedPiece() => mSelectedPiece;

        /// <summary>
        /// 检查是否有棋子被选中
        /// </summary>
        public bool HasSelection() => mSelectedPiece != null;

        #endregion

        #region 调试

        private void OnDrawGizmos()
        {
            if (!showDebugInfo || mBoardState == null)
                return;

            // 绘制所有棋子位置
            foreach (var piece in mBoardState.AllPieces)
            {
                var color = piece.IsMovable ? Color.green : Color.red;
                if (piece.IsSelected)
                    color = Color.yellow;
                
                // TODO: 绘制棋子位置标记
            }
        }

        #endregion
    }
}
