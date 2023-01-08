using RedStoneLib.Packets;
using RedStoneLib.Packets.RSPacket.Move;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneEmu.Packets.Handlers.Move
{
    /// <summary>
    /// 移動パケのハンドラ
    /// </summary>
    [PacketHandlerAttr(0x1023)]
    class Move : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            //移動前の場所
            var posX = reader.ReadUInt16();
            var posY = reader.ReadUInt16();

            //移動先
            var movetoX = reader.ReadUInt16();
            var movetoY = reader.ReadUInt16();

            //移動結果
            context.SendPacket(new MoveResultPacket(context.User));
            //context.isMove = true;

            //他の人に移動を通知
           
            //バトル中なら中断（StopBattleで中断するはずなので保険）

            context.User.PosX = movetoX;
            context.User.PosY = movetoY;

            //NPCSHOP
        }
    }
}
