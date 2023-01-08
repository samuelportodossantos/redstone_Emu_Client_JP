using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Packets.RSPacket.GameLogin
{
    /// <summary>
    /// ゲーム参加成功パケット
    /// </summary>
    public class SuccessToJoinGamePacket : Packet
    {
        public override byte[] Build()
        {

            var writer = new PacketWriter();

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x123E);
        }
    }
}
