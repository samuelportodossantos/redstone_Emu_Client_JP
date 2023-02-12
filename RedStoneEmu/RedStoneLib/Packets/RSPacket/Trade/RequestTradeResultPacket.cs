using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedStoneLib.Packets.RSPacket.Trade
{
    /// <summary>
    /// 取引申請結果
    /// </summary>
    public class RequestTradeResultPacket : Packet
    {
        public enum RequestTradeResult : ushort
        {
            DeclineTrade = 0,//	取引要請を断りました
            TooFar = 1,//	取引要請相手との距離が遠すぎます
            NotFound = 2,//	取引要請相手が見つかりません
            AlreadyTrading = 3,//	既に取引中です
            Dead = 4,//	死んだキャラクターとは取引できません
            ManyTrade = 5,//	取引が多すぎます
            GM = 6,//	運営者に取引の申し込みはできません
            RejectCommunity = 7,//	対象がコミュニティ拒否状態です
            GreaterThan4 = 8,//	取引するには4レベル以上が必要
            CannotTrade = 9,//	取引できる状況ではありません
        }

        readonly RequestTradeResult Result;

        public RequestTradeResultPacket(RequestTradeResult result)
        {
            Result = result;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write((ushort)Result);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1191);
        }
    }
}
