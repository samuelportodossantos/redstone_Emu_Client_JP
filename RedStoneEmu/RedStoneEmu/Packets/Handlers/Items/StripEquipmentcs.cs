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
    /// 装備アイテム脱ぐ
    /// </summary>
    [PacketHandlerAttr(0x104E)]
    class StripEquipmentcs : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            ushort checkSum = reader.ReadUInt16();
            ushort equipPos = reader.ReadUInt16();

            //エラーチェック
            var result = ItemBase.ItemResult.OK;
            if (context.User.InventoryItems.IsFull)
                result = ItemBase.ItemResult.NotEmptyInventory;
            //バトル中

            //脱ぐ処理
            if (result == ItemBase.ItemResult.OK)
            {
                var stripItem = context.User.EquipmentItems[equipPos];
                context.User.InventoryItems.InsertItem(stripItem);
                context.User.EquipmentItems[equipPos] = new RedStoneLib.Model.Item();
            }
            context.SendPacket(new StripEquipmentResult(result, equipPos));
        }
    }
}
