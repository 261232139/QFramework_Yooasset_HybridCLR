# 关卡系统架构文档 v2.0

## 架构设计原则

遵循**单一职责原则**和**关注点分离**，将关卡系统拆分为多个独立的管理器，每个管理器只负责一个特定的职能。

---

## 一、核心组件架构

### 组件关系图

```
LevelScene (关卡主控制器)
    ├── Board (布局管理器)
    │   ├── MapGrid × N (格子)
    │   └── BoardStateManager (状态管理)
    │       └── IPiece × M (棋子逻辑)
    │
    ├── LevelInputHandler (输入处理器)
    │   └── 处理拖拽、点击、移动
    │
    ├── LevelGoalManager (目标管理器)
    │   └── 跟踪目标、计算分数
    │
    └── LevelUIController (UI 控制器)
        └── 更新 UI、显示面板
```

### 职责划分表

| 组件 | 职责 | 不负责 |
|------|------|--------|
| **LevelScene** | 关卡生命周期、组件协调 | 布局、输入、UI 细节 |
| **Board** | 棋盘和棋子的视觉布局 | 游戏逻辑、输入处理 |
| **LevelInputHandler** | 玩家输入、拖拽交互 | 布局、目标检测 |
| **LevelGoalManager** | 目标跟踪、分数计算 | 输入、布局、UI |
| **LevelUIController** | UI 显示和更新 | 游戏逻辑、输入 |
| **BoardStateManager** | 棋子状态、游戏规则 | UI、输入、布局 |

---

## 二、组件详细说明

### 1. LevelScene（关卡主控制器）

**路径**: `Assets/Scripts/HotUpdate/Level/Runtime/LevelScene.cs`

**职责**：
- 关卡生命周期管理（加载、开始、暂停、完成、失败）
- 协调各个子管理器
- 处理关卡事件的分发

**主要方法**：
```csharp
void LoadLevel(LevelConfig config, int levelNumber)
void StartLevel()
void PauseLevel()
void ResumeLevel()
void CompleteLevel()
void FailLevel()
void ResetLevel()
```

**事件**：
```csharp
event Action<LevelConfig> OnLevelStarted
event Action OnLevelCompleted
event Action OnLevelFailed
```

---

### 2. Board（布局管理器）

**路径**: `Assets/Scripts/HotUpdate/Level/Runtime/Board.cs`

**职责**：
- 从配置文件生成棋盘格子布局
- 实例化棋子的视觉对象
- 管理 MapGrid 和棋子 GameObject
- 提供坐标转换工具

**主要方法**：
```csharp
void Build(LevelConfig config)
void MovePiece(IPiece piece, GridPosition to)
void RemovePiece(IPiece piece)
MapGrid GetGridAt(GridPosition position)
```

**属性**：
```csharp
LevelConfig CurrentConfig
BoardStateManager BoardState
Vector2 CellSize
Vector2 CellSpacing
```

---

### 3. LevelInputHandler（输入处理器）

**路径**: `Assets/Scripts/HotUpdate/Level/Runtime/LevelInputHandler.cs`

**职责**：
- 处理玩家的鼠标/触摸输入
- 棋子的选择和拖拽
- 移动验证和执行
- 输入的启用/禁用控制

**主要方法**：
```csharp
void Initialize(LevelScene scene, Board board)
void EnableInput(bool enabled)
```

**事件**：
```csharp
event Action<IPiece, GridPosition, GridPosition, IPiece> OnPieceMoved
event Action<IPiece> OnPieceSelected
event Action OnPieceDeselected
```

**工作流程**：
```
1. 鼠标按下 → 检测棋子 → 选中
2. 鼠标拖拽 → 跟随移动（可选）
3. 鼠标松开 → 检测目标格子 → 验证移动 → 执行/取消
```

---

### 4. LevelGoalManager（目标管理器）

**路径**: `Assets/Scripts/HotUpdate/Level/Runtime/LevelGoalManager.cs`

**职责**：
- 定义和管理关卡目标
- 跟踪移动次数
- 计算分数
- 检测目标完成/失败

**目标类型**：
```csharp
public enum LevelGoalType
{
    ClearAll,       // 清除所有棋子
    RemainOne,      // 只剩一个棋子
    ClearSpecific,  // 清除特定类型棋子
    MoveCount,      // 限制移动次数
    ScoreTarget     // 达到目标分数
}
```

**主要方法**：
```csharp
void Initialize(LevelScene scene)
void StartTracking()
void StopTracking()
void OnPieceMoved(IPiece movedPiece, GridPosition from, GridPosition to, IPiece jumpedPiece)
bool IsGoalCompleted()
bool IsGoalFailed()
string GetGoalDescription()
```

