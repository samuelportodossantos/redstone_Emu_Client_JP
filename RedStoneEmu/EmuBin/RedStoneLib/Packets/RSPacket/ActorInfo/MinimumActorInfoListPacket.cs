using RedStoneLib.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Packets.RSPacket.ActorInfo
{
    /// <summary>
    /// 最小のActorInfo
    /// </summary>
    public class MinimumActorInfoList : Packet
    {
        ushort Count;
        byte[] MinimumActorInfoBytes;

        public MinimumActorInfoList(List<Actor> actors)
        {
            Count = (ushort)actors.Count;
            MinimumActorInfoBytes = actors.SelectMany(t=>MinimumActorInfo.ToBytes(t)).ToArray();
        }
        
        public override byte[] Build()
        {

            var writer = new PacketWriter();
            writer.Write(Count);
            writer.Write(MinimumActorInfoBytes);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x112A);
        }
    }
}
