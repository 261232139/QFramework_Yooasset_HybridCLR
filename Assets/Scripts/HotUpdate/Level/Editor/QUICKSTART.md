# 关卡编辑器快速开始指南

## 🎯 已完成的功能

✅ **完整的关卡编辑器系统**
- 可视化网格编辑界面
- 实时验证系统
- 撤销/重做功能
- AI 生成接口（Cursor 集成）
- 批量生成工具

---

## 🚀 快速使用

### 1. 打开编辑器

Unity 菜单栏: **Tools > Level Editor**

### 2. 创建第一个关卡

1. 点击工具栏 **New** 按钮
2. 在左侧面板设置：
   - Level ID: `my_first_level`
   - Scene Type: Forest
   - Difficulty: Easy
3. 调整棋盘尺寸（左侧 Board Size 滑块）
4. 右侧选择 **Board** 模式
5. 选择 **Playable** 类型
6. 点击网格绘制可玩格子
7. 切换到 **Piece** 模式
8. 选择棋子类型并点击格子放置
9. 确保左侧验证无错误
10. 点击 **Save** 保存

### 3. 使用 AI 生成关卡（与 Cursor 配合）

#### 方式一：完全自动（推荐）
1. 点击工具栏 **AI Generate** 按钮
2. 配置生成参数（尺寸、难度、约束等）
3. 点击 **Copy Parameters to Clipboard**
4. 在 Cursor Chat 中输入：

```
请根据以下参数生成一个 LevelConfig JSON：
[粘贴参数]

要求：
- 确保 board.rows 完整包含所有行和列
- pieces 位置必须在可玩格子上
- 至少有一个可移动棋子
- 返回完整的 JSON，不要省略任何部分
```

5. 复制 Cursor 返回的 JSON
6. 回到编辑器，点击 **Show JSON Input**
7. 粘贴 JSON 并点击 **Load from JSON**
8. 关卡加载成功！可继续手动微调

#### 方式二：使用内置随机生成器
1. 打开 **AI Generate** 窗口
2. 配置参数
3. 点击 **Use Random Generator**
4. 立即生成并加载

---

## 📁 文件位置

- **编辑器代码**: `Assets/Scripts/HotUpdate/Level/Editor/`
- **关卡配置**: `Assets/Resources/LevelConfigs/`
- **示例关卡**: `level_example.json` (已创建)

---

## 🔧 编辑器界面说明

### 左侧面板（配置与验证）
- 基础信息设置
- 棋盘尺寸调整（4x4 到 7x9）
- 统计信息
- 实时验证结果（错误/警告/提示）

### 中央面板（可视化编辑）
- **灰色背景**: 编辑区域
- **白色格子**: 可玩格子 (Playable)
- **深灰格子**: 空格子 (Void)
- **橙色圆**: Peg 棋子
- **蓝色圆**: Gem 棋子
- **灰色圆**: Stone 棋子
- **`>X<` 标记**: 可移动棋子
- **鼠标滚轮**: 缩放视图

### 右侧面板（工具栏）
- **Board 模式**: 绘制棋盘格子
  - Void: 空格子（不可玩）
  - Playable: 可玩格子
  - Fill All: 全部填充为可玩
  - Clear All: 全部清空
  
- **Piece 模式**: 放置和编辑棋子
  - 选择类型: Peg/Gem/Stone
  - 设置是否可移动
  - 点击格子放置
  - 点击已有棋子编辑
  - 棋子列表（可点击选中）

---

## ⚠️ 验证规则

### 必须满足（红色错误）
- ❌ 棋盘尺寸必须在 4x4 到 7x9 之间
- ❌ 至少有 1 个可玩格子
- ❌ 至少有 1 个可移动棋子
- ❌ 所有棋子必须在可玩格子上
- ❌ 同一位置不能有多个棋子
- ❌ 棋子 ID 必须唯一

### 建议优化（黄色警告）
- ⚠️ 可玩格子少于 10 个
- ⚠️ 只有 1 个可移动棋子
- ⚠️ 空格子比例过高

### 优化提示（蓝色信息）
- 💡 棋子总数过少
- 💡 没有 Gem 棋子

---

## 🤖 AI 生成参数说明

### LevelGenerationRequest 结构

```json
{
  "levelId": "生成的关卡ID",
  "targetWidth": 5,         // 目标宽度 4-7
  "targetHeight": 6,        // 目标高度 4-9
  "targetDifficulty": 1,    // 0=Easy, 1=Normal, 2=Hard, 3=Expert
  "sceneType": 0,           // 0=Forest, 1=Desert, 2=Ice, 3=Castle
  "constraints": {
    "minPlayableCells": 10,
    "maxPlayableCells": 63,
    "minPieceCount": 3,
    "maxPieceCount": 20,
    "movablePieceRatio": 0.3,  // 可移动棋子占比
    "pieceTypeDistribution": {
      "0": 5,  // Peg 权重
      "1": 3,  // Gem 权重
      "2": 2   // Stone 权重
    }
  }
}
```

### LevelConfig 输出格式

参考 `level_example.json` 文件

---

## 🔥 高级功能

### 批量生成工具
Unity 菜单: **Tools > Batch Level Generator**

1. 设置生成数量（如 10 个）
2. 配置尺寸范围
3. 点击 Generate
4. 自动保存到 Resources/LevelConfigs/

### 撤销/重做
- **Undo**: 工具栏 Undo 按钮
- **Redo**: 工具栏 Redo 按钮
- 支持最多 20 步历史记录

### 保存机制
- **Save**: 保存到当前文件
- **Save As**: 另存为新文件
- 自动创建 `.backup` 备份文件

---

## 💡 使用技巧

1. **快速创建对称棋盘**
   - 使用 "Fill All Playable" 全部填充
   - 切换到 Board 模式，选择 Void
   - 手动擦除不需要的格子

2. **批量修改棋子属性**
   - 在 Piece 模式下
   - 点击棋子列表选中
   - 在右侧修改属性

3. **验证驱动开发**
   - 先绘制棋盘
   - 查看验证提示
   - 根据提示添加棋子
   - 实时调整直到无错误

4. **AI 生成后微调**
   - 使用 AI 生成基础布局
   - 手动调整特定位置
   - 修改难度相关的细节

---

## 🐛 常见问题

**Q: 保存时提示验证失败**
A: 检查左侧验证面板的红色错误，必须全部解决才能保存

**Q: 调整尺寸后棋子消失**
A: 超出新尺寸范围的棋子会被自动删除，建议先设置好尺寸再放置棋子

**Q: AI 生成的 JSON 无法加载**
A: 确保 JSON 格式完整，特别是 board.rows 必须包含所有行列数据

**Q: 找不到已保存的关卡**
A: 检查 `Assets/Resources/LevelConfigs/` 目录，文件名必须与 levelId 一致

---

## 📚 相关文件

- `README.md`: 详细使用文档
- `level_example.json`: 示例关卡配置
- `level.editor.asmdef`: 编辑器程序集定义

---

## 🎮 开始创作吧！

1. 打开 `Tools > Level Editor`
2. 加载 `level_example` 查看示例
3. 创建你的第一个关卡
4. 使用 AI 快速生成多个变体
5. 在游戏中测试

祝创作愉快！🚀
