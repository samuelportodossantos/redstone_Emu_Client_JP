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
    /// SimpleActorInfoで返さなければならない
    /// </summary>
    [PacketHandlerAttr(0x1028)]
    class AskInfoActorList : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            //charID
            var charIDs = reader.Reads<ushort>((int)(size / 2));

            //送信
            context.SendPacket(new SimpleActorInfoListPacket(context.GetMapActors().Where(t => charIDs.Contains(t.CharID)).ToList()));
        }
    }
}
