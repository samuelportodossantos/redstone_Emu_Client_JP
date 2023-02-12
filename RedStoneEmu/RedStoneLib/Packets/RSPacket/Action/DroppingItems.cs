using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedStoneLib.Packets.RSPacket.Action
{
    public class DroppingItems : Packet
    {
        ushort Count;
        byte[] Datas;

        public DroppingItems(ushort count, byte[] datas)
        {
            Count = count;
            Datas = datas;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write(Count);
            writer.Write(Datas);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1137);
        }
    }
}
