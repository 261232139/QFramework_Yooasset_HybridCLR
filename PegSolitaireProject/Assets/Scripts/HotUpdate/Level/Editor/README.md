# 关卡编辑器使用指南

## 功能概述

关卡编辑器是一个完整的可视化工具，用于创建和编辑 HoldemGem 游戏的关卡配置。支持手动编辑和 AI 辅助生成。

## 打开编辑器

Unity 菜单: `Tools > Level Editor`

## 界面布局

### 左侧面板 - 配置与验证
- **基础信息**: Level ID, 场景类型, 难度
- **棋盘尺寸**: 宽度 4-7, 高度 4-9
- **统计信息**: 实时显示格子数、棋子数
- **验证结果**: 实时显示错误、警告和建议

### 中央面板 - 可视化编辑区
- **网格视图**: 显示棋盘布局
- **缩放**: 鼠标滚轮缩放视图
- **交互**: 点击格子进行编辑
- **颜色说明**:
  - 白色: 可玩格子 (Playable)
  - 灰色: 空格子 (Void)
  - 橙色: Peg 棋子
  - 蓝色: Gem 棋子
  - 灰色: Stone 棋子
  - `>X<` 标记: 可移动棋子

### 右侧面板 - 工具栏
- **模式切换**: Board 模式 / Piece 模式
- **Board 模式工具**: 绘制格子类型
- **Piece 模式工具**: 放置和编辑棋子

## 工作流程

### 1. 手动创建关卡

1. 点击 `New` 创建新关卡
2. 设置基础信息 (Level ID, 难度等)
3. 调整棋盘尺寸
4. 切换到 **Board 模式**:
   - 选择 Playable/Void
   - 点击格子绘制
   - 使用 "Fill All" 快速填充
5. 切换到 **Piece 模式**:
   - 选择棋子类型和属性
   - 点击格子放置棋子
   - 点击已有棋子进行编辑
   - 右侧列表查看所有棋子
6. 查看左侧验证结果，确保无错误
7. 点击 `Save` 保存关卡

### 2. AI 辅助生成关卡 (Cursor)

#### 方法 A: 使用参数生成

1. 点击工具栏 `AI Generate` 按钮
2. 配置生成参数:
   - Board Size: 目标宽高
   - Difficulty: 难度等级
   - Constraints: 约束条件
3. 点击 `Copy Parameters to Clipboard`
4. **参数会复制到剪贴板，弹窗提示包含完整说明**
5. 在 Cursor Chat 中粘贴参数，要求 Cursor 生成 LevelConfig JSON
6. 回到编辑器，点击 `Show JSON Input`
7. 粘贴 Cursor 生成的 JSON
8. 点击 `Load from JSON`
9. 关卡加载成功，可继续手动微调
10. 保存关卡

#### 方法 B: 内置随机生成器

1. 打开 `AI Generate` 窗口
2. 配置参数
3. 点击 `Use Random Generator`
4. 立即生成并加载关卡

### 3. 批量生成关卡

Unity 菜单: `Tools > Batch Level Generator`

1. 设置生成数量
2. 配置尺寸范围
3. 点击 `Generate`
4. 自动保存到 `Assets/Resources/LevelConfigs/`

## AI 生成接口规范

### 输入参数格式 (LevelGenerationRequest)

```json
{
  "levelId": "ai_gen_20260808_120000",
  "minWidth": 4,
  "maxWidth": 7,
  "minHeight": 4,
  "maxHeight": 9,
  "targetWidth": 5,
  "targetHeight": 6,
  "targetDifficulty": 1,
  "sceneType": 0,
  "constraints": {
    "minPlayableCells": 10,
    "maxPlayableCells": 63,
    "minPieceCount": 3,
    "maxPieceCount": 20,
    "movablePieceRatio": 0.3,
    "pieceTypeDistribution": {
      "0": 5,
      "1": 3,
      "2": 2
    }
  }
}
```

### 输出格式 (LevelConfig)

```json
{
  "schemaVersion": 1,
  "levelId": "ai_gen_20260808_120000",
  "sceneType": 0,
  "difficulty": 1,
  "board": {
    "width": 5,
    "height": 6,
    "rows": [
      {
        "cells": [
          {"cellType": 1},
          {"cellType": 1},
          {"cellType": 0},
          {"cellType": 1},
          {"cellType": 1}
        ]
      },
      // ... 其他行
    ]
  },
  "pieces": [
    {
      "id": "peg_000",
      "pieceType": 0,
      "isMovable": true,
      "position": {"x": 0, "y": 0}
    },
    // ... 其他棋子
  ]
}
```

### 枚举值说明

- **SceneType**: 0=Forest, 1=Desert, 2=Ice, 3=Castle
- **LevelDifficulty**: 0=Easy, 1=Normal, 2=Hard, 3=Expert
- **BoardCellType**: 0=Void, 1=Playable
- **PieceType**: 0=Peg, 1=Gem, 2=Stone

### 验证规则

生成的关卡必须满足:
- ✅ Board 宽度 4-7, 高度 4-9
- ✅ 至少 1 个可玩格子
- ✅ 至少 1 个可移动棋子
- ✅ 所有棋子必须在可玩格子上
- ✅ 棋子位置不能重复
- ✅ 棋子 ID 必须唯一

## 快捷键

- **Ctrl+Z**: 撤销
- **Ctrl+Y**: 重做
- **滚轮**: 缩放视图

## 文件位置

- 关卡配置: `Assets/Resources/LevelConfigs/{levelId}.json`
- 备份文件: `Assets/Resources/LevelConfigs/{levelId}.backup`

## 注意事项

1. 保存前会自动创建备份文件
2. 验证有错误时无法保存
3. 调整棋盘尺寸会删除超出范围的棋子
4. Level ID 必须与文件名一致

## 示例关卡 JSON

参考项目中现有的关卡配置文件，或使用随机生成器创建示例。
