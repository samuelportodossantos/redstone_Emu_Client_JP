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
    /// 移動能力（歩く・走る）変更
    /// </summary>
    [PacketHandlerAttr(0x1027)]
    class MoveAbility : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            context.User.IsRun = reader.ReadUInt16() == 1;
            context.SendPacket(new SetMoveAbility(context.User));
        }
    }
}
