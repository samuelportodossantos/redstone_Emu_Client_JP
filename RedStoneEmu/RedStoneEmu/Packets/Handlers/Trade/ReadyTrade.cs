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
    /// 取引準備OK
    /// </summary>
    [PacketHandlerAttr(0x1056)]
    class ReadyTrade : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            //取引モデル
            var trade = Games.Trade.AllTrades[context.User.TradeID.Value];
            trade.SetReady(context.User.Name);
            
            //パケット
            var packet = new ReadyTradePacket(context.User.CharID);
            (var me, var mate) = trade.GetSendPackets(context.User.Name);
            me(packet);
            mate(packet);
        }
    }
}
