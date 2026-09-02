using Game.Level.Data;
using Game.Level.State;
using QFramework;
using UnityEngine;
using HotUpdate.LocalStorageKit;

namespace HotUpdate.UI
{
    public sealed class LobbyController
    {
        private const string LobbyPrefabLocation = "LobbyUI";
        private static readonly LobbyController sInstance = new LobbyController();

        public static LobbyController Instance => sInstance;
        
        public int CurrentLevel => PlayerDataController.Instance.CurrentLevelID;

        private bool mIsLoadingLevel;
        private bool mStaminaConsumedForCurrentLevel;
        private MonoBehaviour mCoroutineHost;
        private LevelStateMachine mLevelStateMachine;

        private LobbyController()
        {
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

        public void EnterLevel(int levelNumber)
        {
            if (mIsLoadingLevel)
            {
                Debug.LogWarning("[LobbyController] Already loading a level");
                return;
            }

            levelNumber = Mathf.Max(1, levelNumber);
            mIsLoadingLevel = true;
            
            UIKit.HidePanel<LobbyPanel>();
            
            if (mCoroutineHost == null)
            {
                Debug.LogError("[LobbyController] Coroutine host is missing.");
                mIsLoadingLevel = false;
                Open();
                return;
            }

            mCoroutineHost.StartCoroutine(LoadAndStartLevel(levelNumber));
        }

        public void ReturnToLobby()
        {
            mIsLoadingLevel = false;
            
            if (mLevelStateMachine != null)
            {
                Object.Destroy(mLevelStateMachine.gameObject);
                mLevelStateMachine = null;
            }
            
            Open();
            Debug.Log("[LobbyController] Returned to lobby");
        }

        public void CompleteLevel()
        {
            if (mStaminaConsumedForCurrentLevel)
            {
                PlayerDataController.Instance.RefundStaminaForLevel();
                mStaminaConsumedForCurrentLevel = false;
            }

            PlayerDataController.Instance.UnlockNextLevel();
            Debug.Log($"[LobbyController] Level completed! Next level: {CurrentLevel}");
        }

        public void FailLevel()
        {
            mStaminaConsumedForCurrentLevel = false;
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

            if (!PlayerDataController.Instance.TryConsumeStaminaForLevel())
            {
                Debug.LogWarning("[LobbyController] Not enough stamina to enter level.");
                mIsLoadingLevel = false;
                ReturnToLobby();
                yield break;
            }

            mStaminaConsumedForCurrentLevel = !PlayerDataController.Instance.HasUnlimitedStamina;

            mLevelStateMachine = Object.FindFirstObjectByType<LevelStateMachine>();
            if (mLevelStateMachine == null)
            {
                var stateMachineObject = new GameObject("LevelStateMachine");
                mLevelStateMachine = stateMachineObject.AddComponent<LevelStateMachine>();
            }

            mLevelStateMachine.Begin(config, levelNumber, mCoroutineHost);
            mIsLoadingLevel = false;
            
            Debug.Log($"[LobbyController] Level {levelNumber} started: {config.levelId}");
        }

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
                    Debug.Log("[LobbyController] Level returned to lobby event received");
                    break;
            }
        }
    }
}
