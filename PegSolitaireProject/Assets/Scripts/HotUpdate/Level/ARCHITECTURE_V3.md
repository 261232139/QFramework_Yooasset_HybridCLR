# 关卡系统架构文档 v3.0 - LevelView 架构

> **已过时快照。** 现行架构见 [ARCHITECTURE_V4.md](./ARCHITECTURE_V4.md)。  
> v3.0 记录的是 LevelView 初版：输入仍按 `LevelInputHandler`、胜利仍按 `LevelGoalType`、棋子类型仍按 Peg/Gem/Stone。这些与当前代码不符，仅作演进对照。


## 一、架构概览

### 设计原则

本次重构采用 **MVC 模式**，将关卡系统分为三个清晰的层次：

1. **Model（模型层）**: 纯逻辑，无 Unity 依赖
   - `LevelController` - 游戏逻辑控制器
   - `BoardStateManager` - 棋盘状态管理
   - `LevelGoalManager` - 目标管理

2. **View（视图层）**: Unity 组件，负责呈现
   - `LevelView` - 总指挥官
   - `Board` - 棋盘布局视图
   - `LevelUIController` - UI 视图

3. **Controller（控制层）**: 协调 Model 和 View
   - `LevelView` 充当主协调者
   - `LevelInputHandler` 处理输入

---

## 二、核心组件架构

### 组件关系图

```
LevelView (总指挥官 - MonoBehaviour)
    │
    ├── LevelController (逻辑控制器 - 纯 C# 类)
    │   ├── BoardStateManager (状态管理)
    │   │   └── IPiece × M (棋子逻辑)
    │   └── LevelGoalManager (目标管理)
    │
    ├── Board (布局视图 - MonoBehaviour)
    │   ├── MapGrid × N (格子)
    │   └── GameObject × M (棋子视觉对象)
    │
    ├── LevelInputHandler (输入处理 - MonoBehaviour)
    │
    └── LevelUIController (UI控制 - MonoBehaviour)
```

### 职责划分表

| 组件 | 类型 | 职责 | 不负责 |
|------|------|------|--------|
| **LevelView** | MonoBehaviour | 总协调、生命周期管理、组件通信 | 具体游戏逻辑、布局细节 |
| **LevelController** | 纯 C# 类 | 游戏逻辑、规则验证、状态管理 | Unity 相关、视图更新 |
| **Board** | MonoBehaviour | 棋盘布局、视觉呈现、坐标转换 | 游戏逻辑、状态管理 |
| **BoardStateManager** | 纯 C# 类 | 棋子状态、移动验证 | 视图更新 |
| **LevelGoalManager** | 纯 C# 类 | 目标跟踪、分数计算 | UI 更新、视图呈现 |
| **LevelInputHandler** | MonoBehaviour | 输入检测、交互处理 | 移动验证、视图更新 |
| **LevelUIController** | MonoBehaviour | UI 显示和更新 | 游戏逻辑 |

---

## 三、组件详细说明

### 1. LevelView（总指挥官）

**路径**: `Assets/Scripts/HotUpdate/Level/Runtime/LevelView.cs`

**定位**: 场景入口，负责整体协调

**职责**：
- 创建和管理 `LevelController`
- 协调各个 View 组件
- 处理组件间的通信
- 管理关卡生命周期

**关键设计**：
```csharp
public class LevelView : MonoBehaviour
{
    [SerializeField] private Board board;
    [SerializeField] private LevelInputHandler inputHandler;
    [SerializeField] private LevelUIController uiController;
    [SerializeField] private LevelGoal levelGoal;
    
    // Controller 运行时创建（不是序列化字段）
    private LevelController controller;
    
    public void LoadLevel(LevelConfig config, int levelNumber)
    {
        // 1. 创建逻辑控制器
        var goalManager = new LevelGoalManager(levelGoal);
        controller = new LevelController(config, goalManager);
        
        // 2. 只负责布局
        board.BuildLayout(config);
        
        // 3. 初始化其他组件
        InitializeManagers();
    }
    
    // 处理输入请求
    public void HandleMoveRequested(IPiece piece, GridPosition from, GridPosition to)
    {
        var result = controller.ExecuteMove(piece, to);
        if (result.Success)
        {
            board.UpdatePieceVisual(result);
            uiController.OnPieceMoved();
        }
    }
}
```

---

### 2. LevelController（逻辑控制器）

**路径**: `Assets/Scripts/HotUpdate/Level/Runtime/LevelController.cs`

**定位**: 纯逻辑控制器，不依赖 Unity

**职责**：
- 持有 `BoardStateManager` 和 `LevelGoalManager`
- 验证和执行移动
- 检查游戏结束条件
- 管理游戏规则

**优势**：
- 可以独立单元测试
- 不依赖 Unity，逻辑清晰
- 易于复用和扩展

