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
    /// 取引中止
    /// </summary>
    [PacketHandlerAttr(0x1052)]
    class CancelTrade : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            if(Games.Trade.AllTrades.TryGetValue(context.User.TradeID.Value, out var trade))
            {
                //パートナーの処理
                (var target, var targetSendPacket) = trade.GetPartner(context.User.Name);
                targetSendPacket(new TradeMessagePacket(RedStoneLib.Model.Base.ItemBase.ItemResult.Trade_Cancel2));

                //トレード消去
                Games.Trade.AllTrades.Remove(trade.TradeID);
            }

            //自分の処理
            context.User.TradeID = null;
        }
    }
}
