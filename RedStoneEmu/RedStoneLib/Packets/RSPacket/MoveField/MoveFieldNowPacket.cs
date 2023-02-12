using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Packets.RSPacket.MoveField
{
    /// <summary>
    /// 移動させる
    /// </summary>
    public class MoveFieldNowPacket : Packet
    {
        public override byte[] Build()
        {
            packetSize = 0;
            return new byte[0];
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x11E4);
        }
    }
}
