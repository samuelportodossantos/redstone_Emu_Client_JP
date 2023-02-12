using RedStoneLib.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Model
{
    public class Shop
    {
        /// <summary>
        /// ショップを開いているNPCの名前
        /// </summary>
        public string NpcName { get; private set; }

        /// <summary>
        /// 値段なし
        /// </summary>
        public bool OnlyDisplay { get; private set; }

        /// <summary>
        /// 最大買取額
        /// </summary>
        public uint MaxPurchasePrice { get; private set; }

        /// <summary>
        /// 番号
        /// </summary>
        public ushort Index { get; private set; }

        public bool isEventShop { get; private set; }

        /// <summary>
        /// ショップのタイプ　enum要作成
        /// </summary>
        public ushort Type { get; private set; }

        /// <summary>
        /// 支払い対価 0xFFFF=ゴールド
        /// </summary>
        public ushort PaymentItem { get; private set; }

        /// <summary>
        /// 値段を決戦ポイントにする
        /// </summary>
        public bool isBattlePoint { get; private set; }

        /// <summary>
        /// 原価の倍率
        /// </summary>
        public ushort CostMagnification { get; private set; }

        /// <summary>
        /// 陳列されたアイテムor値段
        /// </summary>
        public (Item item, uint price, uint unk, ushort unk1, ushort unk2, ushort unk3)[] Displayed { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="pr"></param>
        /// <param name="version"></param>
        public Shop(PacketReader pr, double version)
        {
            NpcName = pr.ReadSjis(0x38);
            isEventShop = pr.ReadUInt16() != 0;
            OnlyDisplay = (pr.ReadUInt16() & 0x3333) == 2;
            MaxPurchasePrice = pr.ReadUInt32();
            Index = pr.ReadUInt16();
            Type = pr.ReadUInt16();
            if (version >= 5.4)
            {
                PaymentItem = pr.ReadUInt16();
            }
            else
            {
                PaymentItem = 0xFFFF;
            }

            //アイテム読み込む
            Displayed = new(Item, uint, uint, ushort, ushort, ushort)[pr.ReadUInt16()];
            for (int i = 0; i < Displayed.Length; i++)
            {
                var itemSource = new Item.ItemInfo()
                {
                    UniqueID = pr.ReadUInt32(),
                    ItemIndex = pr.ReadUInt16(),
                    Number = pr.ReadByte(),
                    Endurance = pr.ReadByte(),
                    Values = pr.ReadBytes(2),
                    OPs = pr.Reads<OP>(3),
                    flags = 0
                };
                uint unk = pr.ReadUInt32();
                ushort unk0 = pr.ReadUInt16();
                ushort unk1 = pr.ReadUInt16();
                ushort unk2 = pr.ReadUInt16();
                uint price = pr.ReadUInt32();

                Displayed[i] = (new Item(itemSource), price, unk, unk0, unk1, unk2);
                Console.WriteLine("name:" + Displayed[i].ToString());
                Console.WriteLine("price:" + price);
            }
            CostMagnification = 1;
        }

        public override string ToString()
        {
            return NpcName;
        }
    }
}
