using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RedStoneEmu.Database.RedStoneEF;
using RedStoneEmu.Provider;
using RedStoneLib.Packets;
using RedStoneLib.Packets.RSPacket.BCS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneEmu.Packets.Handlers.BCS
{
    /// <summary>
    /// コミュニケーション鯖に接続
    /// </summary>
    [PacketHandlerAttr(0x7001)]
    class BCSConnect : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            //EBX+1C1330
            ushort Unknown1 = reader.ReadUInt16();

            reader.BaseStream.Seek(0x06, System.IO.SeekOrigin.Current);

            //ユーザID
            string userID = reader.ReadSjis(0x14);

            //キャラ名
            string charName = reader.ReadSjis(0x12);

            //プレイヤー取得
            using (var db = new gameContext())
            {
                var task = db.Players.SingleAsync(t => t.UserID == userID && t.Name == charName);
                task.Wait();
                context.User = task.Result;
                //context.User.LoadItems(db, Model.Player.TargetItemArea.ALL);
            }

            //ギルド表示インデックス
            ushort guildDisplayIndex = reader.ReadUInt16();

            context.SendPacket(new BCSDataPacket());
        }
    }
}
