# 关卡系统架构文档 v4.0

现行架构文档。相对 v3.0，本版本反映已经落地的能力模型、输入入口和胜负规则。

相关文档：

- 配置步骤：`CONFIGURATION_GUIDE_V2.md`
- 升级结论：`UPGRADE_COMPLETE_REPORT.md`
- v3.0 快照（LevelView 初版，输入与胜负已过时）：`ARCHITECTURE_V3.md`

---

## 一、设计原则

系统仍按 MVC 分层，但游戏规则不再写死在“棋子类型”或 `LevelGoalType` 上。

| 层次 | 职责 | 代表 |
|------|------|------|
| Model | 无 Unity 依赖的规则与状态 | `LevelController`、`BoardStateManager`、`IPiece`、`IMoveSkill` |
| View | 布局、选中反馈、UI | `Board`、`NormalPieceView`、`LevelUIController` |
| Coordinator | 生命周期与事件桥接 | `LevelView`、`LevelStateMachine` |

三条硬约束：

1. **棋子会做什么**由 `moveSkills` 决定，不由独立 `isMovable` 决定。
2. **棋子能被别人做什么**由 `PieceTrait` 决定（`canBeJumped`、`isRescueTarget`）。
3. **关卡何时结束**由 `LevelController.IsVictory / IsDefeat` 决定，不由 `LevelGoalManager` 决定。

---

## 二、组件关系

```
LevelStateMachine
  LobbyToLevel → LoadLevel → LevelReady → LevelRunning
                                    ↕ Pause
                           Success / Fail → LevelToLobby

LoadLevel 实例化预制体 LevelScene（YooAsset 地址）
  └── LevelView（必须存在）
        ├── LevelController（运行时 new）
        │     ├── BoardStateManager
        │     │     └── IPiece（NormalPiece / PieceBase）
        │     │           ├── IMoveSkill[]（JumpMoveSkill）
        │     │           └── PieceTrait
        │     └── LevelGoalManager（步数/分数，不裁决胜负）
        ├── Board
        │     ├── MapGrid × N
        │     └── NormalPieceView × M（当前全部走 pegPiecePrefab）
        ├── PieceMoveManager（正式输入）
        ├── LevelInputHandler（备用，默认关闭）
        └── LevelUIController（可选）
```

### 职责表

| 组件 | 类型 | 负责 | 不负责 |
|------|------|------|--------|
| `LevelView` | MonoBehaviour | 加载/开始/暂停、订阅移动结果、调用胜负检测 | MoveSkill 规则、格子生成细节 |
| `LevelController` | 纯 C# | `ExecuteMove`、`IsVictory`、`IsDefeat` | 视觉、输入 |
| `BoardStateManager` | 纯 C# | 位置索引、移除、解救目标计数、`HasMovablePieces` | UI |
| `Board` | MonoBehaviour | `BuildLayout`、`UpdatePieceVisual`、可走格特效 | 改棋盘逻辑状态 |
| `PieceMoveManager` | MonoBehaviour | 点击选棋、点击空格请求移动 | 规则验证 |
| `LevelInputHandler` | MonoBehaviour | 另一套点击输入（当前未启用） | 正式关卡流程 |
| `LevelGoalManager` | 纯 C# | 步数、分数、描述文本 | 最终胜利/失败 |
| `IMoveSkill` | 纯 C# | 单方向跨越一子是否合法 | 关卡结束 |
| `PieceFactory` | 静态工厂 | 从 `PieceData` 创建 `NormalPiece` | 视觉实例化 |

---

## 三、数据模型

### LevelConfig

```
LevelConfig
  schemaVersion = 1
  levelId                 // 必须等于 YooAsset 地址
  sceneType / difficulty
  board: BoardData        // width 4–7, height 4–9；格子 isActive
  pieces: List<PieceData>
```

校验：棋子 ID 唯一、落在激活格子、位置不重叠、至少一枚棋子 `HasMoveSkills`。

### PieceData

```csharp
public enum PieceType { Normal = 0 }

public enum MoveSkillType { JumpUp, JumpDown, JumpLeft, JumpRight }

public class PieceData
{
    public string id;
    public PieceType pieceType;                 // 当前仅 Normal
    public List<MoveSkillType> moveSkills;      // 空 = 不可移动
    public bool canBeJumped;                    // 默认 true
    public bool isRescueTarget;                 // 默认 false
    public GridPosition position;
}
```

