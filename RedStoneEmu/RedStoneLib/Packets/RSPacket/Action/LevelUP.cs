using RedStoneLib.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedStoneLib.Packets.RSPacket.Action
{
    public class LevelUP : Packet
    {
        ushort Level, StatusPoint;
        uint NowEXP, NowSkillPoint;
        ActorStatus Status;

        public LevelUP(ushort level, uint nowEXP, uint nowSkillPoint, ActorStatus status, ushort statusPoint)
        {
            Level = level;
            NowEXP = nowEXP;
            NowSkillPoint = nowSkillPoint;
            Status = status;
            StatusPoint = statusPoint;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write(Level);

            writer.Write(NowEXP);
            writer.Write(NowSkillPoint);
            foreach(ActorStatusType ast in (IEnumerable<ActorStatusType>)Enum.GetValues(typeof(ActorStatusType)))
            {
                writer.Write(Status[ast]);
            }
            writer.Write(StatusPoint);
            for (int i = 0; i < 40; i++)
            {
                writer.Write((byte)i);
            }

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1148);
        }
    }
}
