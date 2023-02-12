using RedStoneLib.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RedStoneLib.Model.Base.ItemBase;

namespace RedStoneLib.Packets.RSPacket.Items
{
    /// <summary>
    /// 装備拾う
    /// </summary>
    public class PickItemResult : Packet
    {
        ItemResult Result;
        Item PickedItem = null;

        /// <summary>
        /// 失敗
        /// </summary>
        /// <param name="result"></param>
        public PickItemResult(ItemResult result)
        {
            Result = result;
        }

        /// <summary>
        /// 成功
        /// </summary>
        /// <param name="item"></param>
        public PickItemResult(Item item)
        {
            PickedItem = item;
            Result = ItemResult.OK;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write((ushort)Result);
            if (Result == ItemResult.OK)
            {
                writer.Write(PickedItem);
            }

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x118F);
        }
    }
}
