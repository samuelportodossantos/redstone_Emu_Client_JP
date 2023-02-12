using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedStoneLib.Packets.RSPacket.PitchmanShop
{
    /// <summary>
    /// 露店削除
    /// </summary>
    public class RemovePitchmanShop : Packet
    {
        readonly uint ShopID;

        public RemovePitchmanShop(uint shopID)
        {
            ShopID = shopID;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write(ShopID);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x120B);
        }
    }
}
