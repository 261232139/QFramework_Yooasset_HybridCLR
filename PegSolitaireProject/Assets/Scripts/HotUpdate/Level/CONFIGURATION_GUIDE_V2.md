# 关卡系统配置指南

本文档对应当前运行时与编辑器实现：关卡入口是 `LevelView`，移动输入是 `PieceMoveManager`，棋子能力由 `MoveSkillType` 组合决定，胜负由解救目标与可走步检测完成。

---

## 一、当前架构入口

```
LevelStateMachine
  → StateLoadLevel 加载 LevelScene 预制体
  → LevelView.LoadLevel / StartLevel
      ├── LevelController（纯逻辑）
      │     ├── BoardStateManager
      │     └── LevelGoalManager
      ├── Board（纯视图）
      ├── PieceMoveManager（拖拽移动）
      └── LevelUIController（可选）
```

| 组件 | 类型 | 职责 |
|------|------|------|
| `LevelView` | MonoBehaviour | 关卡生命周期与组件协调 |
| `LevelController` | 纯 C# | 验证/执行移动、胜负判定 |
| `Board` | MonoBehaviour | 格子与棋子视觉布局 |
| `BoardStateManager` | 纯 C# | 棋子位置、解救目标计数 |
| `PieceMoveManager` | MonoBehaviour | 正式拖拽输入入口 |
| `LevelGoalManager` | 纯 C# | 步数/分数跟踪（不决定最终胜负） |
| `LevelUIController` | MonoBehaviour | 关卡内 UI（可选） |
| `LevelInputHandler` | MonoBehaviour | 点击选中/点击移动（备用，当前默认关闭） |

---

## 二、脚本清单

### 运行时核心

| 脚本 | 路径 |
|------|------|
| `LevelView.cs` | `Runtime/Core/LevelView.cs` |
| `LevelController.cs` | `Runtime/Core/LevelController.cs` |
| `Board.cs` | `Runtime/Board/Board.cs` |
| `BoardStateManager.cs` | `Runtime/Board/BoardStateManager.cs` |
| `PieceMoveManager.cs` | `Runtime/Input/PieceMoveManager.cs` |
| `LevelGoalManager.cs` | `Runtime/Goals/LevelGoalManager.cs` |
| `LevelUIController.cs` | `Runtime/UI/LevelUIController.cs` |
| `LevelConfig.cs` / `PieceData.cs` / `BoardData.cs` | `Data/` |

### 编辑器

| 脚本 | 用途 |
|------|------|
| `LevelEditorWindow.cs` | `Tools/Level Editor` |
| `BoardEditorView.cs` | 棋盘格子与棋子摆放 |
| `ToolPanelView.cs` | 棋子类型、MoveSkill、解救目标 |
| `SerializationManager.cs` | 保存到 `Assets/Game/Config/Level/` |

### 不再作为入口使用

- `LevelScene` 脚本：已被 `LevelView` 替换。预制体名仍可叫 `LevelScene`，但根节点必须挂 `LevelView`。
- `PieceVisual`：输入已集中到 `PieceMoveManager`。
- `PieceType.Peg / Gem / Stone`：已删除，当前只保留 `PieceType.Normal`。

---

## 三、LevelScene 预制体配置

### 步骤 1：根对象挂 LevelView

1. 打开 `LevelScene.prefab`（YooAsset 地址仍为 `LevelScene`）。
2. 根 GameObject 添加 `LevelView`，不要再挂旧的 `LevelScene` 脚本。
3. `StateLoadLevel` 会实例化该预制体到 `UIRoot.Level`，并调用 `LevelView.LoadLevel`。

### 步骤 2：推荐层级

```
LevelScene
├── BG
├── GameArea
│   ├── Board
│   │   ├── Board
│   │   └── MapGrid（模板，默认隐藏）
│   └── PieceMoveManager
└── UIArea（可选）
    └── LevelUIController
```

`LevelGoalManager` 是纯 C# 对象，由 `LevelView` 在 `LoadLevel` 时创建，**不要**在预制体上挂 GoalManager 组件。

### 步骤 3：配置 LevelView 引用

在根对象 Inspector：

```
LevelView
  Board: GameArea/Board
  Piece Move Manager: GameArea/PieceMoveManager
  Input Handler: 可留空（当前默认关闭点击输入）
  UI Controller: UIArea/LevelUIController（可选）
  Level Goal: 仅用于分数/步数展示，不决定最终胜利
```

