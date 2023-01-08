using RedStoneLib.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedStoneLib.Packets.RSPacket.Trade
{
    /// <summary>
    /// 取引メッセージ
    /// </summary>
    public class TradeMessagePacket : Packet
    {
        readonly ItemBase.ItemResult Message;

        public TradeMessagePacket(ItemBase.ItemResult message)
        {
            Message = message;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write((ushort)Message);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x119A);
        }
    }
}
