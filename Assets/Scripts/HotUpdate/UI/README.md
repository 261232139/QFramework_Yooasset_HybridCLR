# 大厅界面使用指南

## 📋 已创建的文件

### 脚本文件
1. **LobbyPanel.cs** - 大厅UI面板脚本
2. **LobbyController.cs** - 大厅逻辑控制器
3. **StateEnterLobby.cs** - 已修改，支持大厅界面加载

---

## 🎯 功能说明

### LobbyPanel（大厅面板）
- 显示当前关卡编号（Level XX）
- 提供"进入关卡"按钮
- 可显示/隐藏

### LobbyController（大厅控制器）
- 管理当前关卡进度
- 处理关卡进入逻辑
- 加载关卡配置
- 单例模式，场景切换不销毁

---

## 🚀 Unity 中的设置步骤

### 方案 A：在场景中手动创建（推荐，简单快速）

1. **创建大厅 UI 对象**
   - 在 Hierarchy 右键 → UI → Canvas（如果没有）
   - 在 Canvas 下右键 → Create Empty
   - 命名为 "Lobby"

2. **添加 LobbyController 脚本**
   - 选中 "Lobby" 对象
   - 在 Inspector 中点击 "Add Component"
   - 搜索并添加 "LobbyController"

3. **创建 LobbyPanel UI**
   - 在 "Lobby" 下右键 → Create Empty
   - 命名为 "LobbyPanel"
   - 添加 "LobbyPanel" 脚本

4. **添加关卡文本**
   - 在 "LobbyPanel" 下右键 → UI → Text - TextMeshPro
   - 命名为 "LevelText"
   - 设置文本属性（字体、大小、颜色等）
   - 将此对象拖到 LobbyPanel 脚本的 "Level Text" 字段

5. **添加进入关卡按钮**
   - 在 "LobbyPanel" 下右键 → UI → Button - TextMeshPro
   - 命名为 "EnterLevelButton"
   - 修改按钮文本为 "进入关卡"
   - 将此对象拖到 LobbyPanel 脚本的 "Enter Level Button" 字段

6. **连接引用**
   - 选中 "Lobby" 对象（LobbyController）
   - 将 "LobbyPanel" 对象拖到 "Lobby Panel" 字段

7. **调整 UI 布局**
   - 设置 Canvas 为 Screen Space - Overlay
   - 调整 LevelText 和 Button 的位置和大小

### 方案 B：创建预制体（可复用）

1. 按照方案 A 创建完整的 Lobby 层级结构
2. 将 "Lobby" 对象拖到 `Assets/Resources/Prefabs/` 目录
3. 如果目录不存在，先创建它
4. 这样 StateEnterLobby 会自动从 Resources 加载

---

## 📐 推荐的 UI 布局

```
Canvas (Screen Space - Overlay)
└── Lobby (LobbyController)
    └── LobbyPanel (LobbyPanel)
        ├── LevelText (TextMeshProUGUI)
        │   ├── 位置: 屏幕上方中央
        │   ├── 文本: "Level 01"
        │   └── 字体大小: 48
        └── EnterLevelButton (Button)
            ├── 位置: 屏幕中央
            ├── 大小: 200x60
            └── 文本: "进入关卡"
```

---

## 🔧 配置说明

### LobbyController 参数
- **Start Level**: 起始关卡编号（默认 1）
- **Level Scene Name**: 关卡场景名称（如果使用场景切换）
- **Lobby Panel**: LobbyPanel 组件引用

### LobbyPanel 参数
- **Level Text**: 显示关卡编号的 TextMeshProUGUI 组件
- **Enter Level Button**: 进入关卡的按钮组件
- **Current Level**: 当前关卡编号（可在 Inspector 中预设）

---

## 🎮 工作流程

### 启动流程
```
Game Start
    ↓
Launch FSM（启动流程）
    ↓
HotUpdate FSM
    ↓
StateEnterLobby（进入大厅状态）
    ↓
OpenLobby()（打开大厅界面）
    ↓
显示 LobbyPanel（Level 01 + 进入关卡按钮）
```

