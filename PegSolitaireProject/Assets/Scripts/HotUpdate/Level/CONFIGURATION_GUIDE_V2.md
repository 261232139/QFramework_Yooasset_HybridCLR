# 关卡系统 v2.0 快速配置指南

## 一、脚本清单

### 新增脚本
✅ `LevelScene.cs` - 关卡主控制器
✅ `LevelInputHandler.cs` - 输入处理器
✅ `LevelGoalManager.cs` - 目标管理器
✅ `LevelUIController.cs` - UI 控制器

### 更新脚本
✅ `Board.cs` - 精简为布局管理器
✅ `StateLoadLevel.cs` - 支持 LevelScene
✅ `StateLevelReady.cs` - 启动 LevelScene

### 删除脚本
❌ `PieceVisual.cs` - 功能已整合到 LevelInputHandler

---

## 二、LevelScene 预制体配置步骤

### 步骤 1：添加 LevelScene 脚本

1. 打开 `LevelScene.prefab`
2. 选择根 GameObject "LevelScene"
3. 点击 "Add Component"
4. 搜索并添加 "LevelScene" 脚本

### 步骤 2：创建子对象结构

在 LevelScene 下创建以下结构：

```
LevelScene
├── BG (已存在)
├── GameArea (新建 Empty GameObject)
│   ├── Board (已存在，移动到这里)
│   └── InputHandler (新建 Empty GameObject)
└── UIArea (新建 Empty GameObject)
    ├── GoalManager (新建 Empty GameObject)
    └── UIController (新建 Empty GameObject)
```

### 步骤 3：添加管理器脚本

1. **InputHandler**:
   - 选择 InputHandler GameObject
   - 添加 "LevelInputHandler" 脚本
   - 设置 Drag Threshold: 10

2. **GoalManager**:
   - 选择 GoalManager GameObject
   - 添加 "LevelGoalManager" 脚本
   - 配置 Current Goal:
     - Goal Type: RemainOne
     - Target Count: 1

3. **UIController**:
   - 选择 UIController GameObject
   - 添加 "LevelUIController" 脚本

### 步骤 4：配置 LevelScene 引用

选择根 LevelScene GameObject，在 Inspector 中：

```
LevelScene (Script):
  Board: 拖拽 GameArea/Board
  Input Handler: 拖拽 GameArea/InputHandler
  Goal Manager: 拖拽 UIArea/GoalManager
  UI Controller: 拖拽 UIArea/UIController
```

### 步骤 5：配置 UI 元素（可选）

如果需要显示 UI，在 UIController 下创建：

1. **文本元素** (使用 TextMeshPro):
   - LevelNumberText
   - GoalText
   - MoveCountText
   - ScoreText

2. **按钮**:
   - PauseButton
   - RestartButton
   - BackButton

3. **面板**:
   - PausePanel (默认隐藏)
   - CompletePanel (默认隐藏)
   - FailPanel (默认隐藏)

然后在 LevelUIController 组件中拖拽这些引用。

---

## 三、Board 预制体更新

Board 预制体**不需要修改**，已经配置完成：

✅ Board 脚本
✅ MapGrid 模板
✅ 棋子预制体引用（Peg, Gem, Stone）

---

## 四、测试配置

### 最简配置（推荐）

如果暂时不需要 UI，可以使用最简配置：

```
LevelScene
├── Board (已配置)
├── LevelInputHandler (只需添加脚本)
└── LevelGoalManager (只需添加脚本)
```

LevelScene 脚本配置：
- Board: 指向 Board
- Input Handler: 指向 InputHandler GameObject
- Goal Manager: 指向 GoalManager GameObject
- UI Controller: 留空（null）

这样就可以进行基本的游戏测试了。

---

## 五、测试流程

### 1. 在场景中测试

1. 将配置好的 LevelScene.prefab 放入场景
2. 确保场景中有 EventSystem
3. 点击 Play
4. 通过状态机启动关卡

### 2. 使用编辑器测试

1. 在 Board 上设置：
   - Level Id: "level_001"
   - Build On Start: true
2. 点击 Play
3. Board 会自动加载关卡（兼容模式）

### 3. 通过代码测试

```csharp
var levelScene = FindFirstObjectByType<LevelScene>();
var config = /* 加载配置 */;
levelScene.LoadLevel(config, 1);
levelScene.StartLevel();
```

