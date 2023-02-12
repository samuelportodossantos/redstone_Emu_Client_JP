using System.Runtime.InteropServices.ComTypes;
using Microsoft.EntityFrameworkCore;
using RedStoneEmu.Database.RedStoneEF;
using RedStoneEmu.Games;
using RedStoneLib.Model;
using RedStoneLib.Packets;
using RedStoneLib.Packets.RSPacket.GameLogin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedStoneEmu.Provider;
using Microsoft.Extensions.DependencyInjection;

namespace RedStoneEmu.Packets.Handlers.GameLogin
{
    /// <summary>
    /// ゲームに接続
    /// GameLoginHandler: 1st
    /// </summary>
    [PacketHandlerAttr(0x1021)]
    class ConnectGame : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            //unknown
            reader.ReadUInt16();

            //ユーザID
            string userID = reader.ReadSjis(0x14);

            //パスワード認証コード
            uint passwordAuthenticationCode = reader.ReadUInt32();

            //キャラ名
            string charName = reader.ReadSjis(0x12);
            string macAddress = reader.ReadSjis(0x12);

            try
            {
                if (!MAPServer.IsPlayerExistInServerByName(charName, out ushort mapSerial))
                {
                    //プレイヤー取得
                    using (var db = new gameContext())
                    {
                        var task = db.Players.SingleAsync(t => t.UserID == userID && t.Name == charName);
                        task.Wait();
                        context.User = task.Result;
                        db.LoadItems(context.User, gameContext.TargetItemArea.ALL);
                    }
                }
                else
                {
                    //既に存在
                    context.User = new Player();
                    context.User.MapSerial = mapSerial;
                    context.User.Name = charName;
                    context.User.UserID = userID;
                }

                if (RedStoneApp.GameServer.Config.Connectable < RedStoneApp.GameServer.Clients.Count)
                {
                    //満員
                    context.SendPacket(new ResultGameConnectPacket(ResultGameConnectPacket.GameConnectResult.OverConnection));
                }
                else
                {
                    //ゲームに接続
                    context.SendPacket(new ResultGameConnectPacket(context.User.MapSerial));
                }
            }
            catch (Exception ex)
            {
                //失敗
                context.SendPacket(new ResultGameConnectPacket(ResultGameConnectPacket.GameConnectResult.ConnectionFailed));
                Logger.WriteException("[Connect Game]", ex);
            }
        }
    }
}
