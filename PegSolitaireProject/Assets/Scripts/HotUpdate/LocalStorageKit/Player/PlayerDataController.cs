/****************************************************************************
 * PlayerDataController — 玩家数据控制器
 * 
 * 职责：管理玩家数据的加载、保存、更新
 * 使用 EasySave3 进行持久化存储
 * 继承 DataControllerBase<T> 实现单例模式和数据管理
 ****************************************************************************/

using System;
using HotUpdate.Utils;
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

        public int CurrentStamina
        {
            get
            {
                RefreshStamina();
                return Data.hasUnlimitedStamina ? PlayerData.MaxStamina : Data.stamina;
            }
        }

        public int Coins => Data.hasUnlimitedCoins ? int.MaxValue : Data.coins;

        public bool HasUnlimitedStamina => Data.hasUnlimitedStamina;

        public bool HasUnlimitedCoins => Data.hasUnlimitedCoins;

        private PlayerDataController() { }

        public override void OnSingletonInit()
        {
            base.OnSingletonInit();
            Load();
        }

        public override void Load()
        {
            try
            {
                if (ES3.KeyExists(SAVE_KEY))
                {
                    string json = ES3.Load<string>(SAVE_KEY);
                    mPlayerData = JsonUtility.FromJson<PlayerData>(json);
                    if (mPlayerData == null)
                    {
                        throw new InvalidOperationException("Player data JSON is empty or invalid.");
                    }

                    if (mPlayerData.Normalize())
                    {
                        MarkDirty();
                    }

                    Debug.Log($"[PlayerDataController] 数据加载成功: {mPlayerData}");
                    return;
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"[PlayerDataController] 读取存档失败，将使用默认数据。{exception}");
            }

            mPlayerData = new PlayerData();
            Debug.Log("[PlayerDataController] 未找到有效存档，创建新数据");
            MarkDirty();
        }

        public override void Save()
        {
            if (mPlayerData == null) return;
            
            string json = JsonUtility.ToJson(mPlayerData);
            ES3.Save<string>(SAVE_KEY, json);
            Debug.Log($"[PlayerDataController] 数据已保存: {mPlayerData}");
        }

        public override void OnUpdate()
        {
            RefreshStamina();
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

        public bool TryConsumeStaminaForLevel()
        {
            RefreshStamina();
            if (Data.hasUnlimitedStamina)
            {
                return true;
            }

            if (Data.stamina <= 0)
            {
                return false;
            }

            Data.stamina--;
            Data.lastStaminaRecoveryUtcTicks = TimeUtil.UtcNow().Ticks;
            MarkDirty();
            return true;
        }

        public void RefundStaminaForLevel()
        {
            if (Data.hasUnlimitedStamina || Data.stamina >= PlayerData.MaxStamina)
            {
                return;
            }

            Data.stamina++;
            if (Data.stamina >= PlayerData.MaxStamina)
            {
                Data.lastStaminaRecoveryUtcTicks = TimeUtil.UtcNow().Ticks;
            }
            MarkDirty();
        }

        public void AddStamina(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (amount == 0 || Data.hasUnlimitedStamina || Data.stamina >= PlayerData.MaxStamina) return;

            Data.stamina = Math.Min(PlayerData.MaxStamina, Data.stamina + amount);
            if (Data.stamina >= PlayerData.MaxStamina)
            {
                Data.lastStaminaRecoveryUtcTicks = TimeUtil.UtcNow().Ticks;
            }
            MarkDirty();
        }

        public bool TrySpendCoins(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (Data.hasUnlimitedCoins) return true;
            if (Data.coins < amount) return false;

            Data.coins -= amount;
            MarkDirty();
            return true;
        }

        public void AddCoins(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (amount == 0 || Data.hasUnlimitedCoins) return;

            Data.coins = Data.coins > int.MaxValue - amount ? int.MaxValue : Data.coins + amount;
            MarkDirty();
        }

        public void SetUnlimitedStamina(bool enabled)
        {
            if (Data.hasUnlimitedStamina == enabled) return;
            Data.hasUnlimitedStamina = enabled;
            MarkDirty();
        }

        public void SetUnlimitedCoins(bool enabled)
        {
            if (Data.hasUnlimitedCoins == enabled) return;
            Data.hasUnlimitedCoins = enabled;
            MarkDirty();
        }

        private void RefreshStamina()
        {
            if (Data.hasUnlimitedStamina || Data.stamina >= PlayerData.MaxStamina)
            {
                return;
            }

            var nowTicks = TimeUtil.UtcNow().Ticks;
            var intervalTicks = TimeSpan.FromMinutes(PlayerData.StaminaRecoveryMinutes).Ticks;
            var elapsedTicks = nowTicks - Data.lastStaminaRecoveryUtcTicks;
            if (elapsedTicks < 0)
            {
                // A device clock can move backwards; restart this recovery interval
                // instead of leaving the player unable to recover indefinitely.
                Data.lastStaminaRecoveryUtcTicks = nowTicks;
                MarkDirty();
                return;
            }

            if (elapsedTicks < intervalTicks)
            {
                return;
            }

            var recovered = (int)Math.Min(PlayerData.MaxStamina - Data.stamina, elapsedTicks / intervalTicks);
            if (recovered <= 0) return;

            Data.stamina += recovered;
            Data.lastStaminaRecoveryUtcTicks = Data.stamina >= PlayerData.MaxStamina
                ? nowTicks
                : Data.lastStaminaRecoveryUtcTicks + recovered * intervalTicks;
            MarkDirty();
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
