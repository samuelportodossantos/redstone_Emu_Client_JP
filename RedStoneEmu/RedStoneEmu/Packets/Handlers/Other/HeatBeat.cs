using RedStoneLib.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneEmu.Packets.Handlers.Other
{
    /// <summary>
    /// ヒートビート更新
    /// </summary>
    [PacketHandlerAttr(0x1000)]
    class HeatBeat : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            context.BeatTime = DateTime.Now;
        }
    }
}
