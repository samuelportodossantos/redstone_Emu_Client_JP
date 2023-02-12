using RedStoneLib.Model;
using RedStoneLib.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Packets.RSPacket.ActorInfo
{
    /// <summary>
    /// Actor情報送信
    /// </summary>
    public class VerySimpleActorInfoPacket : Packet
    {
        byte[] VerySimpleActorInfoData;

        public VerySimpleActorInfoPacket(Actor actor)
        {
            VerySimpleActorInfoData = VerySimpleActorInfo.ToBytes(actor);
        }
        
        public override byte[] Build()
        {

            var writer = new PacketWriter();
            
            writer.Write(VerySimpleActorInfoData);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x114E);
        }
    }
}
