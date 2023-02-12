using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedStoneLib.Packets.RSPacket.PitchmanShop
{
    /// <summary>
    /// 露店情報リスト
    /// </summary>
    public class SimplePitchmanShopInfoListPacket : Packet
    {
        readonly byte[] Data;

        public SimplePitchmanShopInfoListPacket(byte[] data)
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
            return new PacketHeader(packetSize, 0x1214);
        }
    }
}
