using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedStoneLib.Packets.RSPacket.Trade
{
    /// <summary>
    /// 取引ゴールド追加
    /// </summary>
    public class SetTradeGoldPacket : Packet
    {
        readonly ushort CharID;
        readonly uint Gold;

        public SetTradeGoldPacket(ushort charID, uint gold)
        {
            CharID = charID;
            Gold = gold;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write(CharID);
            writer.Write(Gold);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1195);
        }
    }
}
