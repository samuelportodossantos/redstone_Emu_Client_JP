using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedStoneLib.Packets.RSPacket.Quests
{
    public class GetTitlePackt : Packet
    {
        ushort Index;
        byte TitleIndex, Level;

        public GetTitlePackt(ushort index, byte titleIndex, byte level)
        {
            Index = index;
            TitleIndex = titleIndex;
            Level = level;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write(Index);
            writer.Write(TitleIndex);
            writer.Write(Level);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x11C6);
        }
    }
}
