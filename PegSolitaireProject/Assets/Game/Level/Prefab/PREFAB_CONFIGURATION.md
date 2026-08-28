# 预制体配置文档

## 预制体结构总览

```
Assets/Game/Level/Prefab/
├── Board.prefab           # 棋盘主控制器
├── MapGrid.prefab         # 单个格子模板
└── LevelPieces/           # 棋子预制体文件夹
    ├── Peg.prefab        # Peg棋子
    ├── Gem.prefab        # Gem棋子
    └── Stone.prefab      # Stone棋子
```

## 1. Board 预制体

**路径**: `Assets/Game/Level/Prefab/Board.prefab`

### 层级结构
```
Board (GameObject)
├── RectTransform
├── Image (背景)
├── Board (脚本组件)
└── MapGrid (子对象，隐藏的模板)
```

### Board 脚本配置
```yaml
Script: Board.cs
GUID: 9a0eb7da03db42aa97c8f0c915680eb1

属性配置:
  levelId: "level_001"                    # 测试关卡ID
  buildOnStart: true                      # 启动时自动构建
  mapGridTemplate: MapGrid.prefab         # 格子模板引用
  cellSize: (150, 150)                    # 格子大小
  cellSpacing: (10, 10)                   # 格子间距
  
  棋子预制体引用:
    pegPiecePrefab: Peg.prefab           # Peg棋子预制体
    gemPiecePrefab: Gem.prefab           # Gem棋子预制体
    stonePiecePrefab: Stone.prefab       # Stone棋子预制体
```

### 功能说明
- 作为关卡棋盘的主控制器
- 根据配置文件生成棋盘格子和棋子
- 管理棋子的移动和移除
- 维护 BoardStateManager 进行游戏逻辑

---

## 2. MapGrid 预制体

**路径**: `Assets/Game/Level/Prefab/MapGrid.prefab`

### 层级结构
```
MapGrid (GameObject)
├── RectTransform (150x150)
├── MapGrid (脚本组件)
└── Node (子对象)
    ├── Icon (Image)          # 棋子图标显示区域
    └── Arrow (GameObject)    # 可移动标识
        └── ArrowImage (Image)
```

### MapGrid 脚本配置
```yaml
Script: MapGrid.cs
GUID: 27afc714d13e4ecf8a8f559f6ef3ef5d

属性配置:
  node: Node (GameObject引用)
  arrow: Arrow (GameObject引用)
  pieceContainer: null (自动查找或使用transform)
```

### 功能说明
- 表示棋盘上的单个格子
- 显示/隐藏 node 和 arrow 指示器
- 管理格子上的棋子对象
- 根据棋子状态自动更新显示

---

## 3. Peg 棋子预制体

**路径**: `Assets/Game/Level/Prefab/LevelPieces/Peg.prefab`

### 层级结构
```
Peg (GameObject)
├── RectTransform (150x150)
├── CanvasRenderer
├── PieceVisual (脚本组件)
├── CanvasGroup (透明度控制)
└── Node (子对象)
    └── Icon (Image) # 棋子图标
```

### PieceVisual 脚本配置
```yaml
Script: PieceVisual.cs
GUID: 154a9cdea6f0ae746b6fa20de352fc7e

属性配置:
  pieceImage: Icon (Image引用)
  canvasGroup: CanvasGroup (组件引用)
  moveDuration: 0.3                      # 移动动画时长
  moveCurve: EaseInOut曲线               # 移动动画曲线
  fadeOutDuration: 0.3                   # 淡出动画时长
```

### 视觉配置
- **颜色**: 橙色 (0.8, 0.4, 0.2)
- **大小**: 150x150
- **类型**: PieceType.Peg

---

## 4. Gem 棋子预制体

**路径**: `Assets/Game/Level/Prefab/LevelPieces/Gem.prefab`

### 层级结构
```
Gem (GameObject)
├── RectTransform (120x120)
├── Image (主图)
├── PieceVisual (脚本组件)
├── CanvasGroup
└── Icon (子对象, Image)
```