**关键方法**：
```csharp
public class LevelController
{
    private readonly BoardStateManager boardState;
    private readonly LevelGoalManager goalManager;
    
    public MoveResult ExecuteMove(IPiece piece, GridPosition to)
    {
        // 验证
        var validation = piece.ValidateMove(from, to, boardState);
        if (!validation.IsValid)
            return MoveResult.CreateFailure(...);
        
        // 执行
        boardState.MovePiece(piece, to);
        if (jumpedPiece != null)
            boardState.RemovePiece(jumpedPiece.Position);
        
        // 更新目标
        goalManager.OnPieceMoved(piece, from, to, jumpedPiece);
        
        return MoveResult.CreateSuccess(...);
    }
    
    public bool CheckGameOver() => !boardState.HasMovablePieces();
    public bool IsGoalCompleted() => goalManager.IsGoalCompleted();
}
```

---

### 3. Board（布局视图）

**路径**: `Assets/Scripts/HotUpdate/Level/Runtime/Board.cs`

**定位**: 纯视图组件，负责呈现

**核心改变**：
- ❌ 不再持有 `BoardStateManager`
- ❌ 不再处理游戏逻辑
- ✅ 只负责布局生成
- ✅ 被动接收更新指令

**关键方法**：
```csharp
public class Board : MonoBehaviour
{
    // 不再持有 BoardStateManager
    private LevelConfig currentConfig;
    private Dictionary<string, GameObject> pieceObjects;
    
    // 只负责布局
    public void BuildLayout(LevelConfig config)
    {
        // 生成格子
        for (var y = 0; y < config.board.height; y++)
            for (var x = 0; x < config.board.width; x++)
                CreateGrid(x, y);
        
        // 生成棋子视觉对象
        foreach (var piece in config.pieces)
            CreatePieceVisual(piece);
    }
    
    // 被动更新视图
    public void UpdatePieceVisual(MoveResult result)
    {
        // 移动视觉对象
        var pieceObj = pieceObjects[result.MovedPiece.Id];
        pieceObj.transform.SetParent(toGrid.transform);
        
        // 移除被跳跃的棋子
        if (result.JumpedPiece != null)
            RemovePieceVisual(result.JumpedPiece.Id);
    }
}
```

---

### 4. LevelGoalManager（目标管理器）

**路径**: `Assets/Scripts/HotUpdate/Level/Runtime/LevelGoalManager.cs`

**核心改变**：
- ❌ 不再继承 `MonoBehaviour`
- ✅ 改为纯 C# 类
- ✅ 被 `LevelController` 持有

**关键改变**：
```csharp
// 旧: public class LevelGoalManager : MonoBehaviour
// 新: public class LevelGoalManager
public class LevelGoalManager
{
    private readonly LevelGoal currentGoal;
    private BoardStateManager boardState;
    
    public LevelGoalManager(LevelGoal goal = null)
    {
        currentGoal = goal ?? new LevelGoal();
    }
    
    public void Initialize(BoardStateManager state)
    {
        boardState = state;
    }
    
    public void OnPieceMoved(IPiece piece, GridPosition from, GridPosition to, IPiece jumpedPiece)
    {
        moveCount++;
        OnMoveCountChanged?.Invoke(moveCount);
        
        if (jumpedPiece != null)
            AddScore(CalculateScore(jumpedPiece));
    }
    
    public bool IsGoalCompleted()
    {
        // 直接使用 boardState，不再依赖 LevelScene
        var remainingPieces = boardState.AllPieces.Count;
        // ...
    }
}
```

---

### 5. LevelInputHandler（输入处理器）

**路径**: `Assets/Scripts/HotUpdate/Level/Runtime/LevelInputHandler.cs`

**核心改变**：
- ❌ 不再直接执行移动
- ✅ 只负责输入检测
- ✅ 通过事件通知 `LevelView`

**数据流**：
```
玩家输入 
  → LevelInputHandler.OnPointerUp()
  → 触发事件 OnMoveRequested(piece, from, to)
  → LevelView.HandleMoveRequested()
  → LevelController.ExecuteMove()
  → Board.UpdatePieceVisual()
```

**关键改变**：
```csharp
public class LevelInputHandler : MonoBehaviour
{
    private LevelView levelView;
    
    // 事件：请求移动（不是"已移动"）
    public event Action<IPiece, GridPosition, GridPosition> OnMoveRequested;
    
    public void Initialize(LevelView view, Board boardRef)
    {
        levelView = view;
        board = boardRef;
    }
    
    private void TryMovePiece(IPiece piece, GridPosition from, GridPosition to)
    {
        // 只负责通知，不执行逻辑
        OnMoveRequested?.Invoke(piece, from, to);
    }
}
```

---

### 6. LevelUIController（UI 控制器）

**路径**: `Assets/Scripts/HotUpdate/Level/Runtime/LevelUIController.cs`

