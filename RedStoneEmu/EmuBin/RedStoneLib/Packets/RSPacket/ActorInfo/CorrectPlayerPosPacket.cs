using RedStoneLib.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Packets.RSPacket.ActorInfo
{
    /// <summary>
    /// 指定プレイヤーの位置を修正
    /// </summary>
    public class CorrectPlayerPosPacket : Packet
    {
        ushort posX, posY;
        ushort charID;

        public CorrectPlayerPosPacket(Player player)
        {
            posX = player.PosX;
            posY = player.PosY;
            charID = player.CharID;
        }

        public CorrectPlayerPosPacket(ushort charID, ushort posX, ushort posY)
        {
            this.posX = posX;
            this.posY = posY;
            this.charID = charID;
        }

        public CorrectPlayerPosPacket()
        {
            posX = 0; posY = 0;
            charID = 0xFFFF;
        }

        public override byte[] Build()
        {

            var writer = new PacketWriter();

            writer.Write(charID);
            writer.Write(posX);
            writer.Write(posY);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1127);
        }
    }
}
