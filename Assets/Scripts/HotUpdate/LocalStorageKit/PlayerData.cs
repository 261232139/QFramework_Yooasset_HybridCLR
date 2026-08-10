/****************************************************************************
 * PlayerData — 玩家数据类
 * 
 * 职责：存储玩家的游戏数据
 * 使用 EasySave3 进行持久化
 ****************************************************************************/

using System;

namespace HotUpdate.LocalStorageKit
{
    [Serializable]
    public class PlayerData
    {
        public int currentLevelID = 1;

        public PlayerData()
        {
            currentLevelID = 1;
        }

        public PlayerData(int levelID)
        {
            currentLevelID = levelID;
        }

        public void Reset()
        {
            currentLevelID = 1;
        }

        public override string ToString()
        {
            return $"PlayerData [LevelID: {currentLevelID}]";
        }
    }
}
