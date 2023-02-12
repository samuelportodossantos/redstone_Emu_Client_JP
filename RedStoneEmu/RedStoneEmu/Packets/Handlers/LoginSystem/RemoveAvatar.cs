using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RedStoneEmu.Database.RedStoneEF;
using RedStoneEmu.Provider;
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
    /// キャラ削除
    /// </summary>
    [PacketHandlerAttr(0x1005)]
    class RemoveAvatar : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {


            //セキュリティコード
            var packet_security_code = reader.ReadUInt16();

            //unknown
            reader.ReadUInt32();

            //削除される名前
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

            try
            {

                using (var db = new gameContext())
                {
                    //アバター取得
                    var task = db.Players.SingleAsync(t => t.Name == charName);
                    task.Wait();

                    //ターゲット
                    var targetAvatar = task.Result;

                    //装備以外もロード
                    db.LoadItems(targetAvatar, gameContext.TargetItemArea.ALL);

                    //削除
                    db.RemovePlayer(targetAvatar);

                    //セーブ
                    db.SaveChangesAsync().Wait();
                }

                //結果
                context.SendPacket(new ResultRemoveAvatarPacket(ResultRemoveAvatarPacket.RemoveAvatar.Success, context.PacketSecurityCode));
            }
            catch (Exception ex)
            {
                //失敗
                context.SendPacket(new ResultRemoveAvatarPacket(ResultRemoveAvatarPacket.RemoveAvatar.Failed, context.PacketSecurityCode));
                Logger.WriteException("[RemoveAvatar]", ex);
            }
        }
    }
}
