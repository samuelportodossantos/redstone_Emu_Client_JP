using RedStoneEmu.Games;
using RedStoneLib.Packets;
using RedStoneLib.Packets.RSPacket.PitchmanShop;
using System;
using System.Collections.Generic;
using System.Text;

namespace RedStoneEmu.Packets.Handlers.PitchmanShop
{
    /// <summary>
    /// 露店開店準備
    /// </summary>
    [PacketHandlerAttr(0x108D)]
    class AskOpenThePitchmanShop : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            //未使用
            ushort junk1 = reader.ReadUInt16();
            ushort junk2 = reader.ReadUInt16();

            //取引人チェック

            //銀行中or取引中

            //NPCの近く

            //ポータルの近く

            //他の露店の近く

            //露天数多すぎ

            //登録
            MAPServer map = MAPServer.AllMapServers[context.User.MapSerial];
            Pitchman pitchman = new Pitchman(context.User.MapSerial, context.User, context.User.PosX, context.User.PosY, packet => context.SendPacket(packet, false));
            map.PitchmanShops.Add(pitchman);

            //開始準備
            context.SendPacket(new OpenPitchmanShopPacket(pitchman.ShopID, pitchman.PosX, pitchman.PosY));

            //周囲に通知
            map.SendPacketsToPlayersOnTheMap(new AddPitchmanShopPacket(pitchman.ShopID, context.User.CharID, pitchman.PosX, pitchman.PosY));
        }
    }
}
