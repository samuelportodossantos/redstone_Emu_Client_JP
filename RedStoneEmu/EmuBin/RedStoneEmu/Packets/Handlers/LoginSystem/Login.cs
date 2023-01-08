using Microsoft.EntityFrameworkCore;
using RedStoneEmu.Database.MastodonEF;
using RedStoneEmu.Database.PhpbbEF;
using RedStoneEmu.Database.RedStoneEF;
using RedStoneEmu.Database.SMFEF;
using RedStoneLib.Model;
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
    /// ログインのハンドラ
    /// </summary>
    [PacketHandlerAttr(0x1001)]
    class Login : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            //不明
            var code = reader.ReadUInt16();

            //暗号化されたユーザIDとパス
            var crypted_userID = reader.ReadBytes(0x14);
            var crypted_pass = reader.ReadBytes(0x14);

            //鯖名
            string serverName = reader.ReadSjis(0x10);
            reader.ReadUInt16();//skip junk

            //MACアドレス
            string macAddress = reader.ReadSjis(0x14);

            //junk?
            reader.BaseStream.Seek(0x2C, System.IO.SeekOrigin.Current);

            //不明
            string passCode = reader.ReadSjis(0x0C);
            reader.ReadUInt16();

            //復号化に必要なコード
            ushort userID_securityCode = reader.ReadUInt16();
            ushort pass_securityCode = reader.ReadUInt16();

            //ユーザIDとパスを復号化
            string userID = PacketCrypt.DecodeLoginString(userID_securityCode, crypted_userID);
            string pass = PacketCrypt.DecodeLoginString(pass_securityCode, crypted_pass);

            //認証フラグ
            bool isMatch = false;

            //認証
            /*using (var loginMastodonDB = new postgresContext())
            {
                //ID認証
                var account = loginMastodonDB.Accounts.SingleOrDefaultAsync(t => t.Username == userID).Result;
                if (account != null)
                {
                    //パス認証
                    var pass_hash = loginMastodonDB.Users.SingleAsync(t => t.Id == account.Id).Result.EncryptedPassword;
                    isMatch = DevOne.Security.Cryptography.BCrypt.BCryptHelper.CheckPassword(pass, pass_hash);
                }
            }*/
            /*using (var loginSmfDB = new smfContext())
            {
                //ID認証
                var account = loginSmfDB.smf_members.SingleOrDefaultAsync(t => t.member_name == userID).Result;
                if (account != null)
                {
                    //パス認証
                    isMatch = BCrypt.Net.BCrypt.Verify(userID + pass, account.passwd);
                }
            }*/
            using (var loginPhpBB = new PhpBBContext())
            {
                //ID認証
                var account = loginPhpBB.phpbb_users.SingleOrDefaultAsync(t => t.username == userID).Result;
                if (account != null)
                {
                    //パス認証
                    isMatch = BCrypt.Net.BCrypt.Verify(pass, account.user_password);
                }
            }

            if (isMatch)
            {
                //認証成功

                //プレイヤー情報更新
                context.User = new Player(userID);
                context.ServerID = ServerList.GameServerInfos.SingleOrDefault(t => t.ServerName == serverName)?.ServerId ?? -1;
                context.PacketSecurityCode = (ushort)(new Random().Next(ushort.MaxValue));

                //成功パケ
                context.SendPacket(new ResultLoginPacket(ResultLoginPacket.LoginResult.LoginSuccess_require_onetime, code, context.PacketSecurityCode), flush: true);
                
                using (var db = new gameContext())
                {
                    //アバター取得
                    var task = db.Players.Where(t => t.UserID == userID).ToListAsync(); 
                    task.Wait();
                    ((LoginClient)context).Avatars = task.Result;

                    //装備アイテムだけ読み込み
                    for (int i = 0; i < ((LoginClient)context).Avatars.Count; i++)
                    {
                        db.LoadItems(((LoginClient)context).Avatars[i], gameContext.TargetItemArea.EquipmentItem);
                    }
                }

                //アバターリスト送信
                string localIP = ServerList.GameServerInfos.Single(t => t.ServerId == context.ServerID).LocalIP;
                context.SendPacket(new AvatarListPacket(((LoginClient)context).Avatars.Select(t => (t, localIP)).ToArray(), context.ServerID, context.PacketSecurityCode));

                //記録
                var log = new LoginLog()
                {
                    Datetime = DateTime.Now,
                    UserName = userID,
                    IPAddress = context.Socket.Socket.Client.RemoteEndPoint.ToString(),
                    MACAddress = macAddress
                };
                using (var loginMemoryDB = new loginContext())
                {
                    loginMemoryDB.LoginLogs.Add(log);
                    loginMemoryDB.SaveChangesAsync().Wait();
                }
            }
            else
            {
                //認証失敗
                context.SendPacket(new ResultLoginPacket(ResultLoginPacket.LoginResult.LoginFailures, code, context.PacketSecurityCode));
            }
        }
    }
}