**核心改变**：
- 引用 `LevelView` 而不是 `LevelScene`
- 其他逻辑保持不变

---

### 7. MoveResult（数据结构）

**路径**: `Assets/Scripts/HotUpdate/Level/Runtime/MoveResult.cs`

**新增**: 封装移动结果

**用途**：
- 在 Controller 和 View 之间传递移动结果
- 包含成功/失败、移动的棋子、被跳跃的棋子等信息

```csharp
public class MoveResult
{
    public bool Success { get; }
    public IPiece MovedPiece { get; }
    public GridPosition From { get; }
    public GridPosition To { get; }
    public IPiece JumpedPiece { get; }
    public string ErrorMessage { get; }
    
    public static MoveResult CreateSuccess(...);
    public static MoveResult CreateFailure(...);
}
```

---

## 四、数据流和事件流

### 1. 关卡启动流程

```
StateLoadLevel.OnEnter()
  ↓
加载 LevelScene 预制体
  ↓
检测组件类型:
  - 找到 LevelView → 使用新架构
  - 只找到 LevelScene → 使用旧架构（兼容模式）
  ↓
LevelView.LoadLevel(config, levelNumber)
  ├─ 创建 LevelController(config, goalManager)
  ├─ Board.BuildLayout(config)
  ├─ 初始化 InputHandler, UIController
  └─ 订阅事件
  ↓
StateLevelReady.OnEnter()
  ↓
LevelView.StartLevel()
  ├─ InputHandler.EnableInput(true)
  ├─ GoalManager.StartTracking()
  └─ UIController.OnLevelStart()
  ↓
StateLevelRunning (游戏进行中)
```

### 2. 棋子移动流程（新架构）

```
玩家拖拽棋子
  ↓
LevelInputHandler 检测输入
  ├─ OnPointerDown → 选中棋子
  ├─ OnPointerDrag → 拖拽反馈
  └─ OnPointerUp → 检测目标位置
  ↓
触发事件: OnMoveRequested(piece, from, to)
  ↓
LevelView.HandleMoveRequested(piece, from, to)
  ↓
LevelController.ExecuteMove(piece, to)
  ├─ piece.ValidateMove() (验证)
  ├─ boardState.MovePiece() (更新状态)
  ├─ boardState.RemovePiece() (移除被跳越的)
  ├─ goalManager.OnPieceMoved() (更新目标)
  └─ 返回 MoveResult
  ↓
if (result.Success)
  ├─ Board.UpdatePieceVisual(result) (更新视图)
  ├─ UIController.OnPieceMoved() (更新UI)
  └─ LevelView.CheckGameOver() (检查结束)
```

### 3. 旧架构 vs 新架构对比

