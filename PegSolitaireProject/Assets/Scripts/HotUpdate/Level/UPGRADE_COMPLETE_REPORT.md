# 关卡系统升级完成报告

本文档记录从 `LevelScene` 架构演进到当前系统后的**实际完成状态**，以代码为准，不以旧规划文档为准。

---

## 一、当前完成状态

当前关卡入口、棋子能力和胜负规则已经落地，不再依赖旧的 `LevelScene` 脚本、独立 `isMovable` 字段，或 Peg / Gem / Stone 三种棋子类型。

| 模块 | 状态 |
|------|------|
| LevelView MVC 入口 | 已完成 |
| LevelController 纯逻辑移动/胜负 | 已完成 |
| Board 纯视图 | 已完成 |
| PieceMoveManager 拖拽输入 | 已完成（正式入口） |
| MoveSkill 组合移动 | 已完成 |
| PieceType 仅 Normal | 已完成 |
| isRescueTarget 解救胜利 | 已完成 |
| 关卡编辑器 Tools/Level Editor | 已完成 |
| 旧 LevelScene 脚本 | 已删除 |
| StateLoadLevel 回退旧架构 | 已取消，找不到 LevelView 即失败 |

---

## 二、已完成的升级内容

### 1. LevelScene → LevelView

- 删除 `LevelScene.cs`，避免与新入口冲突。
- 预制体名仍可为 `LevelScene`（YooAsset 地址 `LevelScene`），根节点必须挂 `LevelView`。
- `StateLoadLevel` 只认 `LevelView`：找不到组件会销毁实例并进入失败。
- `StateLevelReady` 调用 `LevelView.StartLevel()` 后进入 `LevelRunning`。
- `LevelView.OnLevelCompleted / OnLevelFailed` 驱动状态机进入 `LevelSuccess / LevelFail`。

### 2. MVC 分层

```
LevelView（协调，MonoBehaviour）
    ├── LevelController（纯逻辑）
    │     ├── BoardStateManager
    │     └── LevelGoalManager
    └── 视图
          ├── Board
          ├── PieceMoveManager
          └── LevelUIController（可选）
```

| 组件 | 当前职责 |
|------|----------|
| `LevelView` | 生命周期、绑定事件、移动后检查胜负 |
| `LevelController` | 验证/执行移动，判定胜利失败 |
| `Board` | 只生成格子/棋子视觉，被动 `UpdatePieceVisual` |
| `BoardStateManager` | 位置、移除、解救目标计数、是否有合法移动 |
| `PieceMoveManager` | 正式拖拽输入，调用 `ExecuteMove` |
| `LevelInputHandler` | 备用点击输入，当前默认不启用 |
| `LevelGoalManager` | 步数/分数跟踪，**不决定最终胜负** |

### 3. 棋子能力：MoveSkill

- 删除独立 `isMovable`。可移动完全由 `piece.moveSkills.Count > 0` 决定。
- 当前 `PieceType` 只保留 `Normal`。
- `Normal` 默认技能：`JumpUp / JumpDown / JumpLeft / JumpRight`。
- 编辑器按类型赋默认技能；选中棋子后仍可单独开关技能。
- 工厂统一创建 `NormalPiece`。
- `Board.GetPiecePrefab()` 统一使用 `pegPiecePrefab`。

### 4. 胜负规则

每次成功移动（以及关卡刚开始）后，`LevelView` 调用 `LevelController`：

**胜利（优先）**

1. 关卡存在解救目标：`RemainingRescueTargetCount == 0`
2. 关卡没有解救目标：棋盘只剩 1 枚棋子

**失败**

- 当前未胜利
- 且 `HasMovablePieces() == false`

最后一个解救目标被移除时，即使之后无合法移动，也判定胜利。

`LevelGoal` 的 RemainOne / ClearAll 等类型仍可用于 UI 文案和计分，但不再作为关卡结束条件。

### 5. 数据与编辑器

- `PieceData`：`pieceType`、`moveSkills`、`canBeJumped`、`isRescueTarget`、`position`
- 关卡 JSON：`Assets/Game/Config/Level/{levelId}.json`
- 运行时：`LevelConfigLoader` 以 YooAsset 地址 `levelId` 加载并校验
- 编辑器：`Tools/Level Editor`（格子、棋子、MoveSkill、Rescue Target、校验后保存）

---

## 三、当前数据流

### 加载

```
LevelStateMachine.Begin(config, levelNumber)
  → StateLoadLevel 实例化 LevelScene 预制体到 UIRoot.Level
  → LevelView.LoadLevel
      创建 LevelController / BoardStateManager / LevelGoalManager
      Board.BuildLayout
      PieceMoveManager.Initialize
  → StateLevelReady → LevelView.StartLevel → CheckGameOver
  → StateLevelRunning
```

### 移动与结束

```
PieceMoveManager 拖拽松手
  → LevelController.ExecuteMove
      ValidateMove（MoveSkill）
      MovePiece
      RemovePiece（若是 Rescue Target，计数 -1）
      GoalManager.OnPieceMoved（步数/分数）
  → Board.UpdatePieceVisual
  → LevelView.CheckGameOver
      胜利 → CompleteLevel → OnLevelCompleted → LevelSuccess
      失败 → FailLevel → OnLevelFailed → LevelFail
```

