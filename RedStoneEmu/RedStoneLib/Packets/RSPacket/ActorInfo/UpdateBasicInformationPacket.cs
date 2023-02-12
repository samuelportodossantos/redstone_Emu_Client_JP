using RedStoneLib.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Packets.RSPacket.ActorInfo
{
    /// <summary>
    /// HP更新？
    /// </summary>
    public class UpdateBasicInformationPacket : Packet
    {
        ushort CharID, NowHP, Unk;

        public UpdateBasicInformationPacket(ushort charID, ushort nowHP, ushort unk)
        {
            CharID = charID;
            NowHP = nowHP;
            Unk = unk;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write((ushort)NowHP);

            //2A5F03C
            writer.Write(Unk);
            writer.Write(CharID);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1147);
        }
    }
}