| 流程节点 | 旧架构 (LevelScene) | 新架构 (LevelView) |
|---------|-------------------|-------------------|
| **输入检测** | LevelInputHandler | LevelInputHandler |
| **移动验证** | InputHandler 内部 | LevelController |
| **状态更新** | Board.BoardState | LevelController.BoardState |
| **视图更新** | Board.MovePiece() | Board.UpdatePieceVisual() |
| **目标更新** | LevelGoalManager (MonoBehaviour) | LevelGoalManager (纯C#) |
| **协调者** | LevelScene | LevelView |

---

## 五、优势总结

### 1. 清晰的职责分离

**旧架构问题**:
- Board 既管状态又管视图
- LevelScene 既管生命周期又管逻辑
- 组件职责交叉，难以维护

**新架构优势**:
- LevelController: 纯逻辑，无 Unity 依赖
- Board: 纯视图，只负责呈现
- LevelView: 纯协调，连接各组件

### 2. 可测试性

**旧架构**:
```csharp
// 难以测试 - 依赖 MonoBehaviour
public class Board : MonoBehaviour
{
    public void MovePiece(IPiece piece, GridPosition to)
    {
        boardState.MovePiece(piece, to);
        // 更新视图...
    }
}
```

**新架构**:
```csharp
// 易于测试 - 纯 C# 类
[Test]
public void TestMoveValidation()
{
    var config = CreateMockConfig();
    var goalManager = new LevelGoalManager();
    var controller = new LevelController(config, goalManager);
    
    var result = controller.ExecuteMove(piece, targetPos);
    Assert.IsTrue(result.Success);
}
```

### 3. 易于扩展

添加新功能只需修改 `LevelController`:

```csharp
public class LevelController
{
    // 新增功能
    public void UndoMove() { }
    public void UseHint() { }
    public void ActivatePowerUp(PowerUpType type) { }
}
```

不需要修改 View 层组件。

### 4. 更好的代码复用

`LevelController` 可以在不同场景中复用：
- 单人模式
- 多人对战模式
- AI 训练模式

只需要更换不同的 View 层实现。

---

## 六、向后兼容性

### StateLoadLevel 的兼容处理

```csharp
// 优先查找 LevelView（新架构）
var levelView = levelSceneObj.GetComponent<LevelView>();
if (levelView != null)
{
    Debug.Log("Using LevelView (new architecture)");
    levelView.LoadLevel(Context.Config, Context.LevelNumber);
}
else
{
    // 回退到旧的 LevelScene（兼容模式）
    var levelScene = levelSceneObj.GetComponent<LevelScene>();
    Debug.LogWarning("Using LevelScene (legacy mode)");
    levelScene.LoadLevel(Context.Config, Context.LevelNumber);
}
```

### 迁移步骤

1. ✅ 创建新组件 (LevelView, LevelController, MoveResult)
2. ✅ 重构现有组件 (Board, LevelGoalManager, LevelInputHandler)
3. ✅ 更新 StateLoadLevel 支持两种模式
4. ⏳ 在 Unity 中创建新的 LevelScene 预制体
5. ⏳ 测试新架构
6. ⏳ 逐步废弃旧的 LevelScene

---

## 七、Unity 场景配置指南

### LevelScene 预制体结构（新架构）

```
LevelScene (GameObject)
├── LevelView (Script) ← 替换 LevelScene 脚本
│   ├─ board: Board 引用
│   ├─ inputHandler: LevelInputHandler 引用
│   ├─ uiController: LevelUIController 引用
│   └─ levelGoal: LevelGoal 配置
│
├── BG (Image)
│
├── GameArea
│   ├── Board (GameObject)
│   │   ├─ Board (Script)
│   │   ├─ cellSize: (150, 150)
│   │   ├─ cellSpacing: (10, 10)
│   │   ├─ pegPiecePrefab: 引用
│   │   ├─ gemPiecePrefab: 引用
│   │   ├─ stonePiecePrefab: 引用
│   │   └─ MapGrid (模板，隐藏)
│   │
│   └── LevelInputHandler (GameObject)
│       └─ LevelInputHandler (Script)
│
├── UIArea
│   └── LevelUIController (GameObject)
│       └─ LevelUIController (Script)
│           ├─ UI 元素引用...
│           └─ 按钮引用...
```

### 配置步骤

1. **复制现有的 LevelScene 预制体**
2. **替换脚本**:
   - 移除 `LevelScene` 组件
   - 添加 `LevelView` 组件
3. **配置 LevelView**:
   - 拖拽 Board GameObject
   - 拖拽 LevelInputHandler GameObject
   - 拖拽 LevelUIController GameObject
   - 配置 Level Goal 参数
4. **测试运行**

---

## 八、常见问题

### Q1: 为什么 LevelController 不是 MonoBehaviour?

**A**: 为了提高可测试性和复用性。纯 C# 类可以:
- 独立于 Unity 进行单元测试
- 在不同的 Unity 场景中复用
- 更清晰地表达逻辑层和视图层的分离

### Q2: Board 不持有状态，如何获取棋子信息?

**A**: Board 只负责视图，不需要知道状态:
- 布局时: 从配置数据 `LevelConfig.pieces` 读取
- 更新时: 接收 `MoveResult` 参数，包含所有需要的信息

### Q3: 如何在旧预制体和新预制体之间切换?

**A**: StateLoadLevel 会自动检测:
- 找到 LevelView → 使用新架构
- 只找到 LevelScene → 使用旧架构

### Q4: 性能有影响吗?

**A**: 几乎没有:
- 事件订阅/取消的开销可忽略
- 额外的一层间接调用不会造成性能问题
- 代码清晰度的提升远大于微小的性能开销

---

## 九、下一步工作

1. ✅ 代码重构完成
2. ⏳ 在 Unity 中创建新的 LevelScene 预制体
3. ⏳ 配置所有引用和参数
4. ⏳ 运行并测试游戏流程
5. ⏳ 修复可能出现的问题
6. ⏳ 编写单元测试
7. ⏳ 更新文档和注释

---

## 十、总结

### 架构改进

| 方面 | v2.0 (LevelScene) | v3.0 (LevelView) |
|------|------------------|------------------|
| **主控制器** | LevelScene | LevelView |
| **逻辑控制** | 分散在各组件 | LevelController (集中) |
| **Board 职责** | 布局 + 状态管理 | 仅布局 |
| **状态管理** | Board 持有 | Controller 持有 |
| **目标管理** | MonoBehaviour | 纯 C# 类 |
| **可测试性** | 困难 | 容易 |
| **可扩展性** | 一般 | 优秀 |
| **代码清晰度** | 一般 | 优秀 |

### 核心思想

**关注点分离 (Separation of Concerns)**:
- Model: 纯逻辑，可测试
- View: 纯呈现，被动更新
- Controller: 协调通信

**这是一个更专业、更易维护的架构！** ✨
