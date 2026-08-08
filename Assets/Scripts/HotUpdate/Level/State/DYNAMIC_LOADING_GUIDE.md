# StateLoadLevel 动态加载方案

## 实现说明

### 设计原则
✅ **无 Object.Find** - 不使用任何 Find 系列函数
✅ **无 new GameObject** - 不在代码中直接创建 GameObject
✅ **动态加载** - 通过 YooAsset 资源系统加载预制体

---

## 加载流程

```
StateLoadLevel.OnEnter()
    ↓
LoadLevelAsync()
    ↓
1. 获取 YooAsset 包管理器
    ↓
2. 加载 "LevelScene" 预制体 (GameObject)
    ↓
3. 实例化预制体
    ↓
4. 获取 LevelScene 组件
    ↓
5. 调用 LevelScene.LoadLevel()
    ↓
6. 监听关卡事件
    ↓
7. 转到 LevelReady 状态
```

---

## 代码实现

### 关键代码段

```csharp
// 1. 通过 YooAsset 加载预制体
var package = QFramework.YooAssetBridge.DefaultPackage;
var handle = package.LoadAssetAsync<GameObject>("LevelScene");
yield return handle;

// 2. 实例化预制体
var levelScenePrefab = handle.AssetObject as GameObject;
var levelSceneObj = Object.Instantiate(levelScenePrefab);

// 3. 获取组件并使用
var levelScene = levelSceneObj.GetComponent<LevelScene>();
levelScene.LoadLevel(Context.Config, Context.LevelNumber);
```

### 错误处理

- ✅ YooAsset 包未初始化 → 转到 LevelFail
- ✅ 预制体加载失败 → 转到 LevelFail
- ✅ LevelScene 组件缺失 → 转到 LevelFail
- ✅ 同步加载请求 → 报错并转到 LevelFail

---

## YooAsset 资源配置要求

### 预制体要求

**预制体名称**: `LevelScene`
**预制体路径**: 需要在 YooAsset 中配置（例如 `Assets/Game/Level/Prefab/LevelScene.prefab`）

### YooAsset 配置步骤

1. **添加到资源包**
   - 打开 YooAsset 配置窗口
   - 将 `LevelScene.prefab` 添加到资源包
   - 设置资源地址为 `"LevelScene"`

2. **构建资源包**
   - 构建 YooAsset 资源包
   - 确保 LevelScene 预制体被正确打包

3. **运行时加载**
   - 确保 YooAsset 已初始化
   - `QFramework.YooAssetBridge.DefaultPackage` 可用

---

## LevelScene 预制体结构

### 必需组件

```
LevelScene (GameObject)
├── LevelScene (Script) ← 必须
│   ├─ board: Board 引用
│   ├─ inputHandler: LevelInputHandler 引用 (可选)
│   ├─ goalManager: LevelGoalManager 引用 (可选)
│   └─ uiController: LevelUIController 引用 (可选)
│
└── 子对象（根据需要配置）
```

### 最简配置

如果只想快速测试，LevelScene 预制体可以只包含：

```
LevelScene (GameObject)
├── LevelScene (Script)
│   └─ board: 指向 Board 子对象
└── Board (GameObject)
    └─ Board (Script) - 已配置完整
```

---

## 与旧版本的区别

| 方面 | 旧版本 | 新版本 |
|------|--------|--------|
| **查找方式** | Object.FindFirstObjectByType | YooAsset 动态加载 |
| **创建方式** | new GameObject() | 实例化预制体 |
| **兼容性** | 尝试多种方式 | 仅支持预制体加载 |
| **错误处理** | 自动创建降级 | 严格验证，失败即报错 |
| **资源管理** | 无 | YooAsset 托管 |

---

## 优势

### 1. 资源管理规范
✅ 所有资源通过 YooAsset 统一管理
✅ 支持热更新
✅ 支持资源卸载

### 2. 代码清晰
✅ 无运行时创建 GameObject
✅ 无场景依赖
✅ 无 Find 查找

### 3. 性能优化
✅ 预制体可预配置
✅ 避免运行时反射查找
✅ 资源生命周期可控

---

## 使用注意事项

### 必须满足的条件

1. ✅ **YooAsset 已初始化**
   - 在进入关卡前确保 YooAsset 初始化完成

2. ✅ **LevelScene 预制体已配置**
   - 预制体必须包含 LevelScene 组件
   - Board 等子组件需要正确配置

3. ✅ **资源已打包**
   - LevelScene 预制体必须在 YooAsset 资源包中
   - 资源地址为 "LevelScene"

### 调试建议

如果加载失败，检查以下内容：

```
1. Console 日志查看具体错误
   - YooAsset 未初始化？
   - 预制体加载失败？
   - 组件缺失？

2. YooAsset 配置检查
   - LevelScene.prefab 是否在资源包中？
   - 资源地址是否为 "LevelScene"？
   - 资源包是否构建？

3. 预制体配置检查
   - 是否有 LevelScene 组件？
   - Board 引用是否配置？
   - 子组件是否完整？
```

---

## 扩展：配置资源地址

如果需要自定义资源地址，可以修改：

```csharp
// 当前硬编码
var levelScenePrefabName = "LevelScene";

// 可改为配置化
var levelScenePrefabName = GameConfig.LevelScenePrefabAddress;

// 或根据关卡类型动态选择
var levelScenePrefabName = GetLevelScenePrefabName(Context.Config.sceneType);
```

---

## 总结

新的实现方式：
- ✅ **完全动态加载** - 通过 YooAsset
- ✅ **无运行时创建** - 只实例化预制体
- ✅ **无场景查找** - 不依赖 Find
- ✅ **规范化管理** - 资源统一管理

现在只需要确保 LevelScene 预制体在 YooAsset 中正确配置即可！
