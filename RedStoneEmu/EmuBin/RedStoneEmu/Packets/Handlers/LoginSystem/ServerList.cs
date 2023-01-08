using RedStoneEmu.Database.RedStoneEF;
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
    /// サーバーリストを返す
    /// </summary>
    [PacketHandlerAttr(0x1007)]
    class ServerList : PacketHandler
    {
        /// <summary>
        /// サーバーリスト
        /// </summary>
        public static List<GameServerInfo> GameServerInfos = null;

        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            //未ロード
            if (GameServerInfos == null) return;

            /*
            var flag = reader.ReadUInt16();*/

            //鯖リストを返す
            Random rnd = new Random();
            context.PacketSecurityCode = (ushort)rnd.Next();

            var servers = GameServerInfos.Where(t => t.Enable).ToList();
            if (servers.Count > 0) {
                context.SendPacket(new ServerListPacket(servers, context.PacketSecurityCode));
            }
        }
    }
}
