using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedStoneLib.Packets.RSPacket.Bank
{
    /// <summary>
    /// 銀行アイテム移動
    /// </summary>
    public class MoveBankItemPacket : Packet
    {
        readonly ushort FromPos, ToPos;
        readonly uint CheckSum;

        public MoveBankItemPacket(ushort fromPos, ushort toPos, uint checkSum)
        {
            FromPos = fromPos;
            ToPos = toPos;
            CheckSum = checkSum;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write(FromPos);
            writer.Write(ToPos);
            writer.Write((ushort)0xCCCC);
            writer.Write(CheckSum);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x11C2);
        }
    }
}
