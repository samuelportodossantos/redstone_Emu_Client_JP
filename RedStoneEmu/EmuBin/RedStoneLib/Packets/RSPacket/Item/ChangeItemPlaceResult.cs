using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RedStoneLib.Model.Base.ItemBase;

namespace RedStoneLib.Packets.RSPacket.Item
{
    public class ChangeItemPlaceResult : Packet
    {
        ushort From, To;
        ItemResult Result;

        public ChangeItemPlaceResult(ushort from, ushort to)
        {
            From = from;
            To = to;
            Result = ItemResult.OK;
        }

        public ChangeItemPlaceResult(ItemResult result)
        {
            Result = result;
        }

        public override byte[] Build()
        {

            var writer = new PacketWriter();
            
            writer.Write((ushort)Result);
            writer.Write(From);
            writer.Write(To);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1145);
        }
    }
}
