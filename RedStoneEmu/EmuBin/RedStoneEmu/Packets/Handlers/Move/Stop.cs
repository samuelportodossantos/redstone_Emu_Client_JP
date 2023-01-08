using RedStoneLib.Model;
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
    /// プレイヤーが止まった時に発生
    /// </summary>
    class Stop : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            context.User.Direct = (ActorDirect)reader.ReadUInt16();
            context.User.PosX = reader.ReadUInt16();
            context.User.PosY = reader.ReadUInt16();
            //context.User.isMove = false;
            context.SendPacket(new StopPacket(context.User));
        }
    }
}
