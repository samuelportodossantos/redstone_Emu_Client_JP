using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RedStoneEmu.Games;
using RedStoneLib;
using RedStoneLib.Model;
using RedStoneLib.Packets;
using RedStoneLib.Packets.RSPacket.Trade;

namespace RedStoneEmu.Packets.Handlers.Trade
{
    /// <summary>
    /// 取引アイテム追加
    /// </summary>
    [PacketHandlerAttr(0x1053)]
    class AddTradeItem : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            ushort inveSlot = reader.ReadUInt16();
            ushort itemCount = reader.ReadUInt16();

            //取引モデル
            var trade = Games.Trade.AllTrades[context.User.TradeID.Value];
            var items = trade.MyItems(context.User.Name);

            //取引アイテム
            Item item = null;
            if (inveSlot >= 100)
            {
                item = context.User.EquipmentItems[inveSlot - 100];
            }
            else
            {
                item = context.User.InventoryItems[inveSlot];
            }

            int index = items.FindIndex(t => t.inveSlot == inveSlot);
            if (index>=0)
            {
                //個数変更
                items[index] = (inveSlot, itemCount, item);
            }
            else
            {
                //新規追加
                items.Add((inveSlot, itemCount, item));
            }

            //パケット
            (var me, var mate) = trade.GetSendPackets(context.User.Name);
            me(new AddTradeItemByMePacket(inveSlot, itemCount));
            mate(new AddTradeItemByTradeMatePacket(item));
            trade.ResetReady();
        }
    }
}
