using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedStoneLib.Packets.RSPacket.PitchmanShop
{
    /// <summary>
    /// 露店にアイテム追加
    /// </summary>
    public class AddPitchmanShopItemPacket : Packet
    {
        readonly ushort From;
        readonly ushort To;
        readonly uint Price;
        readonly byte IsIngot;

        public AddPitchmanShopItemPacket(ushort from, ushort to, uint price, byte isIngot)
        {
            From = from;
            To = to;
            Price = price;
            IsIngot = isIngot;
        }

        public override byte[] Build()
        {

            var writer = new PacketWriter();

            writer.Write(From);
            writer.Write(To);
            writer.Write((ushort)0xCCCC);
            writer.Write(Price);
            writer.Write(IsIngot);
            writer.WriteBytes(0xCC, 3);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }
        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1205);
        }
    }
}
