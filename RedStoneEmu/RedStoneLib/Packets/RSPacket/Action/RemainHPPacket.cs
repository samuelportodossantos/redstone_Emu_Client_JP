using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Packets.RSPacket.Action
{
    public class RemainHPPacket : Packet
    {
        ushort CharID, NowHPRate;
        uint MaxHP;

        public RemainHPPacket(ushort charID, double nowHP, double maxHP)
        {
            CharID = charID;
            MaxHP = (uint)Math.Floor(maxHP*100);
            NowHPRate = (ushort)(nowHP / maxHP * 60000);
        }

        public override byte[] Build()
        {

            var writer = new PacketWriter();

            writer.Write((ushort)0);
            writer.Write(MaxHP);
            writer.Write(NowHPRate);
            writer.Write(CharID);
            writer.Write((uint)0);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x11A5);
        }
    }
}
