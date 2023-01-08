using RedStoneLib.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Packets.RSPacket.Items
{
    /// <summary>
    /// アイテム取得
    /// </summary>
    public class AddItemPacket : Packet
    {
        public enum ItemGetType : ushort
        {
            /// <summary>
            /// 通常の入手
            /// </summary>
            AddItem = 0,

            /// <summary>
            /// G倉庫から取得
            /// </summary>
            PickGuildBank = 1,

            /// <summary>
            /// 鏡成功
            /// </summary>
            SuccessDupe = 2
        }

        ItemGetType Gettype;
        Item Item;

        public AddItemPacket(Item item, ItemGetType gettype)
        {
            Item = item;
            Gettype = gettype;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write(Item);
            writer.Write((ushort)Gettype);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x11A2);
        }
    }
}
