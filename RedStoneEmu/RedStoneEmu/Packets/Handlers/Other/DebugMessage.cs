using RedStoneLib.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneEmu.Packets.Handlers.Other
{
    [PacketHandlerAttr(0x106F)]
    class DebugMessage : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            string msg = reader.ReadSjis();
            Logger.WriteWarning("[DEBUG] " + msg);
        }
    }
}