---

## 六、预制体 GUID 参考

需要在预制体 meta 文件中保证以下 GUID：

```yaml
脚本 GUID:
  Board.cs:              9a0eb7da03db42aa97c8f0c915680eb1
  MapGrid.cs:            27afc714d13e4ecf8a8f559f6ef3ef5d
  LevelScene.cs:         [Unity 会自动生成]
  LevelInputHandler.cs:  [Unity 会自动生成]
  LevelGoalManager.cs:   [Unity 会自动生成]
  LevelUIController.cs:  [Unity 会自动生成]
```

---

## 七、脚本 GUID 获取

如果需要手动配置预制体，可以通过以下方式获取脚本 GUID：

```bash
# 在项目目录下运行
Get-Content "Assets/Scripts/HotUpdate/Level/Runtime/LevelScene.cs.meta"
```

输出示例：
```
fileFormatVersion: 2
guid: xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
```

---

## 八、常见配置错误

### 错误 1：LevelScene 脚本引用丢失

**现象**：Inspector 显示 "Missing (Script)"

**解决**：
1. 检查脚本是否编译成功
2. 删除组件，重新添加
3. 检查命名空间是否正确

### 错误 2：拖拽时无法移动棋子

**原因**：
- EventSystem 缺失
- LevelInputHandler 未正确初始化
- Board 引用未配置

**解决**：
1. 确保场景有 EventSystem
2. 检查 LevelScene 的 Input Handler 引用
3. 检查 LevelInputHandler 的 Enable Debug Log，查看日志

### 错误 3：关卡无法完成

**原因**：
- LevelGoalManager 未配置
- 目标类型设置错误

**解决**：
1. 检查 Goal Manager 引用
2. 检查 Current Goal 配置
3. 启用 Debug Log 查看目标检测日志

---

## 九、调试技巧

### 启用调试日志

在各个管理器组件上：

```
LevelInputHandler:
  Enable Debug Log: true

LevelScene:
  [查看 Console 自动输出]

LevelGoalManager:
  [查看 Console 自动输出]
```

### 查看日志输出

运行时 Console 会显示：
```
[LevelScene] Level 1 loaded: level_001
[LevelScene] Level started: level_001
[LevelInputHandler] Selected piece peg_000 at (0, 0)
[LevelInputHandler] Moved peg_000: (0, 0) -> (2, 0), jumped: (1, 0)
[LevelGoalManager] Score: 10 (+10)
[LevelScene] No more valid moves!
[LevelScene] Level completed!
```

---

## 十、升级检查清单

从旧架构升级到 v2.0，检查以下项目：

- [ ] 删除 PieceVisual.cs 及其 meta 文件
- [ ] 更新 LevelScene.prefab 添加 LevelScene 脚本
- [ ] 添加 LevelInputHandler GameObject 和脚本
- [ ] 添加 LevelGoalManager GameObject 和脚本
- [ ] （可选）添加 LevelUIController GameObject 和脚本
- [ ] 配置 LevelScene 的所有引用
- [ ] 测试关卡加载和游戏流程
- [ ] 检查状态机是否正常工作

---

## 十一、完成后验证

运行以下测试确保配置正确：

1. ✅ 关卡能正常加载显示
2. ✅ 可以拖拽移动棋子
3. ✅ 移动后棋子位置正确
4. ✅ 被跳过的棋子被移除
5. ✅ 没有合法移动时触发结束
6. ✅ 目标完成时触发胜利
7. ✅ UI 正常显示和更新（如果配置了）

---

## 十二、性能优化建议

1. **对象池**：为棋子视觉对象使用对象池
2. **事件优化**：避免在事件回调中进行重计算
3. **UI 更新**：使用脏标记，避免每帧更新
4. **动画优化**：使用 DOTween 等优化的动画库

---

## 十三、下一步扩展

配置完成后，可以考虑添加：

1. **音效系统** - LevelAudioManager
2. **特效系统** - LevelEffectManager
3. **提示系统** - LevelHintManager
4. **录像回放** - LevelReplayManager
5. **成就系统** - LevelAchievementManager

每个新功能都是独立的管理器，不影响现有代码。

---

**配置完成后，系统即可投入使用！** 🎉
