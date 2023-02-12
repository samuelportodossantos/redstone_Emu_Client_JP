using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedStoneLib.Packets.RSPacket.Items
{
    public class RemoveItem : Packet
    {
        public enum ItemRemoveType : ushort
        {
            /// <summary>
            /// 通常の消去
            /// </summary>
            RemoveItem = 0,

            /// <summary>
            /// G倉庫に格納
            /// </summary>
            PushGuildBank = 1,

            /// <summary>
            /// U分解
            /// </summary>
            Disassemble = 2
        }

        ItemRemoveType RemoveType;
        ushort ItemID, Num;

        public RemoveItem(ushort itemID, ushort num, ItemRemoveType removeType)
        {
            ItemID = itemID;
            Num = num;
            RemoveType = removeType;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write(ItemID);
            writer.Write(Num);
            writer.Write((ushort)RemoveType);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x11CB);
        }
    }
}
