/****************************************************************************
 * 棋子移动系统使用示例和测试
 ****************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using Game.Level.Data;
using Game.Level.State;

namespace Game.Level.Runtime
{
    /// <summary>
    /// 棋子移动系统集成示例
    /// 展示如何在关卡中使用完整的移动系统
    /// </summary>
    public class PieceMoveSystemExample : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Board board;
        [SerializeField] private PieceMoveManager moveManager;

        private BoardStateManager mBoardState;
        private LevelStateMachine mStateMachine;

        private void Start()
        {
            // 监听移动事件
            if (moveManager != null)
            {
                moveManager.OnPieceSelected += OnPieceSelected;
                moveManager.OnPieceDeselected += OnPieceDeselected;
                moveManager.OnMoveAttempted += OnMoveAttempted;
                moveManager.OnPieceMoved += OnPieceMoved;
            }

            // 获取状态机
            mStateMachine = FindFirstObjectByType<LevelStateMachine>();
        }

        private void OnDestroy()
        {
            if (moveManager != null)
            {
                moveManager.OnPieceSelected -= OnPieceSelected;
                moveManager.OnPieceDeselected -= OnPieceDeselected;
                moveManager.OnMoveAttempted -= OnMoveAttempted;
                moveManager.OnPieceMoved -= OnPieceMoved;
            }
        }

        /// <summary>
        /// 初始化移动系统（在关卡就绪时调用）
        /// </summary>
        public void InitializeMoveSystem(LevelConfig config)
        {
            // 创建棋盘状态管理器
            mBoardState = new BoardStateManager(config);

            // 初始化移动管理器
            if (moveManager != null)
            {
                moveManager.Initialize(mBoardState, board);
            }

            Debug.Log("[Example] Piece move system initialized");
        }

        #region 事件处理

        private void OnPieceSelected(IPiece piece)
        {
            Debug.Log($"[Example] Piece selected: {piece.Id} at {piece.Position}");
            
            // TODO: 播放选中音效
            // TODO: 高亮显示可移动的目标位置
            // TODO: 显示移动提示UI
        }

        private void OnPieceDeselected(IPiece piece)
        {
            Debug.Log($"[Example] Piece deselected: {piece.Id}");
            
            // TODO: 取消高亮
            // TODO: 隐藏移动提示
        }

        private void OnMoveAttempted(PieceMoveEventArgs args)
        {
            if (!args.IsValid)
            {
                Debug.LogWarning($"[Example] Invalid move: {args.ErrorMessage}");
                
                // TODO: 播放错误音效
                // TODO: 显示错误提示UI
                // TODO: 震动反馈
            }
        }

        private void OnPieceMoved(PieceMoveEventArgs args)
        {
            Debug.Log($"[Example] Piece moved from {args.From} to {args.To}, jumped over {args.JumpedPosition}");
            
            // TODO: 播放移动音效
            // TODO: 播放跳跃动画
            // TODO: 更新分数
            // TODO: 检查胜利条件
            
            CheckWinCondition();
        }

        #endregion

        #region 游戏逻辑

        /// <summary>
        /// 检查胜利条件
        /// </summary>
        private void CheckWinCondition()
        {
            if (mBoardState == null)
                return;

            // 示例：如果只剩一个棋子，游戏胜利
            var remainingPieces = mBoardState.AllPieces.Count;
            Debug.Log($"[Example] Remaining pieces: {remainingPieces}");

            if (remainingPieces == 1)
            {
                Debug.Log("[Example] Victory! Only one piece remaining!");
                
                if (mStateMachine != null)
                    mStateMachine.Win();
            }
            // 检查是否还有可移动的棋子
            else if (!mBoardState.HasMovablePieces())
            {
                Debug.Log("[Example] Game Over! No more valid moves.");
                
                if (mStateMachine != null)
                    mStateMachine.Fail();
            }
        }

        /// <summary>
        /// 重置关卡
        /// </summary>
        public void ResetLevel()
        {
            if (mBoardState != null)
            {
                mBoardState.ResetAllPieces();
                Debug.Log("[Example] Level reset");
            }
        }

        #endregion
    }

    /// <summary>
    /// 移动规则测试工具
    /// </summary>
    public class MoveRulesTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool runTestsOnStart = false;

        private void Start()
        {
            if (runTestsOnStart)
                RunTests();
        }

        [ContextMenu("Run Move Rules Tests")]
        public void RunTests()
        {
            Debug.Log("========== 开始移动规则测试 ==========");

            TestDirectionValidation();
            TestDistanceValidation();
            TestJumpValidation();
            TestBoundaryValidation();

            Debug.Log("========== 测试完成 ==========");
        }

        private void TestDirectionValidation()
        {
            Debug.Log("--- 测试方向验证 ---");

            var config = CreateTestConfig(5, 5);
            var boardState = new BoardStateManager(config);
            var piece = boardState.GetPieceAt(new GridPosition(2, 2));

            // 测试上下左右（合法）
            TestMove(piece, new GridPosition(2, 2), new GridPosition(2, 0), boardState, "向上移动");
            TestMove(piece, new GridPosition(2, 2), new GridPosition(2, 4), boardState, "向下移动");
            TestMove(piece, new GridPosition(2, 2), new GridPosition(0, 2), boardState, "向左移动");
            TestMove(piece, new GridPosition(2, 2), new GridPosition(4, 2), boardState, "向右移动");

            // 测试斜向（非法）
            TestMove(piece, new GridPosition(2, 2), new GridPosition(4, 4), boardState, "斜向移动（应失败）");
        }

        private void TestDistanceValidation()
        {
            Debug.Log("--- 测试距离验证 ---");

            var config = CreateTestConfig(5, 5);
            var boardState = new BoardStateManager(config);
            var piece = boardState.GetPieceAt(new GridPosition(2, 2));

            // 测试距离为2（合法，假设中间有棋子）
            TestMove(piece, new GridPosition(2, 2), new GridPosition(2, 0), boardState, "距离为2");

            // 测试距离为1（非法）
            TestMove(piece, new GridPosition(2, 2), new GridPosition(2, 1), boardState, "距离为1（应失败）");

            // 测试距离为3（非法）
            TestMove(piece, new GridPosition(2, 2), new GridPosition(2, 5), boardState, "距离为3（应失败）");
        }

        private void TestJumpValidation()
        {
            Debug.Log("--- 测试跳跃验证 ---");
            // TODO: 创建特定布局测试跳跃规则
        }

        private void TestBoundaryValidation()
        {
            Debug.Log("--- 测试边界验证 ---");

            var config = CreateTestConfig(5, 5);
            var boardState = new BoardStateManager(config);
            var piece = boardState.GetPieceAt(new GridPosition(0, 0));

            // 测试超出边界
            TestMove(piece, new GridPosition(0, 0), new GridPosition(-2, 0), boardState, "超出左边界（应失败）");
            TestMove(piece, new GridPosition(0, 0), new GridPosition(0, -2), boardState, "超出上边界（应失败）");
        }

        private void TestMove(IPiece piece, GridPosition from, GridPosition to, IBoardState board, string testName)
        {
            if (piece == null)
            {
                Debug.LogWarning($"[{testName}] 棋子为空，跳过测试");
                return;
            }

            var result = piece.ValidateMove(from, to, board);
            var status = result.IsValid ? "✓ 通过" : "✗ 失败";
            Debug.Log($"[{testName}] {status} - {(result.IsValid ? "合法移动" : result.ErrorMessage)}");
        }

        private LevelConfig CreateTestConfig(int width, int height)
        {
            var config = new LevelConfig();
            config.levelId = "test_level";
            config.board = new BoardData { width = width, height = height };
            
            // 创建简单的测试布局
            config.pieces = new List<PieceData>
            {
                new PieceData { id = "p1", position = new GridPosition(2, 2), isMovable = true, pieceType = PieceType.Peg },
                new PieceData { id = "p2", position = new GridPosition(2, 1), isMovable = false, pieceType = PieceType.Stone },
                new PieceData { id = "p3", position = new GridPosition(1, 2), isMovable = false, pieceType = PieceType.Stone },
                new PieceData { id = "p4", position = new GridPosition(3, 2), isMovable = false, pieceType = PieceType.Stone },
                new PieceData { id = "p5", position = new GridPosition(2, 3), isMovable = false, pieceType = PieceType.Stone },
            };

            return config;
        }
    }
}

/****************************************************************************
 * 使用说明
 ****************************************************************************/

