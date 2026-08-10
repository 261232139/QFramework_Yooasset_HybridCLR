/****************************************************************************
 * PlayerDataController — 玩家数据控制器
 * 
 * 职责：管理玩家数据的加载、保存、更新
 * 使用 EasySave3 进行持久化存储
 * 继承 DataControllerBase<T> 实现单例模式和数据管理
 ****************************************************************************/

using UnityEngine;

namespace HotUpdate.LocalStorageKit
{
    public class PlayerDataController : DataControllerBase<PlayerDataController>
    {
        protected override string SAVE_KEY => "PlayerData";

        public override SaveMode SaveMode => SaveMode.Immediate;

        private PlayerData mPlayerData;

        public PlayerData Data
        {
            get
            {
                if (mPlayerData == null)
                {
                    Load();
                }
                return mPlayerData;
            }
        }

        public int CurrentLevelID
        {
            get => Data.currentLevelID;
            set
            {
                if (Data.currentLevelID != value)
                {
                    Data.currentLevelID = value;
                    MarkDirty();
                }
            }
        }

        private PlayerDataController() { }

        public override void OnSingletonInit()
        {
            base.OnSingletonInit();
            Load();
        }

        public override void Load()
        {
            if (ES3.KeyExists(SAVE_KEY))
            {
                string json = ES3.Load<string>(SAVE_KEY);
                mPlayerData = JsonUtility.FromJson<PlayerData>(json);
                Debug.Log($"[PlayerDataController] 数据加载成功: {mPlayerData}");
            }
            else
            {
                mPlayerData = new PlayerData();
                Debug.Log("[PlayerDataController] 未找到存档，创建新数据");
                MarkDirty();
            }
        }

        public override void Save()
        {
            if (mPlayerData == null) return;
            
            string json = JsonUtility.ToJson(mPlayerData);
            ES3.Save<string>(SAVE_KEY, json);
            Debug.Log($"[PlayerDataController] 数据已保存: {mPlayerData}");
        }

        public void DeleteSave()
        {
            if (ES3.KeyExists(SAVE_KEY))
            {
                ES3.DeleteKey(SAVE_KEY);
                Debug.Log("[PlayerDataController] 存档已删除");
            }
            mPlayerData = new PlayerData();
            ClearDirty();
        }

        public void ResetData()
        {
            mPlayerData.Reset();
            MarkDirty();
            Debug.Log("[PlayerDataController] 数据已重置");
        }

        public void UnlockNextLevel()
        {
            CurrentLevelID++;
            Debug.Log($"[PlayerDataController] 解锁下一关: Level {CurrentLevelID}");
        }

        public bool HasSaveData()
        {
            return ES3.KeyExists(SAVE_KEY);
        }

        public override void Dispose()
        {
            mPlayerData = null;
            base.Dispose();
        }
    }
}
