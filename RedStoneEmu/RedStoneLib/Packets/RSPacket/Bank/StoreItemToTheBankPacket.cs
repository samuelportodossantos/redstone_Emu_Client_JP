using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedStoneLib.Packets.RSPacket.Bank
{
    public class StoreItemToTheBankPacket : Packet
    {
        readonly uint CheckSum;
        readonly ushort InveSlot, BankSlot;

        public StoreItemToTheBankPacket(uint checkSum, ushort inveSlot, ushort bankSlot)
        {
            CheckSum = checkSum;
            InveSlot = inveSlot;
            BankSlot = bankSlot;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write((ushort)0xCCCC);
            writer.Write(CheckSum);
            writer.Write(InveSlot);
            writer.Write(BankSlot);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x11BE);
        }
    }
}
