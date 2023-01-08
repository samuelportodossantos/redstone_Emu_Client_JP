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
    public class EquipItemResult : Packet
    {
        ushort ItemFrom, MoveToItemCollection;
        ItemResult Result;

        public EquipItemResult(ItemResult result, ushort itemFrom, ushort moveToItemCollection)
        {
            Result = result;
            ItemFrom = itemFrom;
            MoveToItemCollection = moveToItemCollection;
        }

        public override byte[] Build()
        {

            var writer = new PacketWriter();

            writer.Write((ushort)Result);
            writer.Write(ItemFrom);
            writer.Write(MoveToItemCollection);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1139);
        }
    }
}
