using RedStoneEmu.Games;
using RedStoneEmu.Packets.Handlers;
using RedStoneLib.Model.Base;
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
    /// アイテム拾う
    /// </summary>
    [PacketHandlerAttr(0x102C)]
    class PickFieldItem : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            ushort checkSum = reader.ReadUInt16();
            ushort dropItemIndex = reader.ReadUInt16();

            MAPServer map = MAPServer.AllMapServers[context.User.MapSerial];

            if(map.TryTakeDroppedItem(context.User, dropItemIndex, checkSum, out var pickedItem, out var resultPacket))
            {
                //取得成功
                context.User.InventoryItems.InsertItem(pickedItem);
                ((GameClient)context).AddDBItems.Add(pickedItem);
            }

            //パケット送信
            context.SendPacket(resultPacket);
        }
    }
}