**事件**：
```csharp
event Action OnGoalCompleted
event Action OnGoalFailed
event Action<int> OnScoreChanged
event Action<int> OnMoveCountChanged
```

---

### 5. LevelUIController（UI 控制器）

**路径**: `Assets/Scripts/HotUpdate/Level/Runtime/LevelUIController.cs`

**职责**：
- 管理关卡内的所有 UI 元素
- 更新分数、移动次数、目标文本
- 显示/隐藏各种面板（暂停、完成、失败）
- 处理按钮点击事件

**UI 元素**：
```csharp
TextMeshProUGUI levelNumberText
TextMeshProUGUI goalText
TextMeshProUGUI moveCountText
TextMeshProUGUI scoreText
Button pauseButton
Button restartButton
Button backButton
GameObject pausePanel
GameObject completePanel
GameObject failPanel
```

**主要方法**：
```csharp
void Initialize(LevelScene scene)
void OnLevelStart()
void OnLevelPause()
void OnLevelResume()
void OnLevelComplete()
void OnLevelFail()
```

---

### 6. BoardStateManager（棋盘状态管理器）

**路径**: `Assets/Scripts/HotUpdate/Level/Runtime/BoardStateManager.cs`

**职责**：
- 维护所有棋子的逻辑状态
- 实现 IBoardState 接口供移动验证
- 检查游戏结束条件

**主要方法**：
```csharp
void MovePiece(IPiece piece, GridPosition newPosition)
void RemovePiece(GridPosition position)
void ResetAllPieces()
bool HasMovablePieces()
IPiece GetPieceAt(GridPosition position)
bool HasPieceAt(GridPosition position)
```

---

## 三、数据流和事件流

### 1. 关卡启动流程

```
1. LevelStateMachine.Begin(config, levelNumber)
   ↓
2. StateLoadLevel.OnEnter()
   ↓
3. LevelScene.LoadLevel(config, levelNumber)
   ├─ Board.Build(config)
   ├─ LevelInputHandler.Initialize()
   ├─ LevelGoalManager.Initialize()
   └─ LevelUIController.Initialize()
   ↓
4. StateLevelReady.OnEnter()
   ↓
5. LevelScene.StartLevel()
   ├─ LevelInputHandler.EnableInput(true)
   ├─ LevelGoalManager.StartTracking()
   └─ LevelUIController.OnLevelStart()
   ↓
6. StateLevelRunning (游戏进行中)
```

### 2. 棋子移动流程

```
1. 玩家拖拽棋子
   ↓
2. LevelInputHandler 检测输入
   ├─ OnPointerDown → 选中棋子
   ├─ OnPointerDrag → 拖拽反馈
   └─ OnPointerUp → 检测目标位置
   ↓
3. 验证移动
   IPiece.ValidateMove(from, to, boardState)
   ↓
4. 执行移动
   ├─ Board.MovePiece(piece, to)
   ├─ Board.RemovePiece(jumpedPiece)
   └─ BoardStateManager 更新状态
   ↓
5. 触发事件
   LevelInputHandler.OnPieceMoved
   ↓
6. LevelScene.HandlePieceMoved
   ├─ LevelGoalManager.OnPieceMoved (更新目标)
   ├─ LevelUIController.OnPieceMoved (更新 UI)
   └─ CheckGameOver (检查结束)
   ↓
7. 可能触发关卡完成/失败
   LevelScene.CompleteLevel() / FailLevel()
```

### 3. 事件订阅关系

```
LevelScene 订阅:
  ├─ LevelInputHandler.OnPieceMoved
  ├─ LevelGoalManager.OnGoalCompleted
  └─ LevelGoalManager.OnGoalFailed

LevelUIController 订阅:
  ├─ LevelGoalManager.OnScoreChanged
  └─ LevelGoalManager.OnMoveCountChanged

LevelStateMachine 订阅:
  ├─ LevelScene.OnLevelCompleted
  └─ LevelScene.OnLevelFailed
```

---

## 四、Unity 场景配置

### LevelScene 预制体结构

