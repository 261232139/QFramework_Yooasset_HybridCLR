---
name: unity-connect
description: 自动连接 Unity Editor (MMUnityMcp MCP WebSocket 服务器)
user_invocable: false
disable_model_invocation: false
---

# 🎮 Unity Editor 自动连接

## 项目信息
- **项目路径**: `D:/Workspace/QFramework/QFramework`
- **Unity 版本**: 2022.3+
- **MCP 服务器**: MMUnityMcp (io.github.codergamester/mcp-unity)

## 连接信息

| 配置项 | 值 |
|--------|-----|
| WebSocket URL | `ws://localhost:8090/McpUnity` |
| 协议 | JSON-RPC 2.0 |
| 请求头 | `X-Client-Name: atomcode` |
| Node.js 位置 | `E:/各种package/MMUnityMcp/Server~/dist/index.js` |

## 自动连接流程

**每次会话开始时，自动执行以下步骤：**

### 1. 检查 Unity Editor 是否运行
```bash
tasklist //FI "IMAGENAME eq Unity.exe" 2>nul | findstr Unity >nul && echo "Unity Running" || echo "Unity Not Running"
```

### 2. 检查 WebSocket 端口 8090 是否监听
```bash
netstat -ano | findstr ":8090.*LISTENING" >nul && echo "Port 8090 Open" || echo "Port 8090 Closed"
```

### 3. 检查 Node.js MCP 客户端是否连接
```bash
netstat -ano | findstr "8090.*ESTABLISHED.*node" >nul && echo "MCP Client Connected" || echo "MCP Client Not Connected"
```

### 4. 发送 WebSocket 连接测试（使用 Node.js）
```javascript
const WebSocket = require('ws');
const ws = new WebSocket('ws://localhost:8090/McpUnity', {
    headers: { 'X-Client-Name': 'atomcode' }
});
ws.on('open', () => {
    ws.send(JSON.stringify({
        jsonrpc: '2.0',
        method: 'get_scene_info',
        params: {},
        id: '1'
    }));
});
ws.on('message', (data) => { ws.close(); });
```

## 可用 MCP 工具

| 工具名 | 用途 |
|--------|------|
| `get_scene_info` | 获取当前场景信息 |
| `get_game_object` | 获取场景对象详情 |
| `create_game_object` | 创建 GameObject (Cube/Sphere/Plane/Capsule/Cylinder/Empty) |
| `update_game_object` | 修改 Transform/名称/激活状态 |
| `delete_game_object` | 删除 GameObject |
| `duplicate_game_object` | 复制 GameObject |
| `reparent_game_object` | 修改父子关系 |
| `move_game_object` | 移动位置 |
| `rotate_game_object` | 旋转 |
| `scale_game_object` | 缩放 |
| `set_transform` | 同时设置位置/旋转/缩放 |
| `get_component` | 获取组件信息 |
| `update_component` | 修改组件属性 |
| `add_component` | 添加组件 |
| `remove_component` | 移除组件 |
| `create_material` | 创建材质 |
| `assign_material` | 分配材质到对象 |
| `modify_material` | 修改材质属性 |
| `get_assets` | 获取资源列表 |
| `add_asset_to_scene` | 将资源添加到场景 |
| `create_prefab` | 创建 Prefab |
| `create_scene` | 创建新场景 |
| `load_scene` | 加载场景 |
| `save_scene` | 保存场景 |
| `delete_scene` | 删除场景 |
| `unload_scene` | 卸载场景 |
| `get_console_logs` | 获取控制台日志 |
| `get_packages` | 获取包列表 |
| `add_package` | 添加包 |
| `get_menu_items` | 获取菜单项 |
| `execute_menu_item` | 执行菜单命令 |
| `capture_screenshot` | 截屏 |
| `run_tests` | 运行测试 |
| `recompile_scripts` | 重新编译脚本 |
| `invoke_skill` | 调用 Unity-Skills (431 个技能) |
| `get_skills_manifest` | 获取技能清单 |
| `batch_execute` | 批量执行多个工具 |
| `manage_ui` | 管理 UI 元素 |
| `adjust_scene_visuals` | 调整场景视觉效果 |
| `adjust_particle_system` | 调整粒子系统 |
| `describe_component` | 描述组件用途 |

## 快捷操作示例

### 查看场景
```json
{"jsonrpc":"2.0","method":"get_scene_info","params":{},"id":"1"}
```

### 创建 Cube
```json
{"jsonrpc":"2.0","method":"create_game_object","params":{"type":"Cube","name":"MyCube","position":{"x":0,"y":1,"z":0}},"id":"1"}
```

### 调用 Unity-Skills
```json
{"jsonrpc":"2.0","method":"invoke_skill","params":{"skill_name":"create_scene","name":"NewScene"},"id":"1"}
```

## 故障排除

| 问题 | 解决方案 |
|------|---------|
| 连接超时 | Unity 未运行或 MCP 服务未启动 → 打开 Unity → Tools → MCP Unity → Start Server |
| 端口被占用 | 检查端口 8090 是否被其他程序占用 |
| WebSocket 断开 | Unity 编译/Domain Reload 期间会短暂断开，等待后重试 |
| 501 错误 | 检查 WebSocket 路径是否为 `/McpUnity` (不是 `/`) |