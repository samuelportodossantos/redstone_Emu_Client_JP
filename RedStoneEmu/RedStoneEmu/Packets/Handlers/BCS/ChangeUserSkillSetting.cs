using RedStoneEmu.Database.RedStoneEF;
using RedStoneLib.Packets;
using RedStoneLib.Packets.RSPacket.BCS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneEmu.Packets.Handlers.BCS
{
    /// <summary>
    /// スキル設定の変更
    /// </summary>
    [PacketHandlerAttr(0x7003)]
    class ChangeUserSkillSetting : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            //ToDo
        }
    }
}
