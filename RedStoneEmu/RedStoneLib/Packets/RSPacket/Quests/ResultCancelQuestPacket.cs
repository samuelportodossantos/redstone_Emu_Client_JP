using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedStoneLib.Packets.RSPacket.Quests
{
    public class ResultCancelQuestPacket : Packet
    {
        ushort Index, IsFailed;

        /// <summary>
        /// 成功
        /// </summary>
        /// <param name="index"></param>
        public ResultCancelQuestPacket(ushort index)
        {
            Index = index;
            IsFailed = 0;
        }

        /// <summary>
        /// 失敗
        /// </summary>
        public ResultCancelQuestPacket()
        {
            Index = 0;
            IsFailed = 1;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write(Index);
            writer.Write(IsFailed);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x11CC);
        }
    }
}
