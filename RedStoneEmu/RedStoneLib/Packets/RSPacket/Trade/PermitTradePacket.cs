using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedStoneLib.Packets.RSPacket.Trade
{
    /// <summary>
    /// 取引承認
    /// </summary>
    public class PermitTradePacket : Packet
    {
        readonly ushort CharID;

        public PermitTradePacket(ushort charID)
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
            return new PacketHeader(packetSize, 0x1198);
        }
    }
}
