# LevelView 架构快速指南

## 什么是 LevelView？

LevelView 是关卡系统的**总指挥官**，采用 MVC 模式重构，将逻辑和视图完全分离。

## 核心概念

### 三层架构

```
┌─────────────────────────────────────────┐
│         LevelView (总指挥官)              │
│    协调所有组件，不处理具体逻辑            │
└─────────────────────────────────────────┘
              │
              ├─────────────────────────────┐
              ↓                             ↓
    ┌──────────────────┐          ┌──────────────────┐
    │ LevelController  │          │   View 组件       │
    │   (纯逻辑层)      │          │  Board + UI      │
    │  不依赖Unity      │          │  (纯视图层)      │
    └──────────────────┘          └──────────────────┘
```

### 关键组件

| 组件 | 类型 | 职责 |
|------|------|------|
| **LevelView** | MonoBehaviour | 总协调器 |
| **LevelController** | 纯 C# 类 | 游戏逻辑控制 |
| **Board** | MonoBehaviour | 棋盘视觉布局 |
| **BoardStateManager** | 纯 C# 类 | 棋盘状态管理 |
| **LevelGoalManager** | 纯 C# 类 | 目标管理 |
| **LevelInputHandler** | MonoBehaviour | 输入处理 |
| **LevelUIController** | MonoBehaviour | UI 显示 |

## 数据流

### 玩家移动一个棋子的完整流程

```
1. 玩家拖拽 → LevelInputHandler 检测
                ↓
2. OnMoveRequested 事件 → LevelView.HandleMoveRequested()
                ↓
3. LevelController.ExecuteMove()
   ├─ 验证移动 (BoardStateManager)
   ├─ 更新状态 (BoardStateManager)
   └─ 更新目标 (LevelGoalManager)
                ↓
4. 返回 MoveResult → LevelView 接收
                ↓
5. 更新视图
   ├─ Board.UpdatePieceVisual() (棋盘)
   └─ UIController.OnPieceMoved() (UI)
```

## 与旧架构的区别

### 旧架构 (LevelScene)

```csharp
// Board 持有状态，职责混乱
public class Board : MonoBehaviour
{
    private BoardStateManager boardState; // ❌ 视图持有状态
    
    public void MovePiece(IPiece piece, GridPosition to)
    {
        boardState.MovePiece(piece, to); // ❌ 视图修改状态
        // 更新视觉...
    }
}
```

### 新架构 (LevelView)

```csharp
// Board 纯视图，不持有状态
public class Board : MonoBehaviour
{
    // ✅ 不持有状态
    
    public void UpdatePieceVisual(MoveResult result)
    {
        // ✅ 只负责更新视觉，接收已完成的结果
    }
}

// Controller 持有状态
public class LevelController
{
    private BoardStateManager boardState; // ✅ 逻辑层持有状态
    
    public MoveResult ExecuteMove(IPiece piece, GridPosition to)
    {
        // ✅ 逻辑层处理状态
        boardState.MovePiece(piece, to);
        return MoveResult.CreateSuccess(...);
    }
}
```

## 在 Unity 中使用

### 预制体配置

1. 在场景中创建 `LevelScene` GameObject
2. 添加 `LevelView` 组件（替换旧的 `LevelScene` 组件）
3. 配置引用：
   ```
   LevelView
   ├─ board: 拖拽 Board GameObject
   ├─ inputHandler: 拖拽 LevelInputHandler GameObject
   ├─ uiController: 拖拽 LevelUIController GameObject
   └─ levelGoal: 配置目标参数
   ```

### 加载关卡

```csharp
// StateLoadLevel 会自动检测并使用 LevelView
var levelView = levelSceneObj.GetComponent<LevelView>();
if (levelView != null)
{
    levelView.LoadLevel(config, levelNumber);
    levelView.StartLevel();
}
```

## 优势

### 1. 可测试性

```csharp
[Test]
public void TestMoveLogic()
{
    // ✅ 纯 C# 类，无需 Unity 环境即可测试
    var controller = new LevelController(mockConfig, mockGoalManager);
    var result = controller.ExecuteMove(piece, targetPos);
    Assert.IsTrue(result.Success);
}
```

### 2. 职责清晰

- **LevelView**: 协调通信，不处理具体逻辑
- **LevelController**: 纯逻辑，不依赖 Unity
- **Board**: 纯视图，只负责显示

### 3. 易于扩展

添加新功能只需修改 Controller：

```csharp
public class LevelController
{
    public void UndoMove() { }
    public void UseHint() { }
    public void ActivatePowerUp(PowerUpType type) { }
}
```

## 向后兼容

StateLoadLevel 同时支持：
- ✅ 新架构: `LevelView`
- ✅ 旧架构: `LevelScene`（兼容模式）

迁移是渐进式的，不会破坏现有功能。

## 参考文档

详细架构说明请参考：
- `ARCHITECTURE_V3.md` - 完整架构文档
- `ARCHITECTURE_V2.md` - 旧架构文档（对比参考）

---

**架构升级完成！🎉**
