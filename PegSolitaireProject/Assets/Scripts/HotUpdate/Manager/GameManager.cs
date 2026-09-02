/****************************************************************************
 * GameManager — 游戏管理器
 * 
 * 职责：
 * 1. 管理游戏的整体生命周期和初始化流程
 * 2. 提供各个游戏模块的全局访问入口
 * 3. 协调各个管理器之间的交互
 * 
 * 使用 QFramework.Singleton<T> 实现单例模式
 ****************************************************************************/

using UnityEngine;
using QFramework;
using Game.Config;
using HotUpdate.LocalStorageKit;

namespace HotUpdate.Game
{
    public class GameManager : Singleton<GameManager>
    {
        private bool mIsInitialized = false;

        public PlayerDataController PlayerData => PlayerDataController.Instance;

        public GameSettingsController GameSettings => GameSettingsController.Instance;

        public OneOffEventCtrl OneOffEvents => OneOffEventCtrl.Instance;

        public SaveManager SaveManager => SaveManager.Instance;

        private GameManager() { }

        public override void OnSingletonInit()
        {

        }

        public void Init()
        {
            if (mIsInitialized)
            {
                Debug.LogWarning("[GameManager] 已经初始化过，跳过重复初始化");
                return;
            }

            Debug.Log("[GameManager] 开始初始化游戏系统...");

            InitializeModules();

            mIsInitialized = true;
            Debug.Log("[GameManager] 游戏系统初始化完成");
        }

        private void InitializeModules()
        {
            InitializeConfigSystem();
            InitializeStorageSystem();
            InitializeGameSettings();
            InitializePlayerData();
            InitializeOneOffEvents();
            InitializeIap();
        }

        private void InitializeConfigSystem()
        {
            ConfigManager.Instance.Initialize();
            Debug.Log("[GameManager] 配置系统初始化完成");
        }
        private void InitializeStorageSystem()
        {
            var saveManager = SaveManager.Instance;
            Debug.Log("[GameManager] 存储系统初始化完成");
        }

        private void InitializeGameSettings()
        {
            var settings = GameSettings;
            ApplyGameSettings(settings);
            Debug.Log("[GameManager] 游戏设置初始化完成");
        }

        private void InitializePlayerData()
        {
            var playerData = PlayerData;
            Debug.Log($"[GameManager] 玩家数据初始化完成 - 当前关卡: {playerData.CurrentLevelID}");
        }

        private void InitializeIap()
        {
            IAPManager.Instance.InitializePurchasing();
        }

        private void InitializeOneOffEvents()
        {
            var oneOffEvents = OneOffEvents;
            Debug.Log("[GameManager] 一次性事件系统初始化完成");
        }

        private void ApplyGameSettings(GameSettingsController settings)
        {
            AudioListener.volume = settings.MusicVolume;
            QualitySettings.SetQualityLevel(settings.QualityLevel, true);

            Debug.Log($"[GameManager] 应用游戏设置 - 音量: {settings.MusicVolume}, 画质: {settings.QualityLevel}");
        }

        public void SaveAllData()
        {
            SaveManager.SaveAll();
            Debug.Log("[GameManager] 所有数据已保存");
        }

        public void ResetPlayerProgress()
        {
            PlayerData.ResetData();
            Debug.Log("[GameManager] 玩家进度已重置");
        }

        public void ResetGameSettings()
        {
            GameSettings.ResetToDefault();
            ApplyGameSettings(GameSettings);
            Debug.Log("[GameManager] 游戏设置已重置为默认值");
        }

        public override void Dispose()
        {
            Debug.Log("[GameManager] 清理游戏管理器");
            SaveAllData();
            mIsInitialized = false;
            base.Dispose();
        }
    }
}
