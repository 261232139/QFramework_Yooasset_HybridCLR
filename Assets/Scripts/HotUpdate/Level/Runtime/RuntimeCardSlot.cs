/****************************************************************************
 * Copyright (c) 2024 Game Project UNDER MIT License
 *
 * 运行时卡牌格子
 *
 * 职责: 管理棋盘格子的运行时状态（当前卡牌、备份卡牌）
 * 约束: 不包含 MonoBehaviour，纯数据管理
 ****************************************************************************/

using System;

namespace Game.Level.Data
{
    /// <summary>
    /// 运行时卡牌格子
    /// 
    /// 基于 CardSlotData 配置，管理格子的运行时状态。
    /// 支持卡牌替换和撤销操作。
    /// </summary>
    public class RuntimeCardSlot
    {
        /// <summary>配置数据（只读）</summary>
        public CardSlotData ConfigData { get; private set; }

        /// <summary>当前卡牌（可能被玩家修改）</summary>
        public CardData CurrentCard { get; set; }

        /// <summary>备份卡牌（替换前的旧卡牌，用于撤销）</summary>
        public CardData BackupCard { get; set; }

        public RuntimeCardSlot(CardSlotData configData)
        {
            ConfigData = configData;
            CurrentCard = configData.cardData;
            BackupCard = null;
        }

        /// <summary>该格子是否为固定牌（无法修改）</summary>
        public bool IsFixed => ConfigData.IsFixed;

        /// <summary>
        /// 替换卡牌
        /// 
        /// 固定牌无法替换，返回 false。
        /// 普通牌替换时，旧卡牌保存到 BackupCard。
        /// </summary>
        public bool ReplaceCard(CardData newCard)
        {
            if (IsFixed)
                return false;

            BackupCard = CurrentCard;
            CurrentCard = newCard;
            return true;
        }

        /// <summary>撤销上一次替换（恢复 BackupCard）</summary>
        public bool Undo()
        {
            if (BackupCard == null)
                return false;

            CurrentCard = BackupCard;
            BackupCard = null;
            return true;
        }

        /// <summary>重置为配置初始状态</summary>
        public void Reset()
        {
            CurrentCard = ConfigData.cardData;
            BackupCard = null;
        }
    }
}
