using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedStoneLib.Packets.RSPacket.Trade
{
    /// <summary>
    /// 取引アイテム削除
    /// </summary>
    public class RemoveTradeItemPacket : Packet
    {
        readonly ushort CharID, TradeSlot;

        public RemoveTradeItemPacket(ushort charID, ushort tradeSlot)
        {
            CharID = charID;
            TradeSlot = tradeSlot;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write(CharID);
            writer.Write(TradeSlot);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1196);
        }
    }
}
