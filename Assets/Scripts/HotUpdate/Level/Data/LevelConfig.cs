/****************************************************************************
 * Copyright (c) 2024 Game Project UNDER MIT License
 *
 * 关卡配置数据
 *
 * 职责: 定义一个关卡的全部静态配置（地图、卡牌、难度、时限）
 * 约束: 纯数据类，从 JSON 反序列化
 ****************************************************************************/

using System;
using System.Collections.Generic;

namespace Game.Level.Data
{
    /// <summary>
    /// 关卡配置
    /// 
    /// 从 Assets/Game/LevelConfig/level{id}.json 加载。
    /// 包含棋盘大小、初始卡牌布局、难度、时限等信息。
    /// 
    /// 棋盘数据使用一维 List，索引规则:
    ///   index = y * mapWidth + x
    /// </summary>
    [Serializable]
    public class LevelConfig
    {
        /// <summary>关卡 ID</summary>
        public int levelId;

        /// <summary>棋盘宽度（列数）</summary>
        public int mapWidth;

        /// <summary>棋盘高度（行数）</summary>
        public int mapHeight;

        /// <summary>
        /// 棋盘格子数据（一维数组）
        /// 
        /// 长度应等于 mapWidth * mapHeight
        /// 索引: index = y * mapWidth + x
        /// </summary>
        public List<CardSlotData> cardSlots = new List<CardSlotData>();

        /// <summary>关卡时限（秒）</summary>
        public int duration;

        /// <summary>难度（1=简单, 2=中等, 3=困难）</summary>
        public int difficulty;

        /// <summary>验证配置的有效性</summary>
        public bool Validate()
        {
            if (levelId <= 0) return false;
            if (mapWidth <= 0 || mapHeight <= 0) return false;
            if (cardSlots.Count != mapWidth * mapHeight) return false;
            if (duration <= 0) return false;
            return true;
        }

        /// <summary>
        /// 获取指定坐标的格子数据
        /// 
        /// 返回 null 表示坐标越界
        /// </summary>
        public CardSlotData GetSlot(int x, int y)
        {
            if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight)
                return null;

            int index = y * mapWidth + x;
            return cardSlots[index];
        }
    }
}
