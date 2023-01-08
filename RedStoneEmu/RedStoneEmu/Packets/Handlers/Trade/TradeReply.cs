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
    /// 取引申請返信
    /// </summary>
    [PacketHandlerAttr(0x1051)]
    class TradeReply : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            string name = reader.ReadSjis(0x12);
            ushort reply = reader.ReadUInt16();

            MAPServer map = MAPServer.AllMapServers[context.User.MapSerial];

            var target = map.Actors.Values.FirstOrDefault(t => t.Name == name) as Player;

            //見つからない
            if (target==null)
            {
                context.SendPacket(new RequestTradeResultPacket(RequestTradeResultPacket.RequestTradeResult.NotFound));
                return;
            }

            //取引拒否
            if (reply == 1)
            {
                map.SendPackets[target.CharID](new RequestTradeResultPacket(RequestTradeResultPacket.RequestTradeResult.DeclineTrade));
                return;
            }

            // 死んでる
            if (target.NowHP == 0)
            {
                context.SendPacket(new RequestTradeResultPacket(RequestTradeResultPacket.RequestTradeResult.Dead));
                return;
            }

            // 遠い
            if (Helper.GetDistance(context.User.PosX / 64, context.User.PosY / 32, target.PosX / 64, target.PosY / 32) > 20)
            {
                context.SendPacket(new RequestTradeResultPacket(RequestTradeResultPacket.RequestTradeResult.TooFar));
                return;
            }

            //トレードの作成
            if(Games.Trade.Register(target, context.User, map.SendPackets[target.CharID], map.SendPackets[context.User.CharID]))
            {
                context.SendPacket(new BeginTradePacket(target.CharID, context.User.CharID, target.Name));
                map.SendPackets[target.CharID](new BeginTradePacket(context.User.CharID, target.CharID, context.User.Name));
            }
            else
            {
                //トレード数多い
                context.SendPacket(new RequestTradeResultPacket(RequestTradeResultPacket.RequestTradeResult.ManyTrade));
                map.SendPackets[target.CharID](new RequestTradeResultPacket(RequestTradeResultPacket.RequestTradeResult.ManyTrade));
            }
        }
    }
}
