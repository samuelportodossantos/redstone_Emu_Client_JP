using RedStoneLib.Packets;
using RedStoneLib.Packets.RSPacket.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneEmu.Packets.Handlers.LoginSystem
{
    /// <summary>
    /// キャラ選択
    /// </summary>
    [PacketHandlerAttr(0x1006)]
    class SelectAvatar : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            //セキュリティコード
            var packet_security_code = reader.ReadUInt16();

            //unknown
            reader.ReadUInt32();

            //キャラ名
            string charName = reader.ReadSjis(0x12);

            //セキュリティコード違う
            if (context.PacketSecurityCode != packet_security_code)
            {
                Logger.WriteError("セキュリティコードが違います．>>[{0}]", charName);
                context.Disconnect();
                return;
            }

            //新規セキュリティコード
            context.PacketSecurityCode = (ushort)(new Random().Next());

            //結果
            if (ServerList.GameServerInfos.SingleOrDefault(t => t.ServerId == context.ServerID) == null)
            {
                //鯖きえた
                context.SendPacket(new ResultSelectAvatarPacket(ResultSelectAvatarPacket.SelectAvatarResult.Failed, context.PacketSecurityCode));
            }
            else
            {
                //成功
                context.SendPacket(new ResultSelectAvatarPacket(ResultSelectAvatarPacket.SelectAvatarResult.Success, context.PacketSecurityCode));
            }
        }
    }
}
