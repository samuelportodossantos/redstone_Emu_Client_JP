using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Packets.RSPacket.Other
{
    /// <summary>
    /// 切断通知
    /// </summary>
    public class DisconnectPacket : Packet
    {
        public override byte[] Build()
        {

            var writer = new PacketWriter();

            writer.Write(0);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1107);
        }
    }
}
