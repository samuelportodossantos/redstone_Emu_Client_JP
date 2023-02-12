using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RedStoneEmu.Database.RedStoneEF;
using RedStoneLib.Model;
using RedStoneLib.Packets;
using RedStoneLib.Packets.RSPacket.Bank;

namespace RedStoneEmu.Packets.Handlers.Bank
{
    /// <summary>
    /// 銀行からアイテム取り出す
    /// </summary>
    [PacketHandlerAttr(0x105F)]
    class WithdrawItemFromTheBank : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            ushort junk = reader.ReadUInt16();
            uint checkSum = reader.ReadUInt32();
            ushort invePos = reader.ReadUInt16();
            ushort bankPos = reader.ReadUInt16();
            ushort bankCode = reader.ReadUInt16();
            ushort unk = reader.ReadUInt16();

            var bank = RedStoneLib.Model.Bank.AllBanks[context.User.BankSessionID.Value];
            var bankCheckSum = bank.GetCheckSum(context.User);
            if (checkSum != bankCheckSum) return;
            if (bankCode != bank.BankKey) return;

            //アイテム取り出す
            var targetItem = bank.Items[bankPos];
            bank.Items[bankPos] = new Item();
            bank.TemporaryInventory[invePos] = targetItem;

            context.SendPacket(new WithdrawItemFromTheBankPacket(bankCheckSum, invePos, bankPos));
        }
    }
}
