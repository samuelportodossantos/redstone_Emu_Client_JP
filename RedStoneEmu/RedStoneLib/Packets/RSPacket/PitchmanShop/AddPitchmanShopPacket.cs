using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedStoneLib.Packets.RSPacket.PitchmanShop
{
    /// <summary>
    /// 露店開始を周囲に通知
    /// </summary>
    public class AddPitchmanShopPacket : Packet
    {
        readonly uint ShopID;
        readonly ushort CharID, PosX, PosY;

        public AddPitchmanShopPacket(uint shopID, ushort charID, ushort posX, ushort posY)
        {
            ShopID = shopID;
            CharID = charID;
            PosX = posX;
            PosY = posY;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write(ShopID);
            writer.Write((ushort)(CharID << 4));
            writer.Write(PosX);
            writer.Write(PosY);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x120A);
        }
    }
}
