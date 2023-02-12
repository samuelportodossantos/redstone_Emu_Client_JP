using RedStoneLib.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedStoneLib.Packets.RSPacket.PitchmanShop
{
    /// <summary>
    /// 露店アイテム購入
    /// </summary>
    public class BuyPitchmanShopItemPacket : Packet
    {
        readonly uint Price;
        readonly ushort ShopID, ItemPos;
        readonly byte IsIngot;
        readonly string PlayerName;
        readonly Item ShopItem;

        public BuyPitchmanShopItemPacket(ushort shopID, ushort itemPos, Item item, uint price, byte isIngot, string playerName)
        {
            ShopID = shopID;
            ItemPos = itemPos;
            ShopItem = item;
            Price = price;
            IsIngot = isIngot;
            PlayerName = playerName;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write(ShopID);
            writer.Write(ItemPos);
            writer.Write(ShopItem);
            writer.Write(Price);
            writer.Write(IsIngot);
            writer.Write((byte)0xCC);
            writer.Write((ushort)0xCCCC);
            writer.Write((uint)3);//?
            writer.Write((ushort)0);
            writer.Write((ushort)0xCCCC);
            writer.Write((uint)0);
            writer.Write((ushort)0xCCCC);
            writer.WriteSjis(PlayerName, 0x12);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1208);
        }
    }
}
