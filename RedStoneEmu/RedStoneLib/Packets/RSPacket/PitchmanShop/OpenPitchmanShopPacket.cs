using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedStoneLib.Packets.RSPacket.PitchmanShop
{
    /// <summary>
    /// 露店開始準備
    /// </summary>
    public class OpenPitchmanShopPacket : Packet
    {
        readonly uint ShopID;
        readonly ushort PosX, PosY;

        public OpenPitchmanShopPacket(uint shopID, ushort posX, ushort posY)
        {
            ShopID = shopID;
            PosX = posX;
            PosY = posY;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write(ShopID);
            writer.Write(PosX);
            writer.Write(PosY);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1204);
        }
    }
}
