using RedStoneLib.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Packets.RSPacket.Move
{
    /// <summary>
    /// 移動結果のパケット
    /// MaxHP, NowHP, NowCP, Move_Speed, isRunが必要
    /// </summary>
    public class MovePacket : Packet
    {
        ushort PosX, PosY, MovetoX, MovetoY, CharID, MoveSpeed;

        public MovePacket(ushort charID, ushort posX, ushort posY, ushort movetoX, ushort movetoY, ushort moveSpeed)
        {
            CharID = charID;
            PosX = posX;
            PosY = posY;
            MovetoX = movetoX;
            MovetoY = movetoY;
            MoveSpeed = moveSpeed;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write(CharID);

            writer.Write(PosX);//setPos
            writer.Write(PosY);

            writer.Write(MovetoX);
            writer.Write(MovetoY);

            writer.Write(MoveSpeed);//移動速度

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1123);
        }
    }
}
