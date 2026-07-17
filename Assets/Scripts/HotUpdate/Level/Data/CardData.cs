/****************************************************************************
 * Copyright (c) 2024 Game Project UNDER MIT License
 *
 * 卡牌数据结构
 *
 * 职责: 定义卡牌的不可变数据（类型、数值、花色）
 * 约束: 纯数据类，不包含业务逻辑
 ****************************************************************************/

using System;

namespace Game.Level.Data
{
    /// <summary>卡牌类型</summary>
    public enum CardType
    {
        Normal = 0,  // 普通牌
        Item = 1,    // 道具牌（预留）
    }

    /// <summary>卡牌花色</summary>
    public enum CardSuit
    {
        None = 0,      // 无花色
        Spade = 1,     // 黑桃
        Heart = 2,     // 红心
        Club = 3,      // 梅花
        Diamond = 4,   // 方块
    }

    /// <summary>
    /// 卡牌数据
    /// 
    /// 不可变数据结构，代表一张卡牌的基本属性。
    /// 运行时状态由 RuntimeCardSlot 管理。
    /// </summary>
    [Serializable]
    public class CardData
    {
        /// <summary>卡牌类型（普通/道具）</summary>
        public CardType type = CardType.Normal;

        /// <summary>卡牌数值（A=1, 2-10=2-10, J=11, Q=12, K=13）</summary>
        public int number;

        /// <summary>卡牌花色</summary>
        public CardSuit suit = CardSuit.None;

        public CardData() { }

        public CardData(CardType type, int number, CardSuit suit)
        {
            this.type = type;
            this.number = number;
            this.suit = suit;
        }

        /// <summary>返回卡牌的唯一标识字符串</summary>
        public override string ToString() => $"{suit}{number}";
    }
}
