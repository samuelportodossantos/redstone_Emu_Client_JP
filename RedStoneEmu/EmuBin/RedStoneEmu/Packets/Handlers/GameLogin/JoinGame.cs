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
        //本登録処理
        static void MainJoinProcess(Client client, Player previousPlayer = null)
        {
            Map map = Map.AllMaps[(previousPlayer ?? client.User).MapSerial];

            //匿名メソッド内で消えるので保存
            ushort charID = (previousPlayer ?? client.User).CharID;

            //SendPacket追加
            map.SendPackets[charID] = new SendPacketDelegate(client.SendPacket);

            //マップ離脱時の処理登録
            client.Socket.ConnectionLost += () =>
            {
                //消去
                map.Actors.Remove(charID);
                map.SendPackets.Remove(charID);
            };

            //GetActors
            client.GetMapActors += map.HandleGetActors;

            //参加成功
            client.SendPacket(new ResultJoinGamePacket((previousPlayer ?? client.User), map.Header));
        }

        /// <summary>
        /// MAPに参加
        /// </summary>
        /// <param name="client"></param>
        /// <param name="previousPlayer"></param>
        /// <param name="tmpJoin">仮参加</param>
        /// <returns>仮登録後の本登録ではないフラグ</returns>
        public static bool JoinMap(Client client, out Player previousPlayer, bool tmpJoin = false)
        {
            Map map = Map.AllMaps[client.User.MapSerial];

            if (!map.Join(client.User, out previousPlayer))
            {
                //本登録
                MainJoinProcess(client, previousPlayer);
                return false;
            }

            if (!tmpJoin)
            {
                MainJoinProcess(client);
            }

            //要verysimple
            //client.SendPacket(new SimpleActorInfoListPacket(Actors.Values.Where(t => t is Monster && (t as Monster).Respawned).ToList()));

            previousPlayer = null;
            return true;
        }

        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            try
            {
                //MAPに参加
                if (!JoinMap(context, out Player previousPlayer))
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
