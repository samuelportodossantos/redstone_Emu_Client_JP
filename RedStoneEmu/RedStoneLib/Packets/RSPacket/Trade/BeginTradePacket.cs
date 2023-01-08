using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedStoneLib.Packets.RSPacket.Trade
{
    /// <summary>
    /// 取引開始
    /// </summary>
    public class BeginTradePacket : Packet
    {
        readonly string TraderName;
        readonly ushort TraderCharID, MyCharID;

        public BeginTradePacket(ushort traderCharID, ushort myCharID, string traderName)
        {
            TraderName = traderName;
            TraderCharID = traderCharID;
            MyCharID = myCharID;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write(TraderCharID);
            writer.Write((ushort)0);
            writer.WriteSjis(TraderName, 0x12);
            writer.Write(MyCharID);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1192);
        }
    }
}