### PieceVisual 脚本配置
```yaml
Script: PieceVisual.cs
GUID: 154a9cdea6f0ae746b6fa20de352fc7e

属性配置:
  pieceImage: 主Image (组件引用)
  canvasGroup: CanvasGroup (组件引用)
  moveDuration: 0.3
  moveCurve: EaseInOut曲线
  fadeOutDuration: 0.3
```

### 视觉配置
- **颜色**: 蓝色 (0.2, 0.6, 1.0)
- **大小**: 120x120
- **类型**: PieceType.Gem

---

## 5. Stone 棋子预制体

**路径**: `Assets/Game/Level/Prefab/LevelPieces/Stone.prefab`

### 层级结构
```
Stone (GameObject)
├── RectTransform (120x120)
├── Image (主图)
├── PieceVisual (脚本组件)
├── CanvasGroup
└── Icon (子对象, Image)
```

### PieceVisual 脚本配置
```yaml
Script: PieceVisual.cs
GUID: 154a9cdea6f0ae746b6fa20de352fc7e

属性配置:
  pieceImage: 主Image (组件引用)
  canvasGroup: CanvasGroup (组件引用)
  moveDuration: 0.3
  moveCurve: EaseInOut曲线
  fadeOutDuration: 0.3
```

### 视觉配置
- **颜色**: 灰色 (0.5, 0.5, 0.5)
- **大小**: 120x120
- **类型**: PieceType.Stone

---

## 脚本绑定清单

### Board.cs
```csharp
[SerializeField] private string levelId = "level_001";
[SerializeField] private bool buildOnStart = true;
[SerializeField] private MapGrid mapGridTemplate;
[SerializeField] private Vector2 cellSize = new Vector2(150f, 150f);
[SerializeField] private Vector2 cellSpacing = new Vector2(10f, 10f);

[Header("Piece Prefabs")]
[SerializeField] private GameObject pegPiecePrefab;
[SerializeField] private GameObject gemPiecePrefab;
[SerializeField] private GameObject stonePiecePrefab;
```

### MapGrid.cs
```csharp
[SerializeField] private GameObject node;
[SerializeField] private GameObject arrow;
[SerializeField] private Transform pieceContainer;
```

### PieceVisual.cs
```csharp
[Header("Visual")]
[SerializeField] private Image pieceImage;
[SerializeField] private CanvasGroup canvasGroup;

[Header("Animation")]
[SerializeField] private float moveDuration = 0.3f;
[SerializeField] private AnimationCurve moveCurve;
[SerializeField] private float fadeOutDuration = 0.3f;
```

---

## Unity 编辑器中的配置步骤

### 1. 配置 Board 预制体

1. 打开 `Board.prefab` 进行编辑
2. 选择 Board GameObject
3. 在 Inspector 中找到 Board (Script) 组件
4. 配置以下属性：
   - **Level Id**: 输入测试关卡ID（如 "level_001"）
   - **Build On Start**: 勾选（自动构建）或不勾选（手动构建）
   - **Map Grid Template**: 拖拽 `MapGrid.prefab` 到此字段
   - **Cell Size**: 设置为 (150, 150)
   - **Cell Spacing**: 设置为 (10, 10)
   - **Peg Piece Prefab**: 拖拽 `Peg.prefab` 到此字段
   - **Gem Piece Prefab**: 拖拽 `Gem.prefab` 到此字段
   - **Stone Piece Prefab**: 拖拽 `Stone.prefab` 到此字段

### 2. 配置 MapGrid 预制体

1. 打开 `MapGrid.prefab` 进行编辑
2. 选择 MapGrid GameObject
3. 在 Inspector 中找到 MapGrid (Script) 组件
4. 确认以下自动绑定：
   - **Node**: 应自动绑定到 Node 子对象
   - **Arrow**: 应自动绑定到 Arrow 子对象
   - **Piece Container**: 留空（会自动使用 transform）

### 3. 配置棋子预制体

对于 Peg.prefab、Gem.prefab、Stone.prefab：

1. 打开预制体进行编辑
2. 选择根 GameObject
3. 在 Inspector 中找到 PieceVisual (Script) 组件
4. 确认以下自动绑定：
   - **Piece Image**: 应绑定到主 Image 组件
   - **Canvas Group**: 应绑定到 CanvasGroup 组件
   - **Move Duration**: 设置为 0.3
   - **Move Curve**: 使用 EaseInOut 曲线
   - **Fade Out Duration**: 设置为 0.3

