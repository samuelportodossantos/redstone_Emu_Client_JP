using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Packets.RSPacket.ActorGenerator
{
    /// <summary>
    /// Actorを消す
    /// </summary>
    public class RemoveActorPacket : Packet
    {
        ushort CharID;

        public RemoveActorPacket(ushort charID) => CharID = charID;

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write(CharID);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x114F);
        }
    }
}
