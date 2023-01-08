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
    /// マップからアイテム消す
    /// </summary>
    public class RemoveDropItem : Packet
    {
        ushort Index;

        public RemoveDropItem(ushort index)
        {
            Index = index;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write(Index);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1135);
        }
    }
}