编辑器新建 `Normal` 时默认写入四个 Jump 技能。

### 运行时棋子

```
PieceBase
  MoveSkills → IMoveSkill（JumpMoveSkill）
  Traits     → CanBeJumped / IsRescueTarget
  IsMovable  → moveSkills.Count > 0
  ValidateMove / GetValidMoves → 任一 Skill 匹配即合法
```

`JumpMoveSkill`：目标必须是该方向距离 2 的格子；中间必须有格子、有棋子、且 `CanBeJumped`；目标必须是空激活格。

---

## 四、组件说明

### 1. LevelView

路径：`Runtime/Core/LevelView.cs`

运行时创建 Controller，不把逻辑状态序列化到预制体。

```csharp
public void LoadLevel(LevelConfig config, int levelNumber)
{
    var goalManager = new LevelGoalManager(levelGoal);
    controller = new LevelController(config, goalManager);
    goalManager.Initialize(controller.BoardState);
    board.BuildLayout(config, controller.BoardState);
    InitializeManagers(); // PieceMoveManager.OnMoveExecuted += HandleMoveExecuted
}

private void HandleMoveExecuted(MoveExecutionResult result)
{
    board.UpdatePieceVisual(result);
    uiController?.OnPieceMoved();
    CheckGameOver();
}
```

`StartLevel` 会 Enable 输入并立刻 `CheckGameOver`（开局无合法步则失败）。

### 2. LevelController

路径：`Runtime/Core/LevelController.cs`

```csharp
ExecuteMove(piece, to)
  → piece.ValidateMove
  → MovePiece
  → RemovePiece(jumped)   // Rescue Target 则 RemainingRescueTargetCount--
  → goalManager.OnPieceMoved
  → MoveExecutionResult

IsVictory()
  HasRescueTargets → RemainingRescueTargetCount == 0
  else             → AllPieces.Count == 1

IsDefeat()
  !IsVictory() && !HasMovablePieces()
```

胜利优先于失败：最后一个解救目标被跳掉后即使无后续着法，仍胜利。

### 3. Board / BoardStateManager

`Board.BuildLayout(config, runtimeBoardState)` 只根据配置生成格子和视觉；`GetPiecePrefab` 当前始终返回 `pegPiecePrefab`。

`BoardStateManager` 初始化时统计 `InitialRescueTargetCount` / `RemainingRescueTargetCount`。`ResetAllPieces` 只复位仍在列表中的棋子位置，并重算剩余解救目标。

### 4. PieceMoveManager（正式输入）

路径：`Runtime/Input/PieceMoveManager.cs`

交互：

1. 点击可移动棋子 → 选中，Board 显示可走格
2. 再点同一棋子 → 取消
3. 再点其他可移动棋子 → 改选
4. 再点空格 → `ExecuteMove`
5. 点无效处 → 取消选中

`LevelView` 订阅 `OnMoveExecuted`，不走 `HandleMoveRequested`。

`LevelInputHandler` 仍实现点击选中/点空格移动，但 `LevelView.SetInputEnabled` 只开关 `PieceMoveManager`。

### 5. 状态机

```
LobbyToLevel → LoadLevel → LevelReady → LevelRunning ↔ LevelPause
                                      → LevelSuccess / LevelFail
                                      → LevelToLobby
```

`StateLoadLevel`：YooAsset 加载 `LevelScene` → 挂到 `UIRoot.Level` → 必须拿到 `LevelView`，否则失败。无 LevelScene 回退。

`StateLevelReady`：`LevelView.StartLevel()` → `LevelRunning`。

---

## 五、数据流

### 启动

```
LevelStateMachine.Begin(config, n)
  StateLoadLevel
    Instantiate LevelScene
    LevelView.LoadLevel
      new LevelController + BoardStateManager
      Board.BuildLayout
      PieceMoveManager.Initialize(controller, board)
  StateLevelReady
    LevelView.StartLevel → EnableInput + CheckGameOver
  StateLevelRunning
```

### 一次合法跳跃

```
点击空目标格
  PieceMoveManager.TryMovePiece
    LevelController.ExecuteMove
      JumpMoveSkill.ValidateMove
      更新 BoardStateManager
    OnMoveExecuted
      Board.UpdatePieceVisual（换父节点 + 销毁被跳棋子）
      CheckGameOver
        CompleteLevel → OnLevelCompleted → LevelSuccess
        或 FailLevel → OnLevelFailed → LevelFail
```

