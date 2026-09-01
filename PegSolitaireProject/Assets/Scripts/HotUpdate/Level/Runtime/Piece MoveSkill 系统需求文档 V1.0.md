# Piece MoveSkill 系统需求文档 V1.0

## 1. 目标

重构/实现棋子的移动能力系统。

核心设计原则：

> 一个棋子的移动能力不由单一 `MoveType` 决定，而是由多个独立、原子化的 `MoveSkill` 组合决定。

例如：

```text
普通棋子：
JumpUp + JumpDown + JumpLeft + JumpRight

只能上下移动：
JumpUp + JumpDown

只能左右移动：
JumpLeft + JumpRight

不可移动棋子：
无 MoveSkill
```

未来不同动物棋子可以通过组合不同 MoveSkill 获得不同移动能力。

要求：

- 使用组合方式设计，不使用大量 PieceType / MoveType 枚举硬编码。
- MoveSkill 与棋子其他属性分离。
- 数据逻辑与 UI 表现分离。
- 优先适配项目当前已有架构，不重复创建职责相同的系统。
- V1 只实现当前明确需要的能力，不提前实现复杂技能系统。

---

# 2. 当前基础移动规则

当前游戏核心移动规则基于 Jump。

一次标准移动：

```text
From    Middle    Target

 ●        ●         ○
```

From 棋子跨越 Middle 棋子，到达 Target：

```text
 ○        ○         ●
```

移动结果：

1. From 位置变为空。
2. MovingPiece 移动到 Target。
3. Middle 棋子被移除。
4. 如果 Middle 是 RescueTarget，则执行目标解救逻辑。

标准 Jump：

```text
移动距离 = 2 Cell
跨越棋子 = 1
目标位置 = 空 Cell
```

---

# 3. MoveSkill 设计

## 3.1 MoveSkill 职责

MoveSkill 只负责：

> 描述并计算棋子自身能够进行的移动。

MoveSkill 不负责：

- UI
- 动画
- 胜负判断
- Rescue UI
- 关卡流程
- 棋子美术
- 点击输入

MoveSkill 应能够根据：

```text
Board
Piece
CurrentPosition
```

计算该 Skill 当前产生的合法移动。

---

# 4. V1 MoveSkill

V1 只实现以下四种原子移动能力：

```text
JumpUp
JumpDown
JumpLeft
JumpRight
```

每个 Skill 只负责一个方向。

---

## 4.1 JumpUp

允许棋子：

```text
↑
```

跳跃。

假设：

```text
Target
  ○

Middle
  ●

From
  ●
```

满足：

```text
Middle = 可被跨越的棋子
Target = 有效空 Cell
```

则该移动合法。

---

## 4.2 JumpDown

允许：

```text
↓
```

规则同 JumpUp。

---

## 4.3 JumpLeft

允许：

```text
←
```

规则同上。

---

## 4.4 JumpRight

允许：

```text
→
```

规则同上。

---

# 5. MoveSkill 组合

每个 Piece 可以拥有：

```text
0 ~ N 个 MoveSkill
```

例如：

### Normal Piece

```text
MoveSkills:

JumpUp
JumpDown
JumpLeft
JumpRight
```

表现：

```text
   ↑
←  ●  →
   ↓
```

---

### Vertical Piece

```text
MoveSkills:

JumpUp
JumpDown
```

表现：

```text
↑
●
↓
```

---

### Horizontal Piece

```text
MoveSkills:

JumpLeft
JumpRight
```

表现：

```text
← ● →
```

---

### UpOnly Piece

```text
MoveSkills:

JumpUp
```

---

### Fixed Piece

```text
MoveSkills:

[]
```

没有任何 MoveSkill：

> 棋子自身不可移动。

不要额外创建：

```text
CannotMoveSkill
FixedMoveSkill
```

用于表达不可移动。

---

# 6. MoveSkill 与 PieceTrait 分离

非常重要：

> “自己怎么移动”和“别人能对自己做什么”是两个不同维度。

不要全部放进 MoveSkill。

例如：

```text
不可移动，但允许被其他棋子跨越
```

应该表示为：

```text
MoveSkills = []

CanBeJumped = true
```

而不是创建：

```text
FixedButCanBeJumpedSkill
```

---

# 7. V1 PieceTrait

V1 至少需要支持：

```text
CanBeJumped
IsRescueTarget
```

---

## 7.1 CanBeJumped

表示：

> 其他棋子能否跨越该棋子。

例如：

### 普通棋子

```text
CanBeJumped = true
```

### 固定但可以被跨越的棋子

