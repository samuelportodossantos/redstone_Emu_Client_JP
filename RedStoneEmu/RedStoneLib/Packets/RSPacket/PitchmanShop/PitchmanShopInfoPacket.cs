using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedStoneLib.Packets.RSPacket.PitchmanShop
{
    /// <summary>
    /// 露店陳列アイテムリスト
    /// </summary>
    public class PitchmanShopInfoPacket : Packet
    {
        readonly byte[] Data;

        public PitchmanShopInfoPacket(byte[] data)
        {
            Data = data;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write(Data);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x120D);
        }
    }
}
