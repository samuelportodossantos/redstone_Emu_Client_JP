using RedStoneLib.Model;
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
    /// 自分の詳細情報の要求
    /// GameLoginHandler: 3rd
    /// </summary>
    [PacketHandlerAttr(0x10A1)]
    class AskPlayerInfo : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            //playerInfo
            context.SendPacket(new PlayerInfoPacket(context.User.GetPlayerInfoStruct()));
        }
    }
}
