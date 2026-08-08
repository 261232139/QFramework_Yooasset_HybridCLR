/****************************************************************************
 * 关卡状态机使用示例
 * 
 * 演示如何在游戏中使用关卡状态机和事件系统
 ****************************************************************************/

using UnityEngine;
using Game.Level.State;

namespace Game.Level.Examples
{
    /// <summary>
    /// 关卡UI控制器示例
    /// 展示如何监听关卡事件并更新UI
    /// </summary>
    public class LevelUIExample : MonoBehaviour
    {
        private void OnEnable()
        {
            // 订阅关卡事件
            LevelEventManager.OnLevelEvent += OnLevelEvent;
        }

        private void OnDisable()
        {
            // 取消订阅
            LevelEventManager.OnLevelEvent -= OnLevelEvent;
        }

        private void OnLevelEvent(LevelEventArgs args)
        {
            switch (args.EventType)
            {
                case LevelEventType.LevelLoadStart:
                    Debug.Log($"[UI] 显示加载画面 - Level {args.LevelNumber}");
                    // TODO: 显示加载进度条
                    break;

                case LevelEventType.LevelLoadComplete:
                    Debug.Log("[UI] 隐藏加载画面");
                    // TODO: 隐藏加载进度条
                    break;

                case LevelEventType.LevelReady:
                    Debug.Log("[UI] 显示 'Ready, Go!' 倒计时");
                    // TODO: 播放准备动画
                    break;

                case LevelEventType.LevelStart:
                    Debug.Log("[UI] 显示游戏UI（分数、计时器等）");
                    // TODO: 显示HUD
                    break;

                case LevelEventType.LevelPaused:
                    Debug.Log("[UI] 显示暂停菜单");
                    // TODO: 显示暂停UI
                    break;

                case LevelEventType.LevelResumed:
                    Debug.Log("[UI] 隐藏暂停菜单");
                    // TODO: 隐藏暂停UI
                    break;

                case LevelEventType.LevelWon:
                    Debug.Log($"[UI] 显示胜利界面 - Level {args.LevelNumber}");
                    // TODO: 显示胜利UI，播放动画
                    break;

                case LevelEventType.LevelLost:
                    Debug.Log($"[UI] 显示失败界面 - Level {args.LevelNumber}");
                    // TODO: 显示失败UI
                    break;

                case LevelEventType.ReturnToLobby:
                    Debug.Log("[UI] 准备返回大厅");
                    // TODO: 清理关卡UI
                    break;
            }
        }
    }

    /// <summary>
    /// 关卡音效控制器示例
    /// 展示如何根据关卡事件播放音效
    /// </summary>
    public class LevelAudioExample : MonoBehaviour
    {
        private void OnEnable()
        {
            LevelEventManager.OnLevelEvent += OnLevelEvent;
        }

        private void OnDisable()
        {
            LevelEventManager.OnLevelEvent -= OnLevelEvent;
        }

        private void OnLevelEvent(LevelEventArgs args)
        {
            switch (args.EventType)
            {
                case LevelEventType.LevelStart:
                    Debug.Log("[Audio] 播放背景音乐");
                    // TODO: AudioManager.PlayBGM("level_bgm");
                    break;

                case LevelEventType.LevelWon:
                    Debug.Log("[Audio] 播放胜利音效");
                    // TODO: AudioManager.PlaySFX("victory");
                    break;

                case LevelEventType.LevelLost:
                    Debug.Log("[Audio] 播放失败音效");
                    // TODO: AudioManager.PlaySFX("defeat");
                    break;

                case LevelEventType.ReturnToLobby:
                    Debug.Log("[Audio] 停止背景音乐");
                    // TODO: AudioManager.StopBGM();
                    break;
            }
        }
    }

    /// <summary>
    /// 游戏逻辑控制器示例
    /// 展示如何在游戏中触发状态切换
    /// </summary>
    public class GameLogicExample : MonoBehaviour
    {
        private LevelStateMachine mStateMachine;
        private int mScore = 0;
        private int mTargetScore = 100;

        private void Start()
        {
            mStateMachine = FindFirstObjectByType<LevelStateMachine>();
        }

        private void Update()
        {
            if (mStateMachine == null || !mStateMachine.IsPlaying())
                return;

            // 示例：检测输入
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // 暂停游戏
                mStateMachine.Pause();
            }

            if (Input.GetKeyDown(KeyCode.Space) && mStateMachine.IsPaused())
            {
                // 恢复游戏
                mStateMachine.Resume();
            }

            // 示例：检测胜利条件
            if (mScore >= mTargetScore)
            {
                mStateMachine.Win();
            }

            // 示例：检测失败条件（时间到、生命值为0等）
            // if (time <= 0 || health <= 0)
            // {
            //     mStateMachine.Fail();
            // }
        }

        // 示例：玩家得分
        public void AddScore(int points)
        {
            mScore += points;
            Debug.Log($"[Game] Score: {mScore}/{mTargetScore}");
        }
    }
}

/****************************************************************************
 * 使用说明
 ****************************************************************************/

/*

## 基本流程

1. **大厅进入关卡**
   ```csharp
   LobbyController.Instance.EnterLevel(levelNumber);
   ```
   
   这会触发以下流程：
   - 加载关卡配置
   - 创建LevelStateMachine
   - 启动状态机：LobbyToLevel → LoadLevel → LevelReady → LevelRunning

2. **游戏进行中**
   ```csharp
   var stateMachine = FindFirstObjectByType<LevelStateMachine>();
   
   // 暂停
   stateMachine.Pause();
   
   // 恢复
   stateMachine.Resume();
   
   // 胜利
   stateMachine.Win();
   
   // 失败
   stateMachine.Fail();
   ```

3. **返回大厅**
   状态机会自动处理返回流程，触发 LevelToLobby 状态，
   并最终调用 LobbyController.Instance.ReturnToLobby()

## 事件监听

任何需要响应关卡状态变化的系统都可以监听事件：

```csharp
void OnEnable()
{
    LevelEventManager.OnLevelEvent += OnLevelEvent;
}

void OnDisable()
{
    LevelEventManager.OnLevelEvent -= OnLevelEvent;
}

void OnLevelEvent(LevelEventArgs args)
{
    switch (args.EventType)
    {
        case LevelEventType.LevelStart:
            // 游戏开始
            break;
        case LevelEventType.LevelWon:
            // 游戏胜利
            break;
        // ... 其他事件
    }
}
```

## 状态流程图

```
[大厅] 
  ↓ 点击进关按钮
[LobbyToLevel] ← 过场动画
  ↓
[LoadLevel] ← 加载资源
  ↓
[LevelReady] ← 初始化游戏
  ↓
[LevelRunning] ⇄ [LevelPause] ← 游戏进行
  ↓ (Win/Fail)
[LevelSuccess/LevelFail] ← 结算
  ↓
[LevelToLobby] ← 过场动画
  ↓
[大厅]
```

## 扩展建议

1. **过场动画**：在 StateLobbyToLevel 和 StateLevelToLobby 中实现具体的动画逻辑
2. **倒计时**：在 StateLevelReady 中添加 "3...2...1...GO!" 倒计时
3. **重试功能**：在失败UI中调用 `stateMachine.Retry()`
4. **保存进度**：在 LevelEventType.LevelWon 事件中保存玩家进度
5. **统计数据**：监听事件收集游戏统计（用时、得分等）

*/
