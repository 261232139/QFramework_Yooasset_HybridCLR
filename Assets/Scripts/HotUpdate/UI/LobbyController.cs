using Game.Level.Data;
using Game.Level.State;
using QFramework;
using UnityEngine;

namespace HotUpdate.UI
{
    public sealed class LobbyController
    {
        private const string LobbyPrefabLocation = "LobbyUI";
        private static readonly LobbyController sInstance = new LobbyController();

        public static LobbyController Instance => sInstance;
        public int CurrentLevel { get; private set; } = 1;

        private bool mIsLoadingLevel;
        private MonoBehaviour mCoroutineHost;
        private LevelStateMachine mLevelStateMachine;

        private LobbyController()
        {
            // 监听关卡事件
            LevelEventManager.OnLevelEvent += OnLevelEvent;
        }

        public void Open(MonoBehaviour coroutineHost = null)
        {
            if (coroutineHost != null)
                mCoroutineHost = coroutineHost;

            var data = new LobbyPanelData(CurrentLevel, EnterLevel);
            var panel = UIKit.OpenPanel<LobbyPanel>(UILevel.Common, data, prefabName: LobbyPrefabLocation);
            if (panel == null)
                Debug.LogError($"[LobbyController] Failed to open UIKit panel '{LobbyPrefabLocation}'.");
            else
                Debug.Log("[LobbyController] Lobby opened");
        }

        /// <summary>
        /// 进入关卡（从大厅按钮触发）
        /// </summary>
        public void EnterLevel(int levelNumber)
        {
            if (mIsLoadingLevel)
            {
                Debug.LogWarning("[LobbyController] Already loading a level");
                return;
            }

            CurrentLevel = Mathf.Max(1, levelNumber);
            mIsLoadingLevel = true;
            
            // 隐藏大厅UI
            UIKit.HidePanel<LobbyPanel>();
            
            if (mCoroutineHost == null)
            {
                Debug.LogError("[LobbyController] Coroutine host is missing.");
                mIsLoadingLevel = false;
                Open();
                return;
            }

            // 开始加载关卡
            mCoroutineHost.StartCoroutine(LoadAndStartLevel(CurrentLevel));
        }

        /// <summary>
        /// 返回大厅（从关卡结束后调用）
        /// </summary>
        public void ReturnToLobby()
        {
            mIsLoadingLevel = false;
            
            // 清理状态机引用
            if (mLevelStateMachine != null)
            {
                Object.Destroy(mLevelStateMachine.gameObject);
                mLevelStateMachine = null;
            }
            
            // 重新打开大厅UI
            Open();
            Debug.Log("[LobbyController] Returned to lobby");
        }

        /// <summary>
        /// 关卡完成（胜利）
        /// </summary>
        public void CompleteLevel()
        {
            CurrentLevel++;
            Debug.Log($"[LobbyController] Level completed! Next level: {CurrentLevel}");
        }

        /// <summary>
        /// 关卡失败
        /// </summary>
        public void FailLevel()
        {
            Debug.Log($"[LobbyController] Level {CurrentLevel} failed");
        }

        private System.Collections.IEnumerator LoadAndStartLevel(int levelNumber)
        {
            var levelId = $"level_{levelNumber:D3}";
            LevelConfig config = null;
            
            Debug.Log($"[LobbyController] Loading level config: {levelId}");
            yield return LevelConfigLoader.LoadAsync(levelId, result => config = result);

            if (config == null)
            {
                Debug.LogError($"[LobbyController] Failed to load level '{levelId}'.");
                mIsLoadingLevel = false;
                ReturnToLobby();
                yield break;
            }

            // 创建或获取状态机
            mLevelStateMachine = Object.FindFirstObjectByType<LevelStateMachine>();
            if (mLevelStateMachine == null)
            {
                var stateMachineObject = new GameObject("LevelStateMachine");
                mLevelStateMachine = stateMachineObject.AddComponent<LevelStateMachine>();
            }

            // 启动关卡状态机
            mLevelStateMachine.Begin(config, levelNumber, mCoroutineHost);
            mIsLoadingLevel = false;
            
            Debug.Log($"[LobbyController] Level {levelNumber} started: {config.levelId}");
        }

        /// <summary>
        /// 处理关卡事件
        /// </summary>
        private void OnLevelEvent(LevelEventArgs args)
        {
            switch (args.EventType)
            {
                case LevelEventType.LevelWon:
                    CompleteLevel();
                    break;
                    
                case LevelEventType.LevelLost:
                    FailLevel();
                    break;
                    
                case LevelEventType.ReturnedToLobby:
                    // 关卡已返回大厅，可以做一些清理工作
                    Debug.Log("[LobbyController] Level returned to lobby event received");
                    break;
            }
        }
    }
}
