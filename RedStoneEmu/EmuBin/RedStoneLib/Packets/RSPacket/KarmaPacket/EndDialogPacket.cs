using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Packets.RSPacket.KarmaPacket
{
    /// <summary>
    /// NPCとの会話画面を閉じる
    /// </summary>
    public class EndDialogPacket : Packet
    {
        public override byte[] Build()
        {
            packetSize = 0;
            return new byte[0];
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x115E);
        }
    }
}
