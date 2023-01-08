using RedStoneLib.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedStoneLib.Packets.RSPacket.Trade
{
    /// <summary>
    /// 取引アイテム追加（相手）
    /// </summary>
    public class AddTradeItemByTradeMatePacket : Packet
    {
        readonly Item Item;

        public AddTradeItemByTradeMatePacket(Item item)
        {
            Item = item;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write(Item);
            writer.Write((uint)0);
            writer.Write((ushort)0);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1194);
        }
    }
}
