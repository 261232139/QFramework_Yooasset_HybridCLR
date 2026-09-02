/****************************************************************************
 * PlayerData — 玩家数据类
 * 
 * 职责：存储玩家的游戏数据
 * 使用 EasySave3 进行持久化
 ****************************************************************************/

using System;
using HotUpdate.Utils;

namespace HotUpdate.LocalStorageKit
{
    [Serializable]
    public class PlayerData
    {
        public const int MaxStamina = 5;
        public const int StaminaRecoveryMinutes = 30;
        public const int CurrentDataVersion = 2;

        public int dataVersion = CurrentDataVersion;
        public int currentLevelID = 1;
        public int stamina = MaxStamina;
        public int coins;
        public bool hasUnlimitedStamina;
        public bool hasUnlimitedCoins;
        public long lastStaminaRecoveryUtcTicks;

        public PlayerData()
        {
            Reset();
        }

        public PlayerData(int levelID)
        {
            Reset();
            currentLevelID = Math.Max(1, levelID);
        }

        public void Reset()
        {
            currentLevelID = 1;
            dataVersion = CurrentDataVersion;
            stamina = MaxStamina;
            coins = 0;
            hasUnlimitedStamina = false;
            hasUnlimitedCoins = false;
            lastStaminaRecoveryUtcTicks = TimeUtil.UtcNow().Ticks;
        }

        /// <summary>Repairs data from old or malformed local saves.</summary>
        public bool Normalize()
        {
            var changed = false;

            // Old saves did not contain stamina or recovery timestamps. Give those
            // players the same full stamina granted to a first-time player.
            if (dataVersion < CurrentDataVersion)
            {
                dataVersion = CurrentDataVersion;
                stamina = MaxStamina;
                lastStaminaRecoveryUtcTicks = TimeUtil.UtcNow().Ticks;
                changed = true;
            }

            if (currentLevelID < 1)
            {
                currentLevelID = 1;
                changed = true;
            }

            var clampedStamina = Math.Max(0, Math.Min(MaxStamina, stamina));
            if (stamina != clampedStamina)
            {
                stamina = clampedStamina;
                changed = true;
            }

            if (coins < 0)
            {
                coins = 0;
                changed = true;
            }

            if (lastStaminaRecoveryUtcTicks <= 0)
            {
                lastStaminaRecoveryUtcTicks = TimeUtil.UtcNow().Ticks;
                changed = true;
            }

            return changed;
        }

        public override string ToString()
        {
            return $"PlayerData [LevelID: {currentLevelID}, Stamina: {stamina}/{MaxStamina}, Coins: {coins}, UnlimitedStamina: {hasUnlimitedStamina}, UnlimitedCoins: {hasUnlimitedCoins}]";
        }
    }
}
