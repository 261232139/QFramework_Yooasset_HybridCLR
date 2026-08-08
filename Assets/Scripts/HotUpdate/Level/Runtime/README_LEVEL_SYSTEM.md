# 关卡生成系统使用指南

## 概述

基于现有的 `Board` 和 `MapGrid` 架构，增强了关卡生成系统，现在可以通过配置文件自动生成完整的可玩关卡。

## 核心组件

### 1. Board（棋盘管理器）
**路径**：`Assets/Scripts/HotUpdate/Level/Runtime/Board.cs`

**功能**：
- 从配置文件生成棋盘格子（MapGrid）
- 自动创建棋子的可视化对象
- 管理棋子的移动和移除
- 维护 BoardStateManager 进行游戏逻辑管理

**新增属性**：
```csharp
public LevelConfig CurrentConfig { get; }       // 当前关卡配置
public BoardStateManager BoardState { get; }    // 棋盘状态管理器
```

**新增方法**：
```csharp
void MovePiece(IPiece piece, GridPosition to)  // 移动棋子
void RemovePiece(IPiece piece)                 // 移除棋子
```

### 2. MapGrid（棋盘格子）
**路径**：`Assets/Scripts/HotUpdate/Level/Runtime/MapGrid.cs`

**功能**：
- 表示棋盘上的单个格子
- 管理格子上的棋子对象
- 显示/隐藏 node 和 arrow

**新增方法**：
```csharp
void SetPieceObject(GameObject pieceObj)  // 设置格子上的棋子
GameObject GetPieceObject()               // 获取格子上的棋子
bool HasPiece()                           // 检查是否有棋子
```

### 3. PieceVisual（棋子可视化组件）
**路径**：`Assets/Scripts/HotUpdate/Level/Runtime/PieceVisual.cs`

**功能**：
- 处理棋子的拖拽交互
- 验证和执行移动
- 提供移动和淡出动画
- 自动颜色配置

**主要特性**：
- 实现了 `IBeginDragHandler`, `IDragHandler`, `IEndDragHandler`
- 支持拖拽到目标格子进行移动
- 自动检测碰撞和验证移动合法性
- 移动失败时自动返回原位置

### 4. BoardStateManager（棋盘状态管理器）
**路径**：`Assets/Scripts/HotUpdate/Level/Runtime/BoardStateManager.cs`

**功能**：
- 维护所有棋子的逻辑状态
- 提供 IBoardState 接口供移动验证
- 检查游戏结束条件

## 使用方式

### 方式一：通过状态机（推荐）

状态机会自动处理关卡生成：

```csharp
// 在 LobbyController 中
var levelStateMachine = FindFirstObjectByType<LevelStateMachine>();
levelStateMachine.Begin(config, levelNumber, coroutineHost);
```

状态机流程：
1. `StateLobbyToLevel` - 隐藏大厅UI
2. `StateLoadLevel` - 查找/创建 Board，调用 `Board.Build(config)`
3. `StateLevelReady` - 初始化游戏数据
4. `StateLevelRunning` - 开始游戏

### 方式二：直接使用 Board

```csharp
// 获取或创建 Board
var board = FindFirstObjectByType<Board>();
if (board == null)
{
    var boardObj = new GameObject("Board");
    board = boardObj.AddComponent<Board>();
}

// 从配置生成关卡
board.Build(config);

// 手动移动棋子
board.MovePiece(piece, targetPosition);

// 移除棋子
board.RemovePiece(piece);
```

## Unity 编辑器配置

### Board 组件设置

在场景中创建或使用 Board 预制体：

```
Board (GameObject)
├─ Board (Component)
│  ├─ Level Id: "level_001"           // 测试用关卡ID
│  ├─ Build On Start: true/false      // 是否在Start时自动构建
│  ├─ Map Grid Template: MapGrid预制体 // 格子模板
│  ├─ Cell Size: (150, 150)           // 格子大小
│  ├─ Cell Spacing: (10, 10)          // 格子间距
│  │
│  └─ Piece Prefabs
│     ├─ Peg Piece Prefab             // Peg棋子预制体
│     ├─ Gem Piece Prefab             // Gem棋子预制体
│     └─ Stone Piece Prefab           // Stone棋子预制体
│
└─ MapGrid (隐藏的模板，子对象)
   ├─ Node (子对象)
   │  └─ Arrow (子对象)
   └─ PieceContainer (可选)
```

### 预制体要求

#### MapGrid 预制体
- `RectTransform` 组件
- `MapGrid` 组件
- 结构：
  ```
  MapGrid
  ├─ BG (Image) - 背景
  ├─ Node (GameObject)
  │  └─ Arrow (GameObject) - 可移动标识
  └─ PieceContainer (可选) - 棋子容器
  ```

#### Piece 预制体（Peg, Gem, Stone）
- `RectTransform` 组件
- `Image` 组件（显示棋子图标）
- `CanvasGroup` 组件（可选，会自动添加）
- `PieceVisual` 组件（会自动添加）

## 游戏流程

### 1. 关卡生成
```
Board.Build(config)
  ├─ 验证配置
  ├─ 创建 BoardStateManager
  ├─ 生成棋盘格子
  │  └─ 为每个可玩格子创建 MapGrid
  └─ 生成棋子
     └─ 为每个棋子创建可视化对象
        ├─ 实例化预制体
        ├─ 添加 PieceVisual 组件
        └─ 初始化并关联逻辑
```

