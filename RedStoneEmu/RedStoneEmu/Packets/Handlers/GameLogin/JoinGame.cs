using RedStoneEmu.Games;
using RedStoneLib.Model;
using RedStoneLib.Packets;
using RedStoneLib.Packets.RSPacket.GameLogin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneEmu.Packets.Handlers.GameLogin
{
    /// <summary>
    /// ゲーム（マップ）に参加
    /// GameLoginHandler: 2nd
    /// </summary>
    [PacketHandlerAttr(0x1022)]
    class JoinGame : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            try
            {
                //MAPに参加
                if (!MAPServer.AllMapServers[context.User.MapSerial].Join(context, out var previousPlayer)) //if (!JoinMap(context, out Player previousPlayer))
                {
                    //登録済み
                    context.User = previousPlayer;
                }
                else
                {
                    context.User.Reflesh();
                }
            }
            catch (Exception ex)
            {
                //失敗
                context.SendPacket(new ResultJoinGamePacket(ResultJoinGamePacket.JoinGameResult.ProblemOccured));
                Logger.WriteException("[JoinGame]", ex);
            }
        }
    }
}
