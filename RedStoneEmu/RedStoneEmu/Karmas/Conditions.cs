using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedStoneLib;
using RedStoneLib.Karmas;
using RedStoneLib.Model;

namespace RedStoneEmu.Karmas.Conditions
{
    /// <summary>
    /// 条件無しでOK
    /// </summary>
    [KarmaItemAttr(0x000)]
    class NoCondition : KarmaItemConditionService
    {
        public override bool HandleKarmaItem(Player player, uint[] value)
        {
            return true;
        }
    }

    /// <summary>
    /// レベル
    /// </summary>
    [KarmaItemAttr(0x001)]
    class CheckLevel : KarmaItemConditionService
    {
        public override bool HandleKarmaItem(Player player, uint[] value)
        {
            return player.Level < value[0];
        }
    }

    /// <summary>
    /// 称号チェック
    /// </summary>
    [KarmaItemAttr(0x008)]
    class CheckTitle : KarmaItemConditionService
    {
        public override bool HandleKarmaItem(Player player, uint[] value)
        {
            //称号レベル
            byte titleLevel = player.Titles.TryGetValue((byte)value[0], out byte level) ? level : (byte)0;
            byte requireTitleLevel = (byte)value[1];
            switch (value[2])
            {
                case 0:
                    return titleLevel == requireTitleLevel;
                case 1:
                    return titleLevel <= requireTitleLevel;
                case 2:
                    return titleLevel >= requireTitleLevel;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    /// <summary>
    /// アイテムチェック
    /// </summary>
    [KarmaItemAttr(0x00f)]
    class CheckItem : KarmaItemConditionService
    {
        public override bool HandleKarmaItem(Player player, uint[] value)
        {
            //指定アイテムの数
            int item_num = player.InventoryItems.FindRangeByIndex((ushort)value[0]).Sum(t => t.Count);

            switch (value[2])
            {
                case 0:
                    return item_num == value[1];
                case 1:
                    return item_num <= value[1];
                case 2:
                    return item_num >= value[1];
                default:
                    throw new NotImplementedException("karma:アイテムチェック");
            }
        }
    }

    /// <summary>
    /// バッグ空きチェック
    /// </summary>
    [KarmaItemAttr(0x010)]
    class CheckInventorySpace : KarmaItemConditionService
    {
        public override bool HandleKarmaItem(Player player, uint[] value)
        {
            //バッグ空き数
            int bagEmptySpace = player.InventoryItems.EmptySpaceCount;

            switch (value[1])
            {
                case 0:
                    return bagEmptySpace == value[0];
                case 1:
                    return bagEmptySpace <= value[0];
                case 2:
                    return bagEmptySpace >= value[0];
                default:
                    throw new NotImplementedException("karma:アイテムチェック");
            }
        }
    }

    /// <summary>
    /// クエスト系　不明
    /// </summary>
    [KarmaItemAttr(0x130)]
    class CheckUnknown : KarmaItemConditionService
    {
        public override bool HandleKarmaItem(Player player, uint[] value)
        {
            return false;
        }
    }

    /// <summary>
    /// クエスト受注チェック
    /// </summary>
    [KarmaItemAttr(0x132)]
    class CheckExistQuest : KarmaItemConditionService
    {
        public override bool HandleKarmaItem(Player player, uint[] value)
        {
            int questIndex = (int)value[0];
            bool result = player.ProgressQuests.Select(t=>t.Value.Index).Contains(questIndex);

            //反転
            if (value[1] == 1) result = !result;
            return result;
        }
    }

    /// <summary>
    /// クエスト進行チェック
    /// </summary>
    [KarmaItemAttr(0x133)]
    class CheckProgressQuest : KarmaItemConditionService
    {
        public override bool HandleKarmaItem(Player player, uint[] value)
        {
            int questIndex = (int)value[0];
            int questProgress = (int)value[1];
            if (player.ProgressQuests.Values.All (t=>t.Index!=questIndex))
            {
                //受注していない
                return false;
            }
            return player.ProgressQuests.Values.SingleOrDefault(t => t.Index == questIndex)?.Progress == questProgress;
        }
    }

    /// <summary>
    /// クエスト空きチェック
    /// </summary>
    [KarmaItemAttr(0x135)]
    class CheckEmptyQuest : KarmaItemConditionService
    {
        public override bool HandleKarmaItem(Player player, uint[] value)
        {
            bool result = player.ProgressQuests.Count < 6;
            if (value[0] == 1) result = !result;
            return result;
        }
    }

    /// <summary>
    /// 時間チェック
    /// </summary>
    [KarmaItemAttr(0x193, 1, 2, 3, 4)]
    class CheckDate : KarmaItemConditionService
    {
        public override bool HandleKarmaItem(Player player, uint[] value)
        {
            return DateTime.Now > new DateTime((int)value[0], (int)value[1], (int)value[2], (int)(value[3] % 24), 0, 0);
        }
    }

    /// <summary>
    /// 国チェック
    /// </summary>
    [KarmaItemAttr(0x19C, 1)]
    class CheckCountry : KarmaItemConditionService
    {
        public override bool HandleKarmaItem(Player player, uint[] value)
        {
            return PublicHelper.Country == value[0];
        }
    }
}
