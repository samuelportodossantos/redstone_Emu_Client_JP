using RedStoneLib.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedStoneLib.Packets.RSPacket.Trade
{
    /// <summary>
    /// 取引を実行
    /// </summary>
    public class TradePacket : Packet
    {
        readonly (int inveSlot, int num)[] SendItemIndices;
        readonly Item[] ReceivedItems;
        readonly uint SendGolds, ReceivedGolds;

        /// <summary>
        /// 取引を実行
        /// </summary>
        /// <param name="ItemsToPass">渡すアイテム</param>
        /// <param name="ItemsReceived">受け取るアイテム</param>
        public TradePacket((int inveSlot, int num)[] sendItemIndices, Item[] receivedItems, uint sendGolds, uint receivedGolds)
        {
            SendItemIndices = sendItemIndices;
            ReceivedItems = receivedItems;
            SendGolds = sendGolds;
            ReceivedGolds = receivedGolds;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            //受け取るアイテムの個数
            writer.Write((ushort)ReceivedItems.Length);

            //ゴールド
            writer.Write(ReceivedGolds);
            writer.Write(SendGolds);

            writer.Write((ushort)8);

            //渡すアイテム
            for(int i = 0; i < 18; i++)
            {
                if (SendItemIndices.Length <= i)
                {
                    writer.Write(uint.MaxValue);
                }
                else
                {
                    writer.Write((ushort)SendItemIndices[i].inveSlot);
                    writer.Write((ushort)SendItemIndices[i].num);
                }
            }

            //受け取るアイテム
            foreach(var item in ReceivedItems)
            {
                writer.Write(item);
            }
            writer.Write((ushort)0xFFFF);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1199);
        }
    }
}
