/****************************************************************************
 * Copyright (c) 2024 Game Project UNDER MIT License
 *
 * 卡牌格子数据
 *
 * 职责: 定义棋盘上单个格子的配置（初始卡牌、是否固定）
 * 约束: 纯数据类，不包含运行时状态
 ****************************************************************************/

using System;

namespace Game.Level.Data
{
    /// <summary>
    /// 卡牌格子配置数据
    /// 
    /// 代表关卡配置中棋盘上的一个格子。
    /// 运行时修改状态由 RuntimeCardSlot 管理。
    /// </summary>
    [Serializable]
    public class CardSlotData
    {
        /// <summary>该格子初始的卡牌数据</summary>
        public CardData cardData;

        /// <summary>是否固定（1=固定，玩家无法修改；0=普通，允许修改）</summary>
        public int isFixed;

        public CardSlotData() { }

        public CardSlotData(CardData cardData, int isFixed = 0)
        {
            this.cardData = cardData;
            this.isFixed = isFixed;
        }

        /// <summary>该格子是否为固定牌</summary>
        public bool IsFixed => isFixed == 1;
    }
}
