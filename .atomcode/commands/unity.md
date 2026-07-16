---
name: unity
description: 连接并操作 Unity Editor (MMUnityMcp MCP)
---

# /unity 命令

## 用法
```
/unity              → 检查连接状态并连接 Unity Editor
/unity <操作>       → 执行指定操作
```

## 支持的操作
| 参数 | 说明 |
|------|------|
| `status` | 检查 Unity 连接状态 |
| `info` | 获取当前场景信息 |
| `scene` | 列出场景中的对象 |
| `console` | 查看控制台日志 |
| `help` | 显示所有可用工具 |

## 执行流程
1. 通过 WebSocket 连接 `ws://localhost:8090/McpUnity`
2. 发送 JSON-RPC 2.0 请求
3. 返回并显示结果