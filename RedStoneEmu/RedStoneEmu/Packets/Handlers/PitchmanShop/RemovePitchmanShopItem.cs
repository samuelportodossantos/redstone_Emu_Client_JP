using RedStoneEmu.Games;
using RedStoneLib.Model;
using RedStoneLib.Packets;
using RedStoneLib.Packets.RSPacket.PitchmanShop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedStoneEmu.Packets.Handlers.PitchmanShop
{
    /// <summary>
    /// 露店アイテム削除
    /// </summary>
    [PacketHandlerAttr(0x1090)]
    class RemovePitchmanShopItem : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            ushort shopPos = reader.ReadUInt16();
            ushort shopID = reader.ReadUInt16();
                        
            MAPServer map = MAPServer.AllMapServers[context.User.MapSerial];
            map.PitchmanShops[shopID].ItemSlots[shopPos] = (-1, new Item(), 0, 0);

            context.SendPacket(new RemovePitchmanShopItemPacket(shopPos));
        }
    }
}
