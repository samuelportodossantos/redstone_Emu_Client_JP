using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Packets.RSPacket.Action
{
    public class KillPacket : Packet
    {
        ushort ToCharID, FromCharID;
        int IsGhost, IsInstantDeath;

        public KillPacket(ushort toCharID, ushort fromCharID, bool isGhost = false, bool isInstantDeath = false)
        {
            ToCharID = toCharID;
            FromCharID = fromCharID;
            IsGhost = isGhost ? 1 : 0;
            IsInstantDeath = isInstantDeath ? 1 : 0;
        }

        public override byte[] Build()
        {

            var writer = new PacketWriter();

            writer.Write(ToCharID);
            ushort killType = 0;
            int killInfo = (killType & 0xFFF) | ((IsInstantDeath & 1) << 0x0C) | ((IsGhost & 1) << 0x0D);
            writer.Write(killInfo);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1132);
        }
    }
}
