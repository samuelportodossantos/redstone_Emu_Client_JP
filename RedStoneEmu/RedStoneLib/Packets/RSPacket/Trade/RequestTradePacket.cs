using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedStoneLib.Packets.RSPacket.Trade
{
    /// <summary>
    /// 取引申請
    /// </summary>
    public class RequestTradePacket : Packet
    {
        readonly string PlayerName;

        public RequestTradePacket(string playerName)
        {
            PlayerName = playerName;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.WriteSjis(PlayerName, 0x12);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1190);
        }
    }
}
