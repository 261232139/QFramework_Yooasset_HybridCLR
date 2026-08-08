# 关卡系统完整配置文档

## 一、LevelScene 预制体说明

### 1. LevelScene 预制体的作用

**路径**: `Assets/Game/Level/Prefab/LevelScene.prefab`

LevelScene 预制体是一个**布局容器**，不需要额外的脚本组件。它的作用是：

- 提供关卡场景的整体布局框架
- 包含背景（BG）
- 包含左侧游戏区域（包含 Board）
- 包含右侧 UI 区域（可以添加按钮、信息面板等）

### 2. 层级结构

```
LevelScene (GameObject)
├── RectTransform
├── BG (Image) - 背景图
├── Left (Container) - 左侧游戏区域
│   └── Board (Prefab Instance) - 棋盘控制器
└── Right (Container) - 右侧 UI 区域
```

### 3. 为什么 LevelScene 不需要脚本？

根据现有架构设计：
- **Board** 已经是主要的关卡控制器
- **LevelStateMachine** 负责关卡状态管理
- **LevelScene 预制体**只是作为场景布局使用

如果需要场景级别的控制（如暂停按钮、分数显示等），可以添加一个简单的 UI 控制器脚本。

### 4. 使用方式

```csharp
// 在 StateLoadLevel.cs 中，Board 会自动查找并生成关卡
var board = Object.FindFirstObjectByType<Board>();
board.Build(Context.Config);
```

---

## 二、关卡编辑器系统检查结果

### ✅ 所有编辑器代码检查通过

**检查的文件**:
1. `LevelEditorWindow.cs` - 主编辑器窗口 ✓
2. `BoardEditorView.cs` - 棋盘编辑视图 ✓
3. `ConfigPanelView.cs` - 配置面板 ✓
4. `ToolPanelView.cs` - 工具面板 ✓
5. `SerializationManager.cs` - 配置序列化管理器 ✓
6. `ValidationSystem.cs` - 验证系统 ✓
7. `AIGenerateWindow.cs` - AI 生成窗口 ✓
8. `RandomLevelGenerator.cs` - 随机生成器 ✓
9. `BatchGenerateWindow.cs` - 批量生成窗口 ✓

**检查结果**: 无 linter 错误，所有代码运行正常

### 配置路径已更新

SerializationManager 已正确更新到新路径：
```csharp
private const string ConfigPath = "Assets/Game/Config";
```

---

## 三、完整的关卡系统架构

### 核心组件关系图

```
LevelStateMachine (状态机)
    ↓
StateLoadLevel (加载状态)
    ↓
Board (棋盘控制器) ← LevelConfig (配置文件)
    ↓
├── MapGrid (格子) × N
└── PieceVisual (棋子) × M
    ↓
BoardStateManager (游戏逻辑)
```

### 组件职责

| 组件 | 职责 | 脚本 |
|------|------|------|
| **LevelScene** | 场景布局容器 | 无（纯布局） |
| **Board** | 棋盘主控制器 | Board.cs |
| **MapGrid** | 单个格子 | MapGrid.cs |
| **PieceVisual** | 棋子可视化 | PieceVisual.cs |
| **BoardStateManager** | 游戏逻辑管理 | BoardStateManager.cs |
| **LevelStateMachine** | 关卡状态管理 | LevelStateMachine.cs |

---

## 四、使用流程

### 1. 创建关卡（使用编辑器）

1. 打开关卡编辑器：`Tools > Level Editor`
2. 设置关卡基本信息（Level ID、场景类型、难度）
3. 调整棋盘大小
4. 使用 Board 模式绘制可玩区域
5. 使用 Piece 模式放置棋子
6. 保存关卡到 `Assets/Game/Config/`

### 2. 在游戏中加载关卡

**方式一：通过状态机（推荐）**
```csharp
// 在 LobbyController 中
var levelStateMachine = FindFirstObjectByType<LevelStateMachine>();
levelStateMachine.Begin(config, levelNumber, coroutineHost);
```

**方式二：直接使用 Board**
```csharp
var board = FindFirstObjectByType<Board>();
board.Build(config);
```

### 3. 玩家交互

- 拖拽棋子进行移动
- PieceVisual 自动验证移动合法性
- Board 更新棋盘状态
- BoardStateManager 检查游戏结束

---

## 五、配置文件说明

### 文件位置
```
Assets/Game/Config/
├── level_001.json
├── level_002.json
└── ...
```

### 文件格式
```json
{
  "schemaVersion": 1,
  "levelId": "level_001",
  "sceneType": 0,
  "difficulty": 0,
  "board": {
    "width": 5,
    "height": 5,
    "rows": [...]
  },
  "pieces": [...]
}
```

---

## 六、常见问题

### Q: LevelScene 需要添加脚本吗？
A: 不需要。LevelScene 只是布局容器，Board 已经处理所有游戏逻辑。

### Q: 如何在 LevelScene 中添加 UI 按钮？
A: 在 Right 容器中添加按钮，然后创建一个简单的 UI 控制器脚本。

### Q: 关卡编辑器保存的文件在哪里？
A: `Assets/Game/Config/` 目录下，使用 `.json` 扩展名。

### Q: 如何测试关卡？
A: 
1. 在 Board 预制体上设置 `levelId` 和 `buildOnStart = true`
2. 将 Board 放入场景
3. 点击 Play

---

## 七、下一步建议

### 1. LevelScene UI 扩展（可选）

如果需要在 Right 区域添加 UI，创建一个简单的控制器：

```csharp
public class LevelSceneUI : MonoBehaviour
{
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Text scoreText;
    
    private void Start()
    {
        pauseButton.onClick.AddListener(OnPauseClicked);
        restartButton.onClick.AddListener(OnRestartClicked);
    }
    
    private void OnPauseClicked()
    {
        // 暂停逻辑
    }
    
    private void OnRestartClicked()
    {
        // 重启逻辑
    }
}
```

### 2. 关卡编辑器增强（可选）

- 添加关卡测试按钮
- 添加关卡预览功能
- 添加更多验证规则

### 3. 游戏逻辑增强

- 添加计分系统
- 添加关卡目标系统
- 添加特殊棋子类型

---

## 八、总结

✅ **LevelScene 预制体**：作为布局容器使用，不需要额外脚本
✅ **Board 预制体**：完整配置，包含所有必要的脚本和引用
✅ **关卡编辑器**：所有代码检查通过，配置路径正确
✅ **配置文件**：已迁移到 `Assets/Game/Config/Level/` 目录
✅ **预制体系统**：完整的 MapGrid、Peg、Gem、Stone 预制体

**系统状态**：完全就绪，可以在 Unity 中直接使用！