### 2. 玩家交互
```
玩家拖拽棋子
  └─ PieceVisual.OnBeginDrag
     └─ 记录原始位置
  
  └─ PieceVisual.OnDrag
     └─ 跟随鼠标移动

  └─ PieceVisual.OnEndDrag
     ├─ 检测目标 MapGrid
     ├─ 验证移动合法性 (IPiece.ValidateMove)
     ├─ 如果合法
     │  ├─ Board.MovePiece
     │  ├─ Board.RemovePiece (被跳跃的棋子)
     │  └─ 检查游戏结束
     └─ 如果失败
        └─ 返回原位置（带动画）
```

### 3. 游戏结束检测
```
每次移动后
  └─ BoardStateManager.HasMovablePieces()
     ├─ 遍历所有棋子
     ├─ 检查每个棋子是否有合法移动
     └─ 如果没有可移动棋子
        └─ 游戏结束
```

## 配置文件

配置文件位于：`Assets/Game/Config/level_XXX.json`

示例：
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
  "pieces": [
    {
      "id": "peg_000",
      "pieceType": 0,
      "isMovable": true,
      "position": { "x": 0, "y": 0 }
    }
  ]
}
```

## 移动规则

基于 `PieceBase` 的移动规则：
1. 只能向上下左右四个方向移动
2. 必须跨越恰好一个棋子
3. 目标位置必须为空
4. 被跨越的棋子会被移除

## API 参考

### Board 主要方法

```csharp
// 从配置构建关卡
void Build(LevelConfig config)

// 移动棋子
void MovePiece(IPiece piece, GridPosition to)

// 移除棋子
void RemovePiece(IPiece piece)

// 获取指定位置的 MapGrid
bool TryGetGrid(GridPosition position, out MapGrid grid)
```

### MapGrid 方法

```csharp
// 初始化格子
void Initialize(GridPosition position, PieceData piece)

// 设置/获取棋子对象
void SetPieceObject(GameObject pieceObj)
GameObject GetPieceObject()
bool HasPiece()
```

### PieceVisual 方法

```csharp
// 初始化
void Initialize(IPiece piece, Board boardRef)

// 动画
IEnumerator MoveToPositionAnimated(Vector2 targetPosition, Transform targetParent)
IEnumerator FadeOutAndDestroy()
```

### BoardStateManager 方法

```csharp
// 棋子管理
void MovePiece(IPiece piece, GridPosition newPosition)
void RemovePiece(GridPosition position)
void ResetAllPieces()

// 游戏状态
bool HasMovablePieces()
IPiece GetPieceAt(GridPosition position)
bool HasPieceAt(GridPosition position)
```

## 扩展功能

### 添加新棋子类型

1. 在 `PieceType` 枚举中添加新类型
2. 在 `PieceFactory` 中添加创建逻辑
3. 在 `Board` 的 `GetPiecePrefab` 中添加预制体映射
4. 在 Unity 中配置对应的预制体

### 自定义移动规则

继承 `PieceBase` 并覆盖 `ValidateMoveCustom`：

```csharp
public class SpecialPiece : PieceBase
{
    protected override MoveResult ValidateMoveCustom(
        GridPosition from, GridPosition to, 
        IBoardState board, MoveResult baseResult)
    {
        // 自定义移动规则
        return baseResult;
    }
}
```

### 添加动画效果

修改 `PieceVisual` 的动画参数或实现自定义动画：

```csharp
[SerializeField] private float moveDuration = 0.3f;
[SerializeField] private AnimationCurve moveCurve;
```

## 调试技巧

1. **查看关卡生成日志**
   - Board 会输出关卡构建信息
   - PieceVisual 会输出移动验证结果

2. **测试关卡配置**
   - 在 Board 组件上设置 `Level Id` 和 `Build On Start`
   - 运行场景自动加载测试

3. **验证移动规则**
   - 查看控制台的移动失败原因
   - 使用 `ValidateMove` 手动测试

## 常见问题

**Q: 棋子无法拖拽？**
A: 检查：
- Scene 中是否有 EventSystem
- PieceVisual 的 CanvasGroup.interactable 是否为 true
- 棋子的 isMovable 是否为 true

**Q: 棋子位置不正确？**
A: 检查：
- Board 的 cellSize 和 cellSpacing 设置
- MapGrid 的 RectTransform 设置
- Canvas 的 Scale Mode

**Q: 游戏结束检测不工作？**
A: 确保：
- BoardStateManager 正确更新
- HasMovablePieces 逻辑正确
- 至少有一个可移动棋子在配置中

**Q: 配置文件加载失败？**
A: 检查：
- 配置文件在 `Assets/Game/Config/` 目录下
- YooAsset 资源已正确配置
- levelId 与文件名匹配

## 总结

系统遵循现有架构设计：
- ✅ 使用 `Board` 作为主控制器
- ✅ `MapGrid` 作为格子单元
- ✅ 通过状态机管理关卡流程
- ✅ `BoardStateManager` 管理游戏逻辑
- ✅ `PieceVisual` 处理交互和动画
- ✅ 配置文件驱动关卡生成
