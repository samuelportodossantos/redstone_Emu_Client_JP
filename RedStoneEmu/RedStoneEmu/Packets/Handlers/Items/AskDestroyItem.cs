using RedStoneEmu.Games;
using RedStoneEmu.Packets.Handlers;
using RedStoneLib.Model;
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
    /// アイテム捨てる
    /// </summary>
    [PacketHandlerAttr(0x1082)]
    class AskDestroyItem : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            ushort checkSum = reader.ReadUInt16();
            ushort itemIndex = reader.ReadUInt16();

            Item targetItem = context.User.InventoryItems[itemIndex];
            if (targetItem.IsEmpty) return;

            /*if (targetItem.CheckSum != checkSum)
            {
                //チェックサム
                context.SendPacket(new DestroyItemResult(ItemBase.ItemResult.ItemInfoError2));
            }else */if (targetItem.Base.Flags.HasFlag(ItemBase.ItemFlags.CantDestruction))
            {
                //破壊不可能
                context.SendPacket(new DestroyItemResult(ItemBase.ItemResult.CantDestroy));
            }
            else
            {
                //破壊
                Item destroyItem = context.User.InventoryItems[itemIndex];
                ((GameClient)context).RemoveDBItems.Add(destroyItem);
                context.User.InventoryItems[itemIndex] = new Item();
                context.SendPacket(new DestroyItemResult(itemIndex));
            }
        }
    }
}