```text
MoveSkills = []
CanBeJumped = true
```

### 障碍棋子

如果未来需要：

```text
MoveSkills = []
CanBeJumped = false
```

注意：

V1 不需要实现复杂障碍行为，只需要结构允许表达这个属性。

---

# 8. IsRescueTarget

表示：

> 当前棋子是否属于关卡解救目标。

例如：

```text
Panda

MoveSkills = []
CanBeJumped = true
IsRescueTarget = true
```

当其他棋子合法跨越 Panda：

```text
🐱 🐼 ○
```

执行：

```text
○ ○ 🐱
```

同时：

```text
RemainingRescueTargetCount -= 1
```

Panda 从棋盘离开，并通知关卡目标系统。

当：

```text
RemainingRescueTargetCount == 0
```

关卡胜利。

MoveSkill 本身不要直接操作 Victory UI。

应该通过现有 GameController / LevelController / ObjectiveSystem 等系统处理。

---

# 9. 推荐逻辑结构

具体类名可以根据项目现有代码调整。

不要为了匹配本文类名而重复创建已有职责。

建议逻辑关系：

```text
Piece
│
├── PieceConfig
│
├── MoveSkills[]
│
└── Traits
      ├── CanBeJumped
      └── IsRescueTarget


MoveSkill
│
├── JumpUp
├── JumpDown
├── JumpLeft
└── JumpRight


Board
│
├── Cell
├── Piece
└── BoardState


MoveSystem / MoveValidator
│
├── GetValidMoves()
├── CanMove()
└── ExecuteMove()
```

---

# 10. MoveSkill 接口建议

可以采用类似：

```csharp
public interface IMoveSkill
{
    IEnumerable<MoveOption> GetValidMoves(
        Board board,
        Piece piece
    );
}
```

具体签名根据当前项目架构调整。

核心要求：

> GameController 不应该通过判断 PieceType 来决定棋子如何移动。

禁止出现大量类似：

```csharp
if (piece.Type == PieceType.Normal)
{
    // 上下左右
}
else if (piece.Type == PieceType.Vertical)
{
    // 上下
}
else if (piece.Type == PieceType.Horizontal)
{
    // 左右
}
```

应该：

```text
遍历 Piece.MoveSkills
        ↓
每个 Skill 返回合法移动
        ↓
合并为该棋子的全部 LegalMoves
```

伪代码：

```csharp
foreach (var skill in piece.MoveSkills)
{
    moves.AddRange(
        skill.GetValidMoves(board, piece)
    );
}
```

---

# 11. MoveOption

建议使用统一的数据结构描述一次合法移动。

例如：

```csharp
public class MoveOption
{
    public Vector2Int from;
    public Vector2Int middle;
    public Vector2Int target;
}
```

或者使用项目已有坐标/Cell结构。

MoveOption 至少需要表达：

```text
起点
被跨越位置
目标位置
```

这样表现层可以直接知道：

- 哪个棋子移动
- 哪个棋子需要离场
- 移动到哪里

---

# 12. 标准 Jump 合法性

对于任意方向的标准 Jump：

必须满足：

### From

```text
存在 MovingPiece
```

### Middle

```text
存在 Piece
并且：
CanBeJumped == true
```

### Target

```text
Cell 有效
并且为空
```

满足以上条件：

```text
MoveOption = Valid
```

否则：

```text
Invalid
```

---

# 13. 移动执行

一次合法移动：

```text
ExecuteMove(MoveOption)
```

建议流程：

```text
锁定输入
    ↓
再次验证 MoveOption
    ↓
From 移除 MovingPiece
    ↓
Middle 移除 JumpedPiece
    ↓
Target 放入 MovingPiece
    ↓
如果 JumpedPiece.IsRescueTarget
    ↓
通知 ObjectiveSystem
    ↓
同步/播放表现
    ↓
检测关卡状态
    ↓
解除输入锁定
```

具体“先更新数据还是先播放动画”按照项目现有架构处理，但必须保证：

> 动画过程中不能产生第二次移动导致 BoardState 与 View 不一致。

---

# 14. UI交互

当前操作方式：

```text
Tap Piece
    ↓
GetValidMoves(piece)
    ↓
显示合法 Target
    ↓
Tap Target
    ↓
ExecuteMove()
```

MoveSkill 不处理 UI。

UI 层只消费：

```text
GetValidMoves()
```

返回的结果。

例如：

```text
        ★
        ●
★   ●   ●   ●   ★
        ●
        ★
```

其中：

```text
★ = 当前棋子的合法 Target
```

