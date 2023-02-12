using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedStoneLib.Packets.RSPacket.Action
{
    public class UpdateEXP : Packet
    {
        uint NowEXP, NowSkillPoint;
        ushort Level;

        public UpdateEXP(ushort level, uint nowEXP, uint nowSkillPoint)
        {
            Level = level;
            NowEXP = nowEXP;
            NowSkillPoint = nowSkillPoint;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write((ushort)Level);
            writer.Write(NowEXP);
            writer.Write(NowSkillPoint);
            writer.Write((ushort)0x0000);//unknown

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1146);
        }
    }
}
