using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedStoneLib.Packets.RSPacket.Trade
{
    /// <summary>
    /// 取引準備
    /// </summary>
    public class ReadyTradePacket : Packet
    {
        readonly ushort CharID;

        public ReadyTradePacket(ushort charID)
        {
            CharID = charID;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write(CharID);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1197);
        }
    }
}
