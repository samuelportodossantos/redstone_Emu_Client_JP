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
    /// 取引ゴールドセット
    /// </summary>
    [PacketHandlerAttr(0x1055)]
    class SetTradeGold : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            ushort unk = reader.ReadUInt16();
            uint gold = reader.ReadUInt32();

            //所持量下回る
            if (context.User.Gold < gold) return;

            //取引モデル
            var trade = Games.Trade.AllTrades[context.User.TradeID.Value];

            //ゴールドセット
            trade.SetGold(context.User.Name, gold);

            //パケット
            Packet packet = null;
            var mate = trade.GetMate(context.User.Name);
            if (mate.Gold + gold > RedStoneApp.GameServer.Config.PossessionGoldLimit)
            {
                //所持量超過
                packet = new TradeMessagePacket(RedStoneLib.Model.Base.ItemBase.ItemResult.LimitGold);
            }
            else
            {
                packet = new SetTradeGoldPacket(context.User.CharID, gold);
            }
            (var sendMe, var sendMate) = trade.GetSendPackets(context.User.Name);
            sendMe(packet);
            sendMate(packet);
            trade.ResetReady();
        }
    }
}
