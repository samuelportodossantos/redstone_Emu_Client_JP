using RedStoneLib.Packets;
using RedStoneLib.Packets.RSPacket.ActorInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneEmu.Packets.Handlers.ActorInfo
{
    /// <summary>
    /// actor info問い合わせ
    /// </summary>
    [PacketHandlerAttr(0x1039)]
    class AskActorInfo : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            var targetCharID = reader.ReadUInt16();

            context.SendPacket(new SimpleActorInfoPacket(context.GetMapActors().Single(t=>t.CharID==targetCharID)));
        }
    }
}
