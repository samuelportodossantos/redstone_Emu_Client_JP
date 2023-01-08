using RedStoneEmu.Packets.Handlers.GameLogin;
using RedStoneLib.Model;
using RedStoneLib.Packets;
using RedStoneLib.Packets.RSPacket.MoveField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneEmu.Packets.Handlers.MoveFieldSystem
{
    /// <summary>
    /// フィールド移動
    /// </summary>
    [PacketHandlerAttr(0x103B)]
    class MoveField : PacketHandler
    {
        /// <summary>
        /// マップ間の移動
        /// </summary>
        public void Execute(GameClient client)
        {
            //規定のハンドル解除
            client.Socket.ConnectionLost -= client.Save;

            //マップシリアル変更
            Map.AllMaps[client.User.MapSerial].MoveField(client.User);

            //保存
            client.Save();

            //移動先のマップに参加
            if (!JoinGame.JoinMap(client, out var _, true))
            {
                throw new Exception("既にPlayerが移動先のマップに登録されています");
            }

            //移動
            client.SendPacket(new MoveFieldNowPacket());
        }

        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            Execute((GameClient)context);
        }
    }
}
