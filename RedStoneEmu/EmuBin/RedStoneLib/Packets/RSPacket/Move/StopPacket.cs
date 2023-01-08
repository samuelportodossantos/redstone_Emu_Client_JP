using RedStoneLib.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Packets.RSPacket.Move
{
    /// <summary>
    /// 停止の通知
    /// </summary>
    public class StopPacket : Packet
    {
        ushort charID = 0;
        ushort posX = 0, posY = 0;


        public StopPacket(Player player)
        {
            charID = player.CharID;
            posX = player.PosX;
            posY = player.PosY;
        }

        public override byte[] Build()
        {

            var writer = new PacketWriter();

            writer.Write(charID);
            writer.Write(posX);
            writer.Write(posY);
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1124);
        }
    }
}
