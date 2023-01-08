using RedStoneLib.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Packets.RSPacket.ActorGenerator
{
    /// <summary>
    /// モンスター湧く
    /// </summary>
    public class RegenMonsterPacket : Packet
    {
        byte[] monsterInfo;

        public RegenMonsterPacket(ref Monster monster)
        {
            //リスポーンフラグ立てる
            monster.Spawned = true;
            monsterInfo = SimpleActorInfo.ToBytes(monster);
        }

        public override byte[] Build()
        {

            var writer = new PacketWriter();

            writer.Write((ushort)0xCCCC);
            writer.Write(monsterInfo);
            writer.Write((uint)0);
            writer.Write((byte)0xCC);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x112E);
        }
    }
}
