using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RedStoneLib.Model.Base.ItemBase;

namespace RedStoneLib.Packets.RSPacket.Item
{
    /// <summary>
    /// 装備外す
    /// </summary>
    public class StripEquipmentResult : Packet
    {
        ushort EquipIndex;
        ItemResult Result;

        public StripEquipmentResult(ItemResult result, ushort equipIndex)
        {
            Result = result;
            EquipIndex = equipIndex;
        }

        public override byte[] Build()
        {

            var writer = new PacketWriter();

            writer.Write(EquipIndex);
            writer.Write((ushort)Result);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1189);
        }
    }
}