这与旧报告中的 `LevelInputHandler.OnMoveRequested → HandleMoveRequested` **不一致**。当前正式链路是 `PieceMoveManager → ExecuteMove`。

---

## 四、架构对比（以当前代码为准）

| 方面 | 旧 LevelScene | 当前系统 |
|------|---------------|----------|
| 入口脚本 | LevelScene | LevelView |
| 移动输入 | PieceVisual / LevelInputHandler | PieceMoveManager |
| Board | 布局 + 状态 | 仅布局 |
| 可移动标记 | isMovable | moveSkills |
| 棋子类型 | Peg / Gem / Stone | Normal |
| 胜利条件 | LevelGoal（如 RemainOne） | 解救目标全部移除；无目标时剩 1 子 |
| 失败条件 | 目标失败或无移动 | 未胜利且无合法跳跃 |
| 旧入口兼容 | StateLoadLevel 可回退 | 必须有 LevelView |

---

## 五、关键文件

```
Assets/Scripts/HotUpdate/Level/
├── Runtime/
│   ├── Core/LevelView.cs
│   ├── Core/LevelController.cs
│   ├── Core/MoveExecutionResult.cs
│   ├── Board/Board.cs
│   ├── Board/BoardStateManager.cs
│   ├── Input/PieceMoveManager.cs
│   ├── Input/LevelInputHandler.cs
│   ├── Goals/LevelGoalManager.cs
│   ├── Piece/PieceBase.cs, NormalPiece.cs, MoveSkills.cs
│   └── UI/LevelUIController.cs
├── Data/LevelConfig.cs, PieceData.cs, BoardData.cs
├── Editor/LevelEditorWindow.cs, BoardEditorView.cs, ToolPanelView.cs
├── State/StateLoadLevel.cs, StateLevelReady.cs, LevelStateMachine.cs
└── CONFIGURATION_GUIDE_V2.md
```

已删除或不再作为入口：

- `LevelScene.cs`
- `PieceVisual.cs`
- `PieceType.Peg / Gem / Stone`
- `PieceData.isMovable`

当前仓库中**没有**这些旧报告提到的工具：

- `LevelViewAutoSetup.cs`
- `LevelScenePrefabConfigurator.cs`
- `LevelFlowTester.cs`

不要再按 `.bak` 恢复它们来配置预制体。预制体应直接挂 `LevelView` + `Board` + `PieceMoveManager`。详见 `CONFIGURATION_GUIDE_V2.md`。

---

## 六、验收要点

| 检查项 | 预期 |
|--------|------|
| 预制体根组件 | `LevelView`，不是 `LevelScene` |
| Console | `[StateLoadLevel] Using LevelView (new architecture)` |
| Board | `pegPiecePrefab` 已绑定，Normal 棋子可见 |
| 拖拽 | 有 MoveSkill 的棋子可拖；空技能棋子不可选 |
| 跳跃 | 被跨越且 `canBeJumped` 的棋子被移除 |
| 解救胜利 | 全部 `isRescueTarget` 被移除后进入 `LevelSuccess` |
| 无目标胜利 | 无 Rescue Target 时剩 1 子胜利 |
| 失败 | 未胜利且无合法移动进入 `LevelFail` |
| 开局无步 | `StartLevel` 时立即失败 |
| 编辑器 | 能保存带 MoveSkill / Rescue Target 的 JSON |

---

## 七、已知限制与后续

已接受的限制：

- 视觉预制体字段仍叫 Peg / Gem / Stone，运行时只使用 Peg 槽位。
- `LevelInputHandler` 仍存在，但 `LevelView` 默认关闭点击输入。
- `LevelGoalManager` 与最终胜负解耦，UI 目标文案可能与真实胜利条件不一致。
- `ARCHITECTURE_V2.md` / `ARCHITECTURE_V3.md` / `COMPLETION_CHECKLIST.md` 仍含旧流程，配置以 `CONFIGURATION_GUIDE_V2.md` 和本报告为准。

后续可选：

1. 新棋子类型：扩展 `PieceType` + `GetMoveSkillsForType` + `GetPiecePrefab`
2. Board 视觉字段改名为 Normal 预制体
3. UI 目标文案改为“解救全部目标”
4. 同步或归档过时的 ARCHITECTURE / CHECKLIST 文档
5. 为 `LevelController.IsVictory / IsDefeat` 补纯逻辑单测

---

## 八、结论

LevelView 分层、MoveSkill 移动、解救目标胜利、编辑器配置链路已经形成当前可用系统。

不要再按以下旧结论操作：

- 主输入是 `LevelInputHandler`
- 胜利由 `LevelGoalType.RemainOne` 决定
- 棋子类型是 Peg / Gem / Stone
- 可用自动配置菜单 `Tools → Level → Auto Setup LevelView Architecture`
- `StateLoadLevel` 会回退到 `LevelScene`

当前开发入口：

1. 运行时：`LevelView` + `PieceMoveManager` + `LevelController`
2. 配置：`Tools/Level Editor` + `CONFIGURATION_GUIDE_V2.md`

*架构版本: LevelView + MoveSkill + Rescue Victory*
