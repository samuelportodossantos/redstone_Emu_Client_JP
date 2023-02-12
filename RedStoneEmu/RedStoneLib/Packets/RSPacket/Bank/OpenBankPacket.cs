using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedStoneLib.Packets.RSPacket.Bank
{
    public class OpenBankPacket : Packet
    {
        readonly Model.Bank Bank;
        readonly byte BankTitleLevel;

        public OpenBankPacket(Model.Bank bank, byte bankTitleLevel)
        {
            Bank = bank;
            BankTitleLevel = bankTitleLevel;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write((ushort)0xCCCC);
            writer.WriteSjis(Bank.UserID, 0x14);
            writer.Write(Bank.Gold);
            writer.Write(Bank.Unknown);//unkown 個数？
            writer.Write((ushort)0xCCCC);

            //アイテム
            foreach(var item in Bank.Items)
            {
                writer.Write(item);
            }
            writer.Write((ushort)0x44);//?
            writer.Write(Bank.BankSession);
            writer.Write((ushort)Model.Bank.GetMaxSlotCount(BankTitleLevel));
            writer.Write((ushort)(Model.Bank.GetBankTax(BankTitleLevel) * 100.0));//金利

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x11A7);
        }
    }
}
