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
    /// 露店終了
    /// </summary>
    [PacketHandlerAttr(0x1095)]
    class PitchmanShopClose : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            ushort shopID = reader.ReadUInt16();

            //削除
            MAPServer map = MAPServer.AllMapServers[context.User.MapSerial];
            map.PitchmanShops.RemoveAll(t => t.ShopID == shopID);

            //周囲に通知
            map.SendPacketsToPlayersOnTheMap(new RemovePitchmanShop(shopID));
        }
    }
}
