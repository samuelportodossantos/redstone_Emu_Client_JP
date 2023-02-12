using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedStoneLib.Packets.RSPacket.PitchmanShop
{
    /// <summary>
    /// 露店アイテム削除
    /// </summary>
    public class RemovePitchmanShopItemPacket : Packet
    {
        readonly uint ShopItemPos;

        public RemovePitchmanShopItemPacket(uint shopItemPos)
        {
            ShopItemPos = shopItemPos;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write(ShopItemPos);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1206);
        }
    }
}