/*

## 完整集成流程

1. **在关卡就绪状态初始化移动系统**

在 `StateLevelReady.OnEnter()` 中：

```csharp
protected override void OnEnter()
{
    // 初始化棋盘状态
    var boardState = new BoardStateManager(Context.Config);
    
    // 初始化移动管理器
    var moveManager = FindFirstObjectByType<PieceMoveManager>();
    var board = FindFirstObjectByType<Board>();
    
    if (moveManager != null)
    {
        moveManager.Initialize(boardState, board);
        
        // 监听移动事件
        moveManager.OnPieceMoved += (args) =>
        {
            Debug.Log($"Piece moved from {args.From} to {args.To}");
            CheckWinCondition(boardState);
        };
    }
    
    mFSM.ChangeState(LevelState.LevelRunning);
}
```

2. **检查游戏结束条件**

```csharp
private void CheckWinCondition(BoardStateManager boardState)
{
    // 只剩一个棋子 = 胜利
    if (boardState.AllPieces.Count == 1)
    {
        mFSM.ChangeState(LevelState.LevelSuccess);
        return;
    }
    
    // 没有合法移动 = 失败
    if (!boardState.HasMovablePieces())
    {
        mFSM.ChangeState(LevelState.LevelFail);
    }
}
```

3. **Scene 设置**

在 Unity Scene 中：
- 添加 `PieceMoveManager` 组件到场景
- 关联 `Board` 引用
- 设置 UI Camera

4. **扩展自定义棋子类型**

```csharp
public class SpecialPiece : PieceBase
{
    public SpecialPiece(PieceData config, GameObject visualObject = null) 
        : base(config, visualObject)
    {
    }
    
    // 覆盖验证方法添加特殊规则
    protected override MoveResult ValidateMoveCustom(
        GridPosition from, GridPosition to, IBoardState board, MoveResult baseResult)
    {
        // 例如：特殊棋子可以跨越两个棋子
        // 添加你的自定义逻辑
        
        return baseResult;
    }
}
```

## 移动规则总结

✓ **支持的移动方向**：上、下、左、右
✗ **不支持的方向**：斜向

✓ **移动距离**：必须刚好为 2（跨越一个棋子）
✓ **路径检查**：中间位置必须有棋子
✓ **目标检查**：目标位置必须为空且在棋盘内

## API 快速参考

```csharp
// 创建棋子
var piece = PieceFactory.CreatePiece(pieceData);

// 验证移动
var result = piece.ValidateMove(from, to, boardState);
if (result.IsValid)
{
    // 执行移动
    boardState.MovePiece(piece, to);
    boardState.RemovePiece(result.JumpedPosition);
}

// 检查游戏状态
bool hasMovablePieces = boardState.HasMovablePieces();
int remainingPieces = boardState.AllPieces.Count;
```

*/
