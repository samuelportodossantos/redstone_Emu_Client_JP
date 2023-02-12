using RedStoneLib.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedStoneLib.Packets.RSPacket.PitchmanShop
{
    /// <summary>
    /// 露店アイテム売れた
    /// </summary>
    public class SoldPitchmanShopItemPacket : Packet
    {
        readonly uint Price;
        readonly ushort ItemPos;
        readonly byte IsIngot;
        readonly string PlayerName;

        public SoldPitchmanShopItemPacket(ushort itemPos,　uint price, byte isIngot, string playerName)
        {
            ItemPos = itemPos;
            Price = price;
            IsIngot = isIngot;
            PlayerName = playerName;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.WriteSjis(PlayerName, 0x12);
            writer.Write(ItemPos);
            writer.Write((ushort)0xCCCC);
            writer.Write(Price);
            writer.Write(IsIngot);
            writer.Write((byte)0xCC);
            writer.Write((ushort)0xCCCC);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1209);
        }
    }
}