具体高亮样式由表现层处理。

---

# 15. PieceConfig

如果项目当前适合使用 ScriptableObject，可以使用类似：

```csharp
[CreateAssetMenu]
public class PieceConfig : ScriptableObject
{
    public string pieceId;

    public Sprite icon;

    public List<MoveSkillConfig> moveSkills;

    public bool canBeJumped;

    public bool isRescueTarget;
}
```

具体实现允许根据当前项目架构调整。

目标是做到：

> 新增一个棋子类型时，优先通过配置组合 MoveSkill，而不是修改 GameController。

---

# 16. 示例棋子配置

## Piece A：普通棋子

```text
MoveSkills:
- JumpUp
- JumpDown
- JumpLeft
- JumpRight

CanBeJumped:
true

IsRescueTarget:
false
```

---

## Piece B：垂直棋子

```text
MoveSkills:
- JumpUp
- JumpDown

CanBeJumped:
true

IsRescueTarget:
false
```

---

## Piece C：水平棋子

```text
MoveSkills:
- JumpLeft
- JumpRight

CanBeJumped:
true

IsRescueTarget:
false
```

---

## Piece D：不可移动棋子

```text
MoveSkills:
[]

CanBeJumped:
true

IsRescueTarget:
false
```

效果：

> 自己不能移动，但是其他棋子可以跨越并移除它。

---

## Piece E：解救目标

```text
MoveSkills:
[]

CanBeJumped:
true

IsRescueTarget:
true
```

效果：

> 自己不能移动。

> 可以被其他棋子跨越。

> 被跨越后视为完成一次 Rescue。

---

# 17. 后续扩展方向

当前版本不要实现，但架构不能明显阻止未来加入：

```text
DiagonalJump
LongJump
MultiJump
JumpOverObstacle
CannotBeJumped
Locked
Teleport
```

未来也可能出现：

```text
JumpUp
+
JumpDown
+
DiagonalJump
```

这样的组合。

因此：

> 不要让棋子类型和移动规则形成一对一绑定。

---

# 18. 不要过度抽象

虽然需要考虑扩展性，但 V1 禁止为了未来可能存在的功能提前实现复杂框架。

V1 不需要：

- Buff System
- Ability System
- Skill Tree
- ECS
- Gameplay Ability System
- 技能优先级
- 技能冷却
- 技能触发器
- 技能连锁
- Runtime Skill Editor

当前 MoveSkill 本质上只是：

> **棋子的原子移动规则组件。**

保持简单。

---

# 19. V1 验收测试

至少验证以下情况。

### Case 1：普通棋子

```text
JumpUp
JumpDown
JumpLeft
JumpRight
```

可以正确返回四个方向中实际合法的移动。

---

### Case 2：垂直棋子

```text
JumpUp
JumpDown
```

绝对不能产生：

```text
JumpLeft
JumpRight
```

---

### Case 3：水平棋子

```text
JumpLeft
JumpRight
```

绝对不能产生：

```text
JumpUp
JumpDown
```

---

### Case 4：不可移动棋子

```text
MoveSkills = []
CanBeJumped = true
```

结果：

```text
GetValidMoves() == Empty
```

但其他棋子：

```text
可以跨越该棋子。
```

---

### Case 5：不可被跨越

```text
CanBeJumped = false
```

即使：

```text
From = 有棋子
Middle = 当前棋子
Target = 空
```

也必须判定：

```text
Invalid Move
```

---

### Case 6：Rescue Target

```text
IsRescueTarget = true
CanBeJumped = true
```

被合法跨越：

```text
RescueTarget 从棋盘移除
RemainingRescueTargetCount - 1
```

最后一个 RescueTarget 被解救：

```text
Victory
```

---

# 20. Codex 实施要求

开始修改代码之前：

1. 先检查当前项目已有 Board、Piece、Cell、Move、GameController 等相关实现。
2. 尽量复用现有架构。
3. 不要重复创建已有职责的类。
4. 不要修改与本需求无关的系统。
5. 如果现有代码与本文建议类名不同，优先适配现有项目，而不是强制重命名。
6. 保证现有基础棋子移动功能不被破坏。
7. MoveSkill 必须与 UI / 动画解耦。
8. 完成后列出新增和修改的文件。
9. 说明每个文件的职责。
10. 给出 V1 验收结果以及尚未实现的扩展项。

最终目标：

> **通过组合多个原子 MoveSkill 定义一个棋子的移动能力，并允许同一套移动系统支持普通棋子、方向受限棋子、不可移动棋子以及 Rescue Target。**