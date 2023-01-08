using System;
using System.Collections.Generic;
using System.Text;
using RedStoneEmu.Games;
using RedStoneLib;
using RedStoneLib.Model;
using RedStoneLib.Packets;
using RedStoneLib.Packets.RSPacket.Trade;

namespace RedStoneEmu.Packets.Handlers.Trade
{
    /// <summary>
    /// 取引申請
    /// </summary>
    [PacketHandlerAttr(0x1050)]
    class RequestTrade : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            ushort charID = reader.ReadUInt16();
            string playerName = reader.ReadSjis();

            MAPServer map = MAPServer.AllMapServers[context.User.MapSerial];

            if (!map.Actors.TryGetValue(charID, out var target_actor))
            {
                //見つからない
                context.SendPacket(new RequestTradeResultPacket(RequestTradeResultPacket.RequestTradeResult.NotFound));
                return;
            }
            Player target = (Player)target_actor;

            // 4レベル未満

            // コミュ拒否

            // GM
            if (target.GMLevel > 0)
            {
                context.SendPacket(new RequestTradeResultPacket(RequestTradeResultPacket.RequestTradeResult.GM));
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

            // 既に取引中

            // 銀行中


            // 取引OK
            map.SendPackets[target.CharID](new RequestTradePacket(context.User.Name));
        }
    }
}