### 步骤 4：配置 Board

```
Board
  Map Grid Template: 子物体 MapGrid
  Cell Size: (150, 150)
  Cell Spacing: (10, 10)
  Piece Prefabs
    Peg Piece Prefab: Normal 棋子视觉预制体（当前所有类型都走这个引用）
    Gem / Stone Piece Prefab: 保留字段，当前不使用
```

当前 `GetPiecePrefab()` 始终返回 `pegPiecePrefab`，因此必须正确绑定该字段。

### 步骤 5：配置 PieceMoveManager

```
PieceMoveManager
  Board: 同一 Board
  Drag Threshold: 10
  Show Debug Info: 需要时开启
```

`LevelView` 会在加载关卡时再次 `Initialize(controller, board)`，并在开始/暂停时开关输入。

### 步骤 6：可选 UI

在 `LevelUIController` 下绑定：

- 文本：LevelNumber、Goal、MoveCount、Score
- 按钮：Pause、Restart、Back
- 面板：PausePanel、CompletePanel、FailPanel（默认隐藏）

未绑定 UI 时，关卡仍可完整游玩。

---

## 四、关卡数据配置

### 1. 使用关卡编辑器

菜单：`Tools/Level Editor`

| 区域 | 配置项 |
|------|--------|
| 左侧 Config | `levelId`、`sceneType`、`difficulty`、棋盘宽高（4–7 × 4–9） |
| 中间棋盘 | 灰格=仅编辑空位，绿格=棋盘格子；左键选中 |
| 右侧 Tools | Type、Can Be Jumped、Rescue Target、Move Skills |

编辑棋盘：

1. 选中空白格 → **Add Cell**
2. 选中格子 → **Delete Cell**（同时删除该格棋子）
3. 选中格子 → **Add Piece**
4. 选中棋子后可改 Type / Can Be Jumped / Rescue Target / Move Skills

保存路径：`Assets/Game/Config/Level/{levelId}.json`  
运行时通过 YooAsset 以 `levelId` 加载同名 `TextAsset`。

### 2. PieceType 与 MoveSkill

当前 `PieceType` 只有：

```csharp
public enum PieceType
{
    Normal = 0
}
```

新建 `Normal` 棋子默认技能：

```text
JumpUp, JumpDown, JumpLeft, JumpRight
```

规则：

- `moveSkills.Count > 0` → 可移动
- `moveSkills` 为空 → 固定棋子，不能被玩家拖动
- `canBeJumped` → 能否被跨越并移除
- `isRescueTarget` → 解救目标

### 3. PieceData 字段

```csharp
public class PieceData
{
    public string id;
    public PieceType pieceType;          // 当前仅 Normal
    public List<MoveSkillType> moveSkills;
    public bool canBeJumped;             // 默认 true
    public bool isRescueTarget;          // 默认 false
    public GridPosition position;
}
```

关卡校验要求：

- `schemaVersion == 1`
- `levelId` 非空，且与 YooAsset 地址一致
- 棋盘至少一格
- 棋子 ID 唯一、位于激活格子、位置不重叠
- 至少一枚棋子有 MoveSkill

---

## 五、胜负规则（必须按此配置关卡）

每次成功移动后，`LevelView` 会调用 `LevelController.CheckGameOver()`。

**胜利（优先）**

1. 关卡中存在至少一枚 `isRescueTarget`：全部解救目标被跨越并移除后胜利。
2. 关卡中没有任何解救目标：棋盘只剩 1 枚棋子时胜利。

**失败**

- 当前未胜利
- 且棋盘上不存在任何合法跳跃

注意：

- 最后一个解救目标被移除时，即使之后无合法移动，也判定胜利。
- 关卡开始时也会检测一次；若开局无合法移动且未胜利，会立刻失败。
- `LevelGoalManager` 的 `RemainOne / ClearAll` 等类型只用于描述和计分，**最终胜负不走该管理器**。

推荐配置：

- 至少放置 1 枚 Rescue Target
- 至少 1 枚带四方向 Jump 的可移动棋子
- 解救目标通常 `canBeJumped = true`

---

## 六、运行时流程

### 加载

```
LevelStateMachine.Begin(config, levelNumber)
  → StateLoadLevel 实例化 LevelScene
  → LevelView.LoadLevel(config, levelNumber)
      创建 LevelController / BoardStateManager / LevelGoalManager
      Board.BuildLayout
      PieceMoveManager.Initialize
  → StateLevelReady → LevelView.StartLevel()
  → StateLevelRunning
```

