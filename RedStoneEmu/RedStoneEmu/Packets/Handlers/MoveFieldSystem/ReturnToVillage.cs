using RedStoneLib.Model;
using RedStoneLib.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneEmu.Packets.Handlers.MoveFieldSystem
{
    /// <summary>
    /// 街帰還
    /// </summary>
    [PacketHandlerAttr(0x104F)]
    class ReturnToVillage : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            ushort moveType = reader.ReadUInt16();

            //暫定
            MoveField.Execute((GameClient)context, Map.AllMaps[0].FileName);
            context.User.NowHP = 1;
        }
    }
}
