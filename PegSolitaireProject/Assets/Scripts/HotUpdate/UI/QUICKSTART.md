# 🎮 大厅系统快速操作指南

## ✅ 当前状态
- ✅ 所有脚本文件已创建（无编译错误）
- ✅ 编辑器工具已就绪
- ✅ 示例关卡配置已创建 (level_001.json)
- ✅ 启动流程已集成大厅系统

---

## 🚀 立即开始（3步完成）

### 步骤 1：在 Unity 中创建大厅 UI

1. **打开 Unity 编辑器**
   - 确保当前场景已打开（通常是 Boot.unity）

2. **点击菜单**：
   ```
   Tools → Create Lobby UI
   ```

3. **在弹出的对话框中点击"创建"**
   - 工具会自动创建完整的 UI 结构
   - 所有组件引用会自动连接

4. **查看 Hierarchy**，应该看到：
   ```
   Canvas
   └── Lobby
       └── LobbyPanel
           ├── LevelText
           └── EnterLevelButton
   ```

### 步骤 2：保存场景

- 按 `Ctrl + S` 保存场景

### 步骤 3：运行测试

1. **按 Play 按钮**（或 `Ctrl + P`）

2. **观察流程**：
   ```
   [启动加载画面] 
        ↓
   [进度条加载]
        ↓
   [大厅界面显示]
        ↓
   显示 "Level 01" 和 "进入关卡" 按钮
   ```

3. **点击"进入关卡"按钮**
   - 会加载 level_001.json
   - 启动关卡状态机
   - 开始游戏

---

## 📋 已创建的文件清单

### 运行时脚本
```
Assets/Scripts/HotUpdate/UI/
├── LobbyPanel.cs           ✅ 大厅UI面板
├── LobbyController.cs      ✅ 大厅逻辑控制器
└── README.md               ✅ 详细使用文档
```

### 编辑器工具
```
Assets/Scripts/HotUpdate/UI/Editor/
├── LobbyUICreator.cs       ✅ 自动创建工具
└── HotUpdate.UI.Editor.asmdef  ✅ 程序集定义
```

### 已修改的文件
```
Assets/Scripts/HotUpdate/States/
└── StateEnterLobby.cs      ✅ 集成大厅系统
```

### 示例关卡
```
Assets/Resources/LevelConfigs/
└── level_001.json          ✅ 测试关卡配置
```

---

## 🎯 自动创建的 UI 结构

```
Canvas (Screen Space - Overlay)
└── Lobby
    ├── LobbyController 组件
    │   ├─ Start Level: 1
    │   └─ Lobby Panel: [自动连接]
    │
    └── LobbyPanel (GameObject)
        ├── LobbyPanel 组件
        │   ├─ Level Text: [自动连接]
        │   ├─ Enter Level Button: [自动连接]
        │   └─ Current Level: 1
        │
        ├── LevelText (TextMeshProUGUI)
        │   ├─ 文本: "Level 01"
        │   ├─ 字体大小: 48
        │   ├─ 对齐: 居中
        │   ├─ 颜色: 白色
        │   └─ 位置: (0, 200)
        │
        └── EnterLevelButton (Button)
            ├─ 尺寸: 300x80
            ├─ 背景颜色: 蓝色
            ├─ 位置: (0, 0)
            └── Text (TextMeshProUGUI)
                ├─ 文本: "进入关卡"
                ├─ 字体大小: 36
                └─ 颜色: 白色
```

---

## 🔄 完整的游戏流程

### 1. 启动阶段
```
Unity 启动
    ↓
Boot 场景加载
    ↓
GameLauncher 启动
    ↓
Launch FSM（启动流程）
    ├─ StateLaunch
    ├─ StateHotCheckVersion
    └─ StateHotDownload
    ↓
HotUpdateRunner 接管
    ↓
HotUpdate FSM
    ├─ StateLoadModules
    └─ StateEnterLobby ⭐
        ↓
    OpenLobby()
        ↓
    显示大厅界面
```

### 2. 大厅阶段
```
大厅界面显示
    ↓
显示 "Level 01"
显示 "进入关卡" 按钮
    ↓
用户点击按钮
```

