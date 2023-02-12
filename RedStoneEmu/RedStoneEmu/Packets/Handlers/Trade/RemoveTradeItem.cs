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
    /// 取引アイテム削除
    /// </summary>
    [PacketHandlerAttr(0x1054)]
    class RemoveTradeItem : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            ushort tradeSlot = reader.ReadUInt16();

            //取引モデル
            var trade = Games.Trade.AllTrades[context.User.TradeID.Value];
            var items = trade.MyItems(context.User.Name);

            //削除
            items.RemoveAt(tradeSlot);

            //パケット
            var packet = new RemoveTradeItemPacket(context.User.CharID, tradeSlot);
            (var me, var mate) = trade.GetSendPackets(context.User.Name);
            me(packet);
            mate(packet);
        }
    }
}
