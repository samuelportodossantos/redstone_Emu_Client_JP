using RedStoneEmu.Games;
using RedStoneLib.Packets;
using RedStoneLib.Packets.RSPacket.PitchmanShop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedStoneEmu.Packets.Handlers.PitchmanShop
{
    /// <summary>
    /// 露店オープン・中断
    /// </summary>
    [PacketHandlerAttr(0x1091)]
    class ChangePitchmanShopInfo : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            ushort shopID = reader.ReadUInt16();
            ushort unk = reader.ReadUInt16();
            
            //ステート変化
            MAPServer map = MAPServer.AllMapServers[context.User.MapSerial];
            Pitchman shop = map.PitchmanShops[shopID];
            shop.State = (ChangePitchmanShopInfoPacket.ShopState)reader.ReadUInt16();

            //文字変化
            shop.Advertising = reader.ReadSjis(24);

            map.SendPacketsToPlayersOnTheMap(new ChangePitchmanShopInfoPacket(shopID, shop.State, shop.PosX, shop.PosY, shop.Advertising));
        }
    }
}
