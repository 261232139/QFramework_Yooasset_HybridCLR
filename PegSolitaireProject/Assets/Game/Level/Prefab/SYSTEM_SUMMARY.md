# 关卡系统摘要（当前实现）

本文档描述当前 `LevelView` 架构。早期以 `LevelScene` 或 `Board` 直接承担关卡流程的文档已不适用。

## 1. 入口与运行流程

项目只在构建设置中启用 `Assets/Scenes/Boot.unity`。游戏从大厅进入关卡时，流程如下：

```text
LobbyController.EnterLevel(levelNumber)
  -> LevelConfigLoader.LoadAsync("level_XXX")
  -> LevelStateMachine.Begin(config, levelNumber)
  -> LobbyToLevel -> LoadLevel -> LevelReady -> LevelRunning
  -> YooAsset 加载并实例化 "LevelScene" 到 UIRoot.Level
  -> LevelView.LoadLevel(config, levelNumber)
  -> LevelView.StartLevel()
```

`StateLoadLevel` 只支持异步加载；找不到 YooAsset 包、`UIRoot.Level`、`LevelScene` 资源或根节点上的 `LevelView` 时，流程会进入失败状态。

## 2. LevelScene 预制体

**路径**：`Assets/Game/Level/Prefab/LevelScene.prefab`
**YooAsset 地址**：`LevelScene`

`LevelScene` 是运行时动态实例化的 UI 层级容器，而非独立 Unity 场景。它的根节点挂载以下组件：

- `LevelView`：关卡协调器，管理加载、开局、暂停、重置、胜负和组件通信。
- `LevelInputHandler`：处理鼠标点击选中棋子与点击落点。
- `LevelUIController`：更新关卡信息、目标、步数、分数与暂停/成功/失败面板。

其子层级包含：

```text
LevelScene
├── BG                         # 背景
├── Left
│   └── Board                  # Board 预制体实例
│       └── MapGrid × N        # 运行时生成的有效格子
│           └── PieceView × M  # 运行时生成的棋子表现
└── Right                      # 关卡 UI 区域
```

`Board` 专注于生成与更新表现，不负责游戏规则或关卡生命周期。

## 3. 核心组件与职责

| 组件 | 职责 |
|---|---|
| `LobbyController` | 读取玩家当前关卡，加载配置，并启动状态机。 |
| `LevelStateMachine` | 管理大厅/关卡的 8 个生命周期状态。 |
| `LevelConfigLoader` | 通过 YooAsset 加载 `TextAsset`，反序列化、校验并缓存配置。 |
| `LevelView` | 创建 `LevelController`，连接棋盘、输入、目标和 UI，并判定胜负。 |
| `LevelController` | 执行合法移动，维护逻辑棋盘，通知目标系统。 |
| `BoardStateManager` | 保存运行时棋子位置，提供占用与可移动性查询。 |
| `Board` / `MapGrid` | 根据配置创建棋盘格、棋子预制体和可移动提示，并同步移动表现。 |
| `LevelInputHandler` | 使用 UI Raycast 把点击位置映射为 `MapGrid`，发送移动请求。 |
| `LevelGoalManager` | 跟踪步数、分数与可选目标。 |
| `LevelUIController` | 响应关卡生命周期与目标事件，刷新 UI。 |

## 4. 规则与交互

当前交互是“点击棋子，再点击空格”。仅 `isMovable: true` 的棋子可被选中。

基础移动规则由 `PieceBase` 验证：

- 只能水平或垂直移动；
- 必须跨过恰好一枚棋子；
- 落点必须是有效且为空的格子；
- 被跨越的棋子会被移除。

棋盘最终仅剩一枚棋子时胜利；不存在合法跳跃且尚未胜利时失败。

## 5. 关卡配置

**目录**：`Assets/Game/Config/Level/`
**当前文件**：`level_001.json`、`level_002.json`、`level_003.json`

大厅将数字关卡编号格式化为 `level_{编号:000}`，例如第 1 关对应 `level_001`。加载器要求 YooAsset 地址与配置内的 `levelId` 完全一致。

配置模式：

```json
{
  "schemaVersion": 1,
  "levelId": "level_001",
  "sceneType": 0,
  "difficulty": 1,
  "board": { "width": 5, "height": 5, "rows": [] },
  "pieces": [
    { "id": "peg_001", "pieceType": 0, "isMovable": true,
      "position": { "x": 0, "y": 0 } }
  ]
}
```

保存/加载前会校验 schema、关卡 ID、棋盘尺寸和行列数、有效格、棋子 ID 唯一性、位置合法性与至少一枚可移动棋子。

## 6. 编辑与验证

- 打开编辑器：`Tools > Level Editor`
- 关卡配置由 `SerializationManager` 写入 `Assets/Game/Config/Level/`。
- `ValidationSystem` 和 `LevelConfig.Validate` 用于编辑时与运行时校验。
- `Tools > Batch Level Generator` 用于批量生成；AI/随机生成器位于 `Assets/Scripts/HotUpdate/Level/Editor/`。

## 7. 当前限制

- 过场、倒计时、结算奖励以及部分状态机的运行期逻辑仍保留 TODO。
- 成功或失败状态目前等待 3 秒后自动回大厅。
- 当前 3 个 JSON 的棋盘和棋子布局相同，仅关卡 ID 不同；新增关卡时需提供不同的配置，并确保资源地址已纳入 YooAsset。
- 关卡完成时会通过 `PlayerDataController.UnlockNextLevel()` 递增玩家的当前关卡编号。

## 8. 主要源码索引

- `Assets/Scripts/HotUpdate/UI/LobbyController.cs`
- `Assets/Scripts/HotUpdate/Level/State/LevelStateMachine.cs`
- `Assets/Scripts/HotUpdate/Level/State/StateLoadLevel.cs`
- `Assets/Scripts/HotUpdate/Level/Runtime/LevelView.cs`
- `Assets/Scripts/HotUpdate/Level/Runtime/Board.cs`
- `Assets/Scripts/HotUpdate/Level/Runtime/LevelConfigLoader.cs`
- `Assets/Scripts/HotUpdate/Level/Data/LevelConfig.cs`