### 移动

```
PieceMoveManager 拖拽松手
  → LevelController.ExecuteMove
      验证 MoveSkill
      移动棋子
      移除被跨越棋子（若为 Rescue Target 则计数 -1）
  → Board.UpdatePieceVisual
  → CheckGameOver
      胜利 → LevelView.CompleteLevel → OnLevelCompleted → StateLevelSuccess
      失败 → LevelView.FailLevel → OnLevelFailed → StateLevelFail
```

---

## 七、最简测试配置

不需要 UI 时：

```
LevelScene
├── Board（MapGrid 模板 + pegPiecePrefab）
└── PieceMoveManager
```

`LevelView`：

- Board：已绑定
- Piece Move Manager：已绑定
- UI Controller：空
- Input Handler：空

场景要求：

- 存在 `EventSystem`
- `UIRoot.Level` 可用（正式流程由状态机挂载）
- 关卡 JSON 已加入 YooAsset，地址等于 `levelId`

代码测试：

```csharp
var levelView = FindFirstObjectByType<LevelView>();
levelView.LoadLevel(config, 1);
levelView.StartLevel();
```

不要再调用 `FindFirstObjectByType<LevelScene>()`。

---

## 八、常见配置错误

### 1. Missing Script / 找不到 LevelView

根预制体仍挂着已删除的 `LevelScene`。改为挂 `LevelView`。

`StateLoadLevel` 找不到 `LevelView` 会直接失败。

### 2. 无法拖动棋子

检查：

- 场景有 EventSystem
- `PieceMoveManager` 已绑定 Board
- 棋子 `moveSkills` 不为空
- 关卡已 `StartLevel`（输入才会 Enable）
- Console 是否有 `[PieceMoveManager] Invalid move`

### 3. 棋子看不见

`Board.pegPiecePrefab` 未绑定。当前所有 `Normal` 棋子都使用该预制体。

### 4. 关卡无法胜利

- 未勾选任何 `isRescueTarget`，系统会走“只剩 1 子”规则
- 勾了 Rescue Target，但目标 `canBeJumped = false`，无法被解救
- 期望靠旧的 `LevelGoalType.RemainOne` 获胜：该目标不再决定胜负

### 5. 开局立刻失败

棋盘上没有任何带合法跳跃的棋子。补一枚四方向 Jump 的 `Normal` 棋子。

### 6. 保存失败

编辑器校验报错时不能保存。常见原因：没有可移动棋子、棋子不在格子上、重复 ID。

---

## 九、调试日志

开启 `PieceMoveManager.Show Debug Info` 后可见：

```
[LevelView] Level 1 loaded: level_001
[Board] Layout built: 5x6, 12 pieces
[PieceMoveManager] Started dragging piece normal_000
[LevelController] Move executed: normal_000 from (0, 0) to (2, 0)
```

胜利/失败由 `LevelView.CompleteLevel / FailLevel` 触发，随后状态机进入 `LevelSuccess / LevelFail`。

---

## 十、配置检查清单

- [ ] `LevelScene` 根节点是 `LevelView`，不是旧 `LevelScene` 脚本
- [ ] `Board` 绑定 MapGrid 模板和 `pegPiecePrefab`
- [ ] `PieceMoveManager` 绑定同一 Board
- [ ] 关卡 JSON 位于 `Assets/Game/Config/Level/`，YooAsset 地址 = `levelId`
- [ ] 至少一枚棋子有 MoveSkill
- [ ] 计划用解救胜利时，至少一枚 `isRescueTarget` 且 `canBeJumped`
- [ ] 场景有 EventSystem
- [ ] 拖拽后棋子位移正确，被跳棋子消失
- [ ] 全部 Rescue Target 被移除后进入胜利
- [ ] 无合法移动且未胜利时进入失败

---

## 十一、扩展提示

后续加新棋子类型时：

1. 在 `PieceType` 增加枚举值
2. 在 `BoardEditorView.GetMoveSkillsForType` 指定默认技能
3. 在 `Board.GetPiecePrefab` 绑定对应视觉预制体

当前不要依赖 `Peg / Gem / Stone` 或独立的 `isMovable` 字段；可移动完全由 `moveSkills` 决定。