### 进入关卡流程
```
用户点击"进入关卡"按钮
    ↓
LobbyPanel.OnEnterLevelClicked()
    ↓
LobbyController.EnterLevel(levelNumber)
    ↓
加载关卡配置（level_001.json）
    ↓
启动 LevelStateMachine
    ↓
进入游戏关卡
```

### 关卡完成流程
```
关卡胜利
    ↓
调用 LobbyController.LevelCompleted()
    ↓
currentLevel++
    ↓
返回大厅，显示下一关
```

---

## 💡 使用示例

### 在代码中访问 LobbyController
```csharp
// 获取 LobbyController 实例
var lobby = LobbyController.Instance;

// 设置当前关卡
lobby.SetCurrentLevel(5);

// 返回大厅
lobby.ReturnToLobby();

// 关卡完成，进入下一关
lobby.LevelCompleted();

// 获取当前关卡编号
int currentLevel = lobby.GetCurrentLevel();
```

### 从关卡返回大厅
```csharp
// 在关卡完成时调用
LobbyController.Instance.LevelCompleted();

// 或直接返回大厅
LobbyController.Instance.ReturnToLobby();
```

---

## 📝 关卡配置命名规则

关卡配置文件需要遵循以下命名规则：
- **格式**: `level_{编号:D3}.json`
- **示例**:
  - Level 1: `level_001.json`
  - Level 10: `level_010.json`
  - Level 99: `level_099.json`

文件位置: `Assets/Resources/LevelConfigs/`

---

## 🐛 常见问题

### Q: 点击按钮没有反应
**A**: 检查以下内容：
1. LobbyPanel 脚本的 Button 引用是否正确连接
2. Canvas 是否有 Graphic Raycaster 组件
3. Button 是否有 Interactable 勾选

### Q: 显示"未找到 LobbyController"
**A**: 确保：
1. 场景中有 LobbyController 组件
2. 或者 `Resources/Prefabs/Lobby` 预制体存在

### Q: 关卡配置加载失败
**A**: 确认：
1. 关卡配置文件在 `Assets/Resources/LevelConfigs/`
2. 文件命名符合 `level_001.json` 格式
3. 使用 Level Editor 创建的关卡配置

### Q: TextMeshPro 文本不显示
**A**: 
1. 确保导入了 TextMeshPro Essentials
2. 在 Window → TextMeshPro → Import TMP Essential Resources

---

## 🎨 美化建议

### 视觉效果
1. **背景**: 添加一个 Image 组件作为背景
2. **按钮**: 使用 Sprite 或 Gradient 美化按钮
3. **文本**: 使用 TMP 的 Gradient 和 Outline 效果
4. **动画**: 添加 DOTween 实现按钮和文本的动画效果

### 布局参考
```
- 关卡文本居中偏上（Y: 200）
- 进入按钮居中（Y: 0）
- 按钮大小: 宽 300, 高 80
- 字体大小: 标题 60, 按钮 36
```

---

## 🔄 后续扩展

### 可以添加的功能
1. **关卡选择界面**: 显示多个关卡供选择
2. **关卡星级**: 显示每关的完成星级
3. **玩家数据**: 保存/加载玩家进度
4. **音效**: 按钮点击音效和背景音乐
5. **过渡动画**: 场景切换的淡入淡出效果

---

## ✅ 快速检查清单

启动游戏前检查：
- [ ] Canvas 存在于场景中
- [ ] Lobby 对象包含 LobbyController 脚本
- [ ] LobbyPanel 对象包含 LobbyPanel 脚本
- [ ] LevelText 已连接到 LobbyPanel
- [ ] EnterLevelButton 已连接到 LobbyPanel
- [ ] LobbyPanel 已连接到 LobbyController
- [ ] 至少有一个关卡配置文件（如 level_001.json）

完成以上设置后，运行游戏应该能看到大厅界面！🎉