---

## 六、与 v3.0 的差异

| 项 | v3.0 文档 | v4.0 代码 |
|----|-----------|-----------|
| 正式输入 | `LevelInputHandler` 拖拽/`OnMoveRequested` | `PieceMoveManager` 点击选中 + 点击落子 |
| 可移动 | 未强调 / 旧 `isMovable` | `moveSkills.Count > 0` |
| 棋子类型 | Peg / Gem / Stone | 仅 `Normal` |
| 移动规则 | 写在棋子/通用规则里 | 原子 `IMoveSkill` 组合 |
| 胜利 | `LevelGoalType.RemainOne` 等 | 解救目标清零；无目标时剩 1 子 |
| 失败 | 目标失败或无移动混用 | 未胜利且无合法跳跃 |
| 移动结果类型 | 文档中的 `MoveResult` | 验证用 `MoveResult`，执行用 `MoveExecutionResult` |
| 旧入口 | 可回退 LevelScene | 必须 LevelView |
| 编辑器自动配置菜单 | 规划中 | 不存在；用 `Tools/Level Editor` |

---

## 七、预制体与编辑器

### LevelScene 预制体

```
LevelScene
├── LevelView
│     board / pieceMoveManager / uiController（可选）
├── Board
│     MapGrid 模板
│     pegPiecePrefab ← 当前唯一使用的棋子视觉
└── PieceMoveManager
      board 指向同一 Board
```

需要 EventSystem。正式流程由状态机挂到 `UIRoot.Level`。

### 关卡编辑器

菜单：`Tools/Level Editor`

- 灰格 = 编辑空位，绿格 = 棋盘格
- Add Cell / Delete Cell / Add Piece
- Type、Can Be Jumped、Rescue Target、Move Skills
- 保存 `Assets/Game/Config/Level/{levelId}.json`

扩展新类型时同时改三处：`PieceType`、`BoardEditorView.GetMoveSkillsForType`、`Board.GetPiecePrefab`。

---

## 八、扩展点

| 需求 | 改哪里 |
|------|--------|
| 新方向/斜跳/多格跳 | 新 `IMoveSkill` + `MoveSkillType` + Factory |
| 新棋子种类默认技能 | `PieceType` + `GetMoveSkillsForType` |
| 不能被跳 / 解救目标 | `PieceTrait`，不必新类型 |
| 新胜利条件 | `LevelController.IsVictory`，不要塞进 GoalManager |
| 新输入手势 | 新 Manager，仍只调用 `ExecuteMove` |
| 视觉差异 | `Board.GetPiecePrefab` 与 PieceView |

保持 `LevelController` 可单测：构造 `LevelConfig` → `ExecuteMove` → 断言 `IsVictory` / `IsDefeat`。

---

## 九、FAQ

**Q: 为什么 GoalManager 还在？**  
A: 仍用于步数、分数和 UI 文案。最终结束条件已收到 `IsVictory/IsDefeat`，避免 RemainOne 与解救目标打架。

**Q: 为什么还有 Peg 预制体字段？**  
A: 序列化兼容。运行时只读 `pegPiecePrefab`。后续可改名。

**Q: 为什么同时存在 PieceMoveManager 和 LevelInputHandler？**  
A: 后者是点击输入原型。现行绑定是前者；后者默认不 Enable。

**Q: 开局为什么直接失败？**  
A: `StartLevel` 会检测。没有任何带合法 Jump 的棋子就会失败。

---

## 十、文件索引

```
Runtime/Core/          LevelView, LevelController, MoveExecutionResult
Runtime/Board/         Board, BoardStateManager, MapGrid
Runtime/Input/         PieceMoveManager, LevelInputHandler
Runtime/Piece/         IPiece, PieceBase, NormalPiece, MoveSkills, PieceTrait
Runtime/Goals/         LevelGoalManager
Runtime/UI/            LevelUIController
Data/                  LevelConfig, PieceData, BoardData
Editor/                LevelEditorWindow, BoardEditorView, ToolPanelView
State/                 LevelStateMachine, StateLoadLevel, StateLevelReady
```

**v4.0 核心思想**：分层仍然是 MVC；规则从“类型枚举 + 目标枚举”改成“MoveSkill 组合 + Trait + 解救目标计数”。