```
LevelScene (GameObject)
├── LevelScene (Script)
│   ├─ board: Board 引用
│   ├─ inputHandler: LevelInputHandler 引用
│   ├─ goalManager: LevelGoalManager 引用
│   └─ uiController: LevelUIController 引用
│
├── BG (Image) - 背景
│
├── GameArea (Container)
│   ├── Board (GameObject)
│   │   ├─ Board (Script)
│   │   └─ MapGrid (模板，隐藏)
│   │
│   └── LevelInputHandler (GameObject)
│       └─ LevelInputHandler (Script)
│
├── UIArea (Container)
│   └── LevelUIController (GameObject)
│       ├─ LevelUIController (Script)
│       ├─ LevelNumberText
│       ├─ GoalText
│       ├─ MoveCountText
│       ├─ ScoreText
│       ├─ PauseButton
│       ├─ RestartButton
│       ├─ BackButton
│       ├─ PausePanel
│       ├─ CompletePanel
│       └─ FailPanel
│
└── LevelGoalManager (GameObject)
    └─ LevelGoalManager (Script)
```

### 组件引用配置

**LevelScene 组件**：
- Board: 拖拽 Board GameObject
- Input Handler: 拖拽 LevelInputHandler GameObject
- Goal Manager: 拖拽 LevelGoalManager GameObject
- UI Controller: 拖拽 LevelUIController GameObject

**LevelInputHandler 组件**：
- Drag Threshold: 10
- Enable Debug Log: false

**LevelGoalManager 组件**：
- Current Goal:
  - Goal Type: RemainOne
  - Target Count: 1
  - Target Piece Type: Peg

**LevelUIController 组件**：
- 所有 UI 元素引用

---

## 五、扩展指南

### 添加新的目标类型

1. 在 `LevelGoalType` 枚举中添加新类型
2. 在 `LevelGoalManager.IsGoalCompleted()` 中添加检测逻辑
3. 在 `LevelGoalManager.GetGoalDescription()` 中添加描述文本

### 添加新的输入方式

1. 继承 `LevelInputHandler` 或创建新的输入处理器
2. 实现相同的事件接口
3. 在 LevelScene 中替换引用

### 添加关卡特效

创建新的管理器：
```csharp
public class LevelEffectManager : MonoBehaviour
{
    public void Initialize(LevelScene scene) { }
    public void PlayMoveEffect(GridPosition from, GridPosition to) { }
    public void PlayRemoveEffect(GridPosition position) { }
}
```

### 添加音效系统

创建音效管理器：
```csharp
public class LevelAudioManager : MonoBehaviour
{
    public void Initialize(LevelScene scene) { }
    public void PlayMoveSound() { }
    public void PlayRemoveSound() { }
    public void PlayCompleteSound() { }
}
```

---

## 六、优势总结

### 1. 单一职责
每个类只负责一个特定功能，易于理解和维护

### 2. 低耦合
各组件通过事件通信，可独立测试和替换

### 3. 高内聚
相关功能集中在对应的管理器中

### 4. 易扩展
新增功能只需添加新的管理器，不影响现有代码

### 5. 易测试
每个组件可独立单元测试

### 6. 可复用
各个管理器可在不同类型的关卡中复用

---

## 七、与旧架构的对比

| 方面 | 旧架构 | 新架构 |
|------|--------|--------|
| **主控制器** | Board | LevelScene |
| **Board 职责** | 布局 + 游戏逻辑 + 输入 | 仅布局 |
| **输入处理** | PieceVisual (分散) | LevelInputHandler (集中) |
| **目标管理** | 无独立管理 | LevelGoalManager |
| **UI 管理** | 分散在各处 | LevelUIController |
| **可扩展性** | 较差 | 优秀 |
| **可维护性** | 较差 | 优秀 |

---

## 八、迁移指南

### 从旧架构迁移

1. **保持兼容性**
   - StateLoadLevel 支持检测 LevelScene
   - 如果找不到，回退到 Board（legacy mode）

2. **逐步迁移**
   - 可以先只使用 LevelScene + Board
   - 逐步添加其他管理器

3. **删除旧代码**
   - PieceVisual.cs 已被 LevelInputHandler 替代
   - Board 中的游戏逻辑已移至 LevelScene

---

## 九、常见问题

### Q: 为什么不在 Board 中处理输入？
A: Board 应该只负责布局，输入处理是独立的关注点，分离后更易维护和测试。

### Q: LevelScene 会不会变得太复杂？
A: 不会。LevelScene 只负责协调，具体逻辑都在各个管理器中。

### Q: 如何添加更多的游戏玩法？
A: 创建新的管理器，在 LevelScene 中引用并初始化即可。

### Q: 事件太多会不会影响性能？
A: 事件订阅/取消的性能开销可忽略，相比代码清晰度，这是值得的。

---

## 十、下一步

1. 在 Unity 中更新 LevelScene 预制体
2. 配置各个管理器的引用
3. 测试关卡加载和游戏流程
4. 根据需要添加音效、特效管理器

**系统已完全重构，职责清晰，易于扩展！** ✨
