using RedStoneLib.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Packets.RSPacket.KarmaPacket
{
    /// <summary>
    /// NPCなどの店を開く
    /// </summary>
    public class OpenShopPacket : Packet
    {
        Shop SHOP;

        public OpenShopPacket(Shop shop)
        {
            SHOP = shop;
        }

        public override byte[] Build()
        {

            var writer = new PacketWriter();

            writer.Write((ushort)SHOP.Displayed.Count());
            writer.Write((ushort)0x12);
            writer.Write((ushort)(SHOP.Displayed.Count() & 0x7FFF | (SHOP.isBattlePoint ? 1 : 0) << 15));
            writer.Write((ushort)((SHOP.CostMagnification!=0?1:0) | SHOP.CostMagnification << 1));

            writer.Write((ushort)SHOP.Type);
            writer.Write((uint)SHOP.MaxPurchasePrice);
            writer.Write((ushort)SHOP.PaymentItem);//支払い対価　0xFFFF=ゴールド
            writer.Write((ushort)(SHOP.OnlyDisplay ? 1 : 0));
            writer.Write((uint)0);//checkSum?
            writer.WriteBytes(0, 0x1C);

            foreach (var v in SHOP.Displayed)
            {
                var writtenItem = v.item;
                writtenItem.UniqueID = (int)v.price;
                writer.WriteStruct(writtenItem.GetM_Value());
            }

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x115F);
        }
    }
}
