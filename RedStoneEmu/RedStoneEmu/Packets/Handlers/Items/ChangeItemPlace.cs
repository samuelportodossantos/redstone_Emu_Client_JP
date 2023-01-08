using RedStoneEmu.Packets.Handlers;
using RedStoneLib.Packets;
using RedStoneLib.Packets.RSPacket.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneEmu.Packets.Handlers.Items
{
    /// <summary>
    /// アイテム場所移動
    /// </summary>
    [PacketHandlerAttr(0x1035)]
    class ChangeItemPlace : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            ushort checkSum = reader.ReadUInt16();
            ushort itemFrom = reader.ReadUInt16();
            ushort itemTo = reader.ReadUInt16();

            if(context.User.InventoryItems.ChangePlace(itemFrom, itemTo))
            {
                context.SendPacket(new ChangeItemPlaceResult(itemFrom, itemTo));
            }
        }
    }
}
