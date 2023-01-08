using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedStoneLib.Packets.RSPacket.PitchmanShop
{
    /// <summary>
    /// 露店アイテム位置変更
    /// </summary>
    public class ChangePitchmanShopItemPlacePacket : Packet
    {
        readonly ushort From;
        readonly ushort To;

        public ChangePitchmanShopItemPlacePacket(ushort from, ushort to)
        {
            From = from;
            To = to;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write(From);
            writer.Write(To);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x120F);
        }
    }
}
