using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedStoneLib.Packets.RSPacket.Trade
{
    /// <summary>
    /// 取引アイテム追加（自分）
    /// </summary>
    public class AddTradeItemByMePacket : Packet
    {
        readonly ushort InveSlot, Count;

        public AddTradeItemByMePacket(ushort inveSlot, ushort count)
        {
            InveSlot = inveSlot;
            Count = count;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write(InveSlot);
            writer.Write(Count);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1193);
        }
    }
}
