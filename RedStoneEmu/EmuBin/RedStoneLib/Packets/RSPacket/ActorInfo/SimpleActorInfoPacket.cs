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
    /// 送信後したキャラクターは新規登録されない
    /// </summary>
    public class SimpleActorInfoPacket : Packet
    {
        byte[] SimpleActorInfoData;

        public SimpleActorInfoPacket(Actor actor)
        {
            SimpleActorInfoData = SimpleActorInfo.ToBytes(actor);
        }
        
        public override byte[] Build()
        {

            var writer = new PacketWriter();
            
            writer.Write(SimpleActorInfoData);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x123A);
        }
    }
}
