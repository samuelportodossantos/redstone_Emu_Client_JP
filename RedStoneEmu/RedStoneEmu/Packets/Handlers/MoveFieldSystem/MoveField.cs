using RedStoneEmu.Games;
using RedStoneEmu.Packets.Handlers.GameLogin;
using RedStoneLib;
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
        public static void Execute(GameClient client, string moveToFileName=null)
        {
            //規定のハンドル解除
            client.Socket.ConnectionLost -= client.Save;

            //マップシリアル変更
#if DEBUG
            string ip = IPAddressProvider.LocalIP;
#else
            string ip = IPAddressProvider.GlobalIP;
#endif
            //移動
            if (moveToFileName == null)
            {
                MAPServer.AllMapServers[client.User.MapSerial].MoveField(client.User, ip);
            }
            else if (Map.AllMaps.Values.Any(t => t.FileName == moveToFileName))
            {
                //moveToFile存在
                Map targetMap = Map.AllMaps.Values.First(t => t.FileName == moveToFileName);
                client.User.MapSerial = targetMap.SerialID;
                if (targetMap.AreaInfos.Any(t => t.ObjectInfo == Map.AreaInfo.ObjectType.SystemMovePosition))
                {
                    //システム転送位置1
                    var sysMovePoint = targetMap.AreaInfos.First(t => t.ObjectInfo == Map.AreaInfo.ObjectType.SystemMovePosition);
                    client.User.PosX = (ushort)sysMovePoint.CenterPos.X;
                    client.User.PosY = (ushort)sysMovePoint.CenterPos.Y;
                }
                else if (targetMap.AreaInfos.Any(t => t.ObjectInfo == Map.AreaInfo.ObjectType.System && t.SubObjectInfo == 0x412))
                {
                    //システム転送位置2
                    var sysMovePoint = targetMap.AreaInfos.First(t => t.ObjectInfo == Map.AreaInfo.ObjectType.System && t.SubObjectInfo == 0x412);
                    client.User.PosX = (ushort)sysMovePoint.CenterPos.X;
                    client.User.PosY = (ushort)sysMovePoint.CenterPos.Y;
                }
                else
                {
                    //ランダム
                    int x = 0, y = 0;
                    do
                    {
                        x = Helper.StaticRandom.Next((int)targetMap.Size.Width);
                        y = Helper.StaticRandom.Next((int)targetMap.Size.Height);
                    } while (targetMap.GetBlock(x, y) != 0);
                    client.User.PosX = (ushort)(x * 64);
                    client.User.PosY = (ushort)(x * 32);
                }
            }
            else
            {
                throw new KeyNotFoundException($"Not Found:{moveToFileName}");
            }

            //保存
            client.Save();

            //移動先のマップに参加
            if (!MAPServer.AllMapServers[client.User.MapSerial].Join(client, out var _, true))
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