### 3. 关卡阶段
```
LobbyPanel.OnEnterLevelClicked()
    ↓
LobbyController.EnterLevel(1)
    ↓
加载 level_001.json
    ↓
验证关卡配置
    ↓
启动 LevelStateMachine
    ↓
开始游戏
```

### 4. 关卡完成
```
关卡胜利/失败
    ↓
调用 LobbyController.LevelCompleted()
    ↓
currentLevel++
    ↓
返回大厅，显示下一关
```

---

## 🧪 测试检查点

### ✅ 创建 UI 后检查
- [ ] Hierarchy 中有 Canvas/Lobby/LobbyPanel
- [ ] LobbyPanel 下有 LevelText 和 EnterLevelButton
- [ ] Lobby 组件的 Lobby Panel 字段已连接
- [ ] LobbyPanel 组件的字段都已连接

### ✅ 运行游戏后检查
- [ ] 启动流程正常（加载条显示）
- [ ] 加载条消失后显示大厅界面
- [ ] 看到 "Level 01" 文本
- [ ] 看到 "进入关卡" 按钮
- [ ] Console 显示 "[EnterLobby] 打开大厅界面"

### ✅ 点击按钮后检查
- [ ] Console 显示 "[LobbyPanel] 进入关卡: Level 1"
- [ ] Console 显示 "[LobbyController] 准备进入关卡: Level 1"
- [ ] Console 显示 "[LobbyController] 加载关卡配置: level_001"
- [ ] 关卡状态机启动

---

## 💡 代码使用示例

### 从关卡返回大厅
```csharp
// 在关卡胜利代码中
LobbyController.Instance.LevelCompleted();  // 进入下一关

// 或者关卡失败时
LobbyController.Instance.ReturnToLobby();   // 重新挑战
```

### 跳转到指定关卡
```csharp
LobbyController.Instance.SetCurrentLevel(5);
LobbyController.Instance.ReturnToLobby();
```

### 获取当前关卡
```csharp
int currentLevel = LobbyController.Instance.GetCurrentLevel();
Debug.Log($"当前关卡: {currentLevel}");
```

---

## 🎨 美化建议（可选）

### 修改颜色
在创建后，可以手动调整：
- **LevelText**: 字体、颜色、大小
- **按钮背景**: Image 组件的 Color
- **按钮文本**: 字体、颜色、大小

### 添加背景
1. 在 LobbyPanel 下添加 Image 组件
2. 设置为全屏（Anchor Presets: Stretch)
3. 设置背景图片或纯色

### 添加动画
1. 使用 DOTween 实现按钮缩放动画
2. 文本淡入淡出效果
3. 场景过渡动画

---

## 🐛 常见问题排查

### Q: 点击菜单没有"Create Lobby UI"
**A**: 
- 确保 Unity 编译完成（查看右下角进度条）
- 确保有打开的场景（不是 Project 视图）

### Q: 点击按钮没反应
**A**:
- 检查 Canvas 是否有 Graphic Raycaster 组件
- 检查 Button 组件的 Interactable 是否勾选
- 检查 EventSystem 是否存在（自动创建）

### Q: 显示"未找到关卡配置"
**A**:
- 确认 `Assets/Resources/LevelConfigs/level_001.json` 存在
- 文件名必须是 `level_001.json`（3位数字）
- 使用 Level Editor 创建更多关卡

### Q: TextMeshPro 报错
**A**:
- Window → TextMeshPro → Import TMP Essential Resources
- 点击 Import

---

## 📚 相关文档

- **详细使用文档**: `Assets/Scripts/HotUpdate/UI/README.md`
- **关卡编辑器**: `Assets/Scripts/HotUpdate/Level/Editor/README.md`
- **关卡编辑器快速指南**: `Assets/Scripts/HotUpdate/Level/Editor/QUICKSTART.md`

---

## 🎉 准备就绪！

**现在去 Unity 中执行以下操作：**

1. **Tools → Create Lobby UI**
2. **保存场景 (Ctrl+S)**
3. **运行游戏 (Ctrl+P)**
4. **点击"进入关卡"按钮测试**

如果一切正常，你会看到大厅界面，并能成功进入关卡！

---

**有任何问题随时告诉我！** 🚀
