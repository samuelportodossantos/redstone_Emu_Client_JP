using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedStoneLib.Packets.RSPacket.PitchmanShop
{
    /// <summary>
    /// 露店開始準備
    /// </summary>
    public class ChangePitchmanShopInfoPacket : Packet
    {
        readonly uint ShopID;
        readonly ShopState State;
        readonly ushort PosX, PosY;
        readonly string Advertising;
        
        public enum ShopState : ushort
        {
            /// <summary>
            /// 準備中
            /// </summary>
            Preparing = 0,

            /// <summary>
            /// 販売中
            /// </summary>
            Sales = 4,
        }

        public ChangePitchmanShopInfoPacket(uint shopID, ShopState state, ushort posX, ushort posY, string advertising)
        {
            ShopID = shopID;
            State = state;
            PosX = posX;
            PosY = posY;
            Advertising = advertising;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write(ShopID);
            writer.Write((ushort)(0x80 | (ushort)State));
            writer.Write(PosX);
            writer.Write(PosY);
            writer.WriteSjis(Advertising);
            writer.Write((byte)0);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1207);
        }
    }
}
