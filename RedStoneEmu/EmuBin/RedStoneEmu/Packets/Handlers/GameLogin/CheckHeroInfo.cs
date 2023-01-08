using RedStoneLib.Packets;
using RedStoneLib.Packets.RSPacket.ActorInfo;
using RedStoneLib.Packets.RSPacket.GameLogin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneEmu.Packets.Handlers.GameLogin
{
    /// <summary>
    /// ヒーローの情報を確認
    /// GameLoginHandler: 4th
    /// </summary>
    [PacketHandlerAttr(0x10A2)]
    class CheckHeroInfo : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            //context.SendPacket(new UpdateBasicInformationPacket(context.User.CharID, (ushort)(context.User.NowHP * 100), 100));

            context.SendPacket(new SuccessToJoinGamePacket());
        }
    }
}
