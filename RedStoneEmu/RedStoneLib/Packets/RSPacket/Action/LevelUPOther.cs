using RedStoneLib.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedStoneLib.Packets.RSPacket.Action
{
    public class LevelUPOther : Packet
    {
        ushort Level, CharID;

        public LevelUPOther(ushort charID, ushort level)
        {
            CharID = charID;
            Level = level;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write(CharID);
            writer.Write(Level);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1159);
        }
    }
}
