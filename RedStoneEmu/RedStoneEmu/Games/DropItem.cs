using RedStoneLib;
using RedStoneLib.Model;
using RedStoneLib.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedStoneEmu.Games
{
    /// <summary>
    /// モンスターが落とすアイテム
    /// </summary>
    public class DropItem
    {
        /// <summary>
        /// 内包されるアイテム
        /// </summary>
        public Item DroppedItem { get; private set; }

        /// <summary>
        /// 落とした人
        /// </summary>
        public ushort CharID { get; private set; }

        /// <summary>
        /// マップ内のインデックス
        /// </summary>
        [BitField(0, 0x0A)]
        public ushort Index { get; private set; }

        /// <summary>
        /// アイテムID
        /// </summary>
        [BitField(1, 0x0E)]
        ushort ItemID
            => (ushort)DroppedItem.ItemIndex;

        /// <summary>
        /// アイテム種類
        /// </summary>
        [BitField(2, 0x08)]
        byte ItemType
        {
            get
            {
                if (DroppedItem.OPs.Any(t => !t.IsEmpty))
                {
                    if (DroppedItem.Base.IsUniqueItem) return 4;//U OP付き
                    else if (DroppedItem.OPs.Any(t => !t.IsEmpty && t.Base.IsWeak)) return 3;//弱効果
                    else return 1;//N OP付き
                }
                else
                {
                    if (DroppedItem.Base.IsUniqueItem) return 2;//U
                    else return 0;//N
                }
            }
        }

        /// <summary>
        /// アイテム個数（ゴールドはUID）
        /// </summary>
        [BitField(3)]
        ushort ItemNum
            => DroppedItem.ItemIndex != 0 ? DroppedItem.Count : (ushort)DroppedItem.UniqueID;

        /// <summary>
        /// X座標
        /// </summary>
        [BitField(4)]
        ushort PosX { get; set; }

        /// <summary>
        /// Y座標
        /// </summary>
        [BitField(5)]
        ushort PosY { get; set; }

        private DropItem(Item droppedItem, Point<ushort> pos, ushort charID)
        {
            DroppedItem = droppedItem;
            PosX = pos.X;
            PosY = pos.Y;
            CharID = charID;
        }

        /// <summary>
        /// アイテム抽選
        /// </summary>
        /// <param name="player"></param>
        /// <param name="monster"></param>
        /// <returns></returns>
        public static DropItem[] Rottely(Player player, Monster monster)
        {
            List<DropItem> result = new List<DropItem>();
            int rottelyNum = 3;

            //invalidを除く落としうるアイテム
            var monsterDroppable = monster.ABase.DropItems.Where(t => t.Type != (ItemBase.ItemType)0xFFFF);

            //落としうるアイテムのタイプ
            ItemBase.ItemType[] monsterDroppableItemType = monsterDroppable.Select(u => u.Type).ToArray();
            
            //落としうるアイテムの確率辞書
            Dictionary<ItemBase.ItemType, int> dropProbDic = monsterDroppable.OrderByDescending(t=>t.Prob).ToDictionary(t => t.Type, t => (int)t.Prob);

            for (int i = 0; i < rottelyNum; i++)
            {
                //アイテム種類抽選
                ItemBase.ItemType targetType = ItemBase.ItemType.None;
                double itemRottelyIndex = Helper.StaticRandom.Next(0, (int)monsterDroppable.Sum(t => t.Prob));
                foreach (var key in dropProbDic.Keys)
                {
                    itemRottelyIndex -= dropProbDic[key];
                    if (itemRottelyIndex <= 0)
                    {
                        targetType = key;
                        break;
                    }
                }

                //落としうるアイテム（ドロップ係数未考慮）
                var droppableItems = ItemBase.AllItemBases.Where(t => t.DropLevel <= monster.Level && targetType == t.Type).OrderByDescending(t => t.DropCoefficient).ToArray();
                if (droppableItems.Length == 0) continue;

                //ドロップ係数抽選
                ItemBase targetItemBase = null;
                int dropCoefRottelyIndex = droppableItems.Sum(t => t.DropCoefficient);
                foreach (var itemBase in droppableItems)
                {
                    dropCoefRottelyIndex -= itemBase.DropCoefficient;
                    if (dropCoefRottelyIndex <= 0)
                    {
                        targetItemBase = itemBase;
                        break;
                    }
                }

                byte num = (byte)Helper.StaticRandom.Next(1, targetItemBase.StackableNum);
                result.Add(new DropItem(new Item((ushort)targetItemBase.Index, num), monster.Pos, player.CharID));
            }
            return result.ToArray();
        }

        /// <summary>
        /// バイト列に変換
        /// </summary>
        /// <param name="index">マップ内の順位</param>
        /// <returns></returns>
        public byte[] ToBytes()
            =>BitField.ToBytes<DropItem, BitField>(this, GetType());

        /// <summary>
        /// インデックスのセット
        /// </summary>
        /// <param name="index"></param>
        public void SetIndex(int index)
            => Index = (ushort)index;
    }
}
