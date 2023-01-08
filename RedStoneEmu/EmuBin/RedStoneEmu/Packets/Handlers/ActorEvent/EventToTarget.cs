using RedStoneLib.Model;
using RedStoneLib.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneEmu.Packets.Handlers.ActorEvent
{
    /// <summary>
    /// NPCに話しかけた
    /// </summary>
    [PacketHandlerAttr(0x103F)]
    class EventToTarget : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            ushort charID = reader.ReadUInt16();

            //ターゲットNPC
            NPC targetNPC = context.GetMapActors().SingleOrDefault(t => t.CharID == charID) as NPC;
            if (targetNPC == null) return;

            //プレイヤーにセット
            context.User.PlayerEvent = (targetNPC.Events, 0);

            //実行
            context.User.PlayerEvent.Events[0].Execute(context.User, context.SendPacket, charID);
        }
    }
}
