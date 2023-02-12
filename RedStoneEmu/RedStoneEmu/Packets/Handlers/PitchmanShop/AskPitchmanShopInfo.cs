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
    /// 露店開いた時の処理
    /// </summary>
    [PacketHandlerAttr(0x1093)]
    class AskPitchmanShopInfo : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            ushort shopID = reader.ReadUInt16();
            
            MAPServer map = MAPServer.AllMapServers[context.User.MapSerial];

            //店がない
            if (!map.PitchmanShops.Exists(t => t.ShopID == shopID))
            {
                context.SendPacket(new PitchmanShopMessagePacket(PitchmanShopMessagePacket.PitchmanShopResult.ChangedInformation));
                return;
            }

            Pitchman shop = map.PitchmanShops.Single(t => t.ShopID == shopID);

            //準備中
            if (shop.State == ChangePitchmanShopInfoPacket.ShopState.Preparing)
            {
                context.SendPacket(new PitchmanShopMessagePacket(PitchmanShopMessagePacket.PitchmanShopResult.Preparation));
                return;
            }

            //通知
            context.SendPacket(new PitchmanShopInfoPacket(shop.InnerData()));
        }
    }
}
