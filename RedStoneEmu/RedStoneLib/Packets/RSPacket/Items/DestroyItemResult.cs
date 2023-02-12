using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RedStoneLib.Model.Base.ItemBase;

namespace RedStoneLib.Packets.RSPacket.Items
{
    /// <summary>
    /// 装備つける
    /// </summary>
    public class DestroyItemResult : Packet
    {
        ushort ItemIndex;
        ItemResult Result;

        /// <summary>
        /// 失敗
        /// </summary>
        /// <param name="result"></param>
        public DestroyItemResult(ItemResult result)
        {
            Result = result;
        }

        /// <summary>
        /// 成功
        /// </summary>
        /// <param name="itemIndex"></param>
        public DestroyItemResult(ushort itemIndex)
        {
            Result = ItemResult.OK;
            ItemIndex = itemIndex;
        }

        public override byte[] Build()
        {

            var writer = new PacketWriter();

            writer.Write((ushort)Result);
            writer.Write(ItemIndex);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x11E9);
        }
    }
}
