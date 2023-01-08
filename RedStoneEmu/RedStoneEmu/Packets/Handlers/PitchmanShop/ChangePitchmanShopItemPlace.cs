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
    /// 露店アイテム位置変更
    /// </summary>
    [PacketHandlerAttr(0x1094)]
    class ChangePitchmanShopItemPlace : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            ushort shopID = reader.ReadUInt16();
            ushort fromPos = reader.ReadUInt16();
            ushort toPos = reader.ReadUInt16();
            if (fromPos == toPos) return;
            if (fromPos > 9 || toPos > 9) return;

            //入れ替え
            MAPServer map = MAPServer.AllMapServers[context.User.MapSerial];
            var tmp = map.PitchmanShops[shopID].ItemSlots[fromPos];
            map.PitchmanShops[shopID].ItemSlots[fromPos] = map.PitchmanShops[shopID].ItemSlots[toPos];
            map.PitchmanShops[shopID].ItemSlots[toPos] = tmp;

            context.SendPacket(new ChangePitchmanShopItemPlacePacket(fromPos, toPos));
        }
    }
}
