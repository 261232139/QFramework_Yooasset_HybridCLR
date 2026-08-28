# 如何使用自动化配置工具

## 快速开始

### 方法 1: 一键自动配置（推荐）

1. 在 Unity 编辑器顶部菜单栏，选择：
   ```
   Tools → Level → Auto Setup LevelView Architecture ⚡
   ```

2. 等待配置完成（约 2-3 秒）

3. 在弹出的对话框中选择：
   - **进入 Play 模式** - 立即测试
   - **稍后手动测试** - 稍后再测试

### 方法 2: 分步骤配置

1. 打开测试工具窗口：
   ```
   Tools → Level → Level Flow Tester
   ```

2. 按顺序点击：
   - **配置 LevelScene 预制体**
   - **验证预制体**
   - **进入 Play 模式并测试**

## 测试关卡加载流程

### 步骤 1: 进入 Play 模式

点击 Unity 编辑器的 Play 按钮，或使用快捷键 `Ctrl+P`

### 步骤 2: 选择关卡

在游戏中进入关卡选择界面，选择任意关卡

### 步骤 3: 观察日志

打开 Console 窗口（`Ctrl+Shift+C`），你应该看到类似的日志：

```
========== 进入 Play 模式 ==========
开始监控关卡加载流程...
✓ 找到 LevelView 组件（新架构）
[LevelState] LoadLevel - Loading level 1
[StateLoadLevel] Using LevelView (new architecture)
[LevelController] Created for level: level_001
[Board] Layout built: 7x7, 12 pieces
[LevelView] Level 1 loaded: level_001
[LevelView] Level started: level_001
✓ 关卡开始事件: level_001
```

### 步骤 4: 测试游戏功能

- 拖拽棋子进行移动
- 观察目标系统是否正常工作
- 检查 UI 是否正确更新

## 验证要点

### ✓ 成功标志

- Console 中显示 "Using LevelView (new architecture)"
- 没有错误日志
- 棋子可以正常移动
- 移动次数和分数正确更新
- 关卡完成/失败逻辑正常

### ✗ 失败标志

- Console 中显示 "Using LevelScene (legacy mode)"
- 出现 NullReferenceException
- 棋子无法移动或移动后不消失
- UI 不更新

## 故障排查

### 问题 1: 预制体配置失败

**解决方案**：
1. 手动打开 `Assets/Game/Level/Prefab/LevelScene.prefab`
2. 确认根对象有 `LevelView` 组件
3. 确认以下引用已配置：
   - Board
   - InputHandler  
   - UIController

### 问题 2: 编译错误

**解决方案**：
1. 检查 Console 中的错误信息
2. 确认所有新脚本都已创建
3. 点击 `Assets → Reimport All`

### 问题 3: 关卡无法加载

**解决方案**：
1. 检查 StateLoadLevel.cs 是否已更新
2. 确认 LevelScene.prefab 在正确的路径
3. 查看 Console 日志，找到具体错误原因

## 手动配置指南（备用）

如果自动配置失败，可以手动配置：

### 1. 编辑 LevelScene 预制体

1. 打开 `Assets/Game/Level/Prefab/LevelScene.prefab`
2. 选择根对象 "LevelScene"
3. 在 Inspector 中：
   - 移除 `LevelScene` 组件（如果存在）
   - 添加 `LevelView` 组件

### 2. 配置 LevelView 引用

在 LevelView 组件中：

- **Board**: 拖拽 Left/Board 对象
- **Input Handler**: 拖拽 Left/LevelInputHandler 对象（如果没有则创建）
- **UI Controller**: 拖拽 Right/LevelUIController 对象（如果没有则创建）

### 3. 配置 Level Goal

在 LevelView 组件中设置：

- **Goal Type**: RemainOne
- **Target Count**: 1
- **Target Piece Type**: Peg

### 4. 保存并应用

按 `Ctrl+S` 保存预制体

## 有用的菜单项

- `Tools → Level → Auto Setup LevelView Architecture ⚡` - 一键配置
- `Tools → Level → Configure LevelScene Prefab` - 仅配置预制体
- `Tools → Level → Verify LevelScene Prefab` - 仅验证配置
- `Tools → Level → Level Flow Tester` - 打开测试工具窗口

## 相关文档

- `ARCHITECTURE_V3.md` - 完整架构文档
- `README_LEVELVIEW.md` - 架构快速指南
- `SETUP_REPORT.txt` - 自动生成的配置报告

---

**祝测试顺利！** 🎉