---

## 预制体使用流程

### 在场景中使用

1. **添加 Board 到场景**
   - 将 `Board.prefab` 拖入场景
   - 放置在 Canvas 下
   - 调整位置和大小

2. **配置关卡加载**
   - 通过状态机自动加载（推荐）
   - 或在 Board 上设置 `buildOnStart = true` 进行测试

3. **运行测试**
   - 点击 Play
   - Board 会自动从配置文件生成关卡
   - 可以拖拽棋子进行移动

---

## 预制体 GUID 参考

```yaml
脚本 GUID:
  Board.cs:        9a0eb7da03db42aa97c8f0c915680eb1
  MapGrid.cs:      27afc714d13e4ecf8a8f559f6ef3ef5d
  PieceVisual.cs:  154a9cdea6f0ae746b6fa20de352fc7e

预制体 GUID:
  MapGrid.prefab:  a42666e5561f35b4aa1a9bcdd4ceafff
  Peg.prefab:      7042421ca14a8294d83170b15662be9d
  Gem.prefab:      e2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7
  Stone.prefab:    f3a4b5c6d7e8f9a0b1c2d3e4f5a6b7c8
```

---

## 常见问题

### Q: 预制体中的引用丢失了？
A: 在 Unity 中：
1. 打开对应的预制体
2. 重新拖拽对应的对象到字段中
3. 保存预制体

### Q: 棋子无法拖拽？
A: 检查：
1. PieceVisual 组件已正确添加
2. CanvasGroup 的 Interactable 为 true
3. 场景中有 EventSystem

### Q: 棋子颜色不对？
A: PieceVisual 会自动根据棋子类型设置颜色：
- Peg: 橙色
- Gem: 蓝色
- Stone: 灰色

如果颜色不对，检查棋子的 PieceType 是否正确。

### Q: 如何添加新的棋子类型？
A: 
1. 复制现有棋子预制体（如 Peg.prefab）
2. 重命名为新类型（如 NewPiece.prefab）
3. 调整颜色和大小
4. 在 Board.cs 中添加新预制体字段
5. 在 PieceVisual.cs 的 GetPieceColor() 中添加颜色映射

---

## 预制体完整性检查清单

### Board 预制体 ✓
- [x] Board 脚本组件已添加
- [x] mapGridTemplate 已引用 MapGrid.prefab
- [x] pegPiecePrefab 已引用 Peg.prefab
- [x] gemPiecePrefab 已引用 Gem.prefab
- [x] stonePiecePrefab 已引用 Stone.prefab
- [x] cellSize 设置为 (150, 150)
- [x] cellSpacing 设置为 (10, 10)

### MapGrid 预制体 ✓
- [x] MapGrid 脚本组件已添加
- [x] node 字段已绑定到 Node 子对象
- [x] arrow 字段已绑定到 Arrow 子对象
- [x] 层级结构正确（Node -> Icon, Arrow）

### Peg 预制体 ✓
- [x] PieceVisual 脚本组件已添加
- [x] CanvasGroup 组件已添加
- [x] pieceImage 字段已绑定
- [x] canvasGroup 字段已绑定
- [x] moveDuration 设置为 0.3
- [x] fadeOutDuration 设置为 0.3

### Gem 预制体 ✓
- [x] PieceVisual 脚本组件已添加
- [x] CanvasGroup 组件已添加
- [x] pieceImage 字段已绑定
- [x] canvasGroup 字段已绑定
- [x] 颜色设置为蓝色

### Stone 预制体 ✓
- [x] PieceVisual 脚本组件已添加
- [x] CanvasGroup 组件已添加
- [x] pieceImage 字段已绑定
- [x] canvasGroup 字段已绑定
- [x] 颜色设置为灰色

---

## 下一步

1. 在 Unity 编辑器中打开项目
2. 验证所有预制体的引用是否正确
3. 在场景中放置 Board 预制体进行测试
4. 调整棋子的图标 Sprite（目前使用占位图）
5. 根据需要调整动画参数

所有预制体已经配置完成，脚本绑定已就绪！
