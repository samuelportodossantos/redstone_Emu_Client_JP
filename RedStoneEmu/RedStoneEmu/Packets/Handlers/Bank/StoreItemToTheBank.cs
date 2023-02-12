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
    /// 銀行にアイテム格納
    /// </summary>
    [PacketHandlerAttr(0x105D)]
    class StoreItemToTheBank : PacketHandler
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

            //アイテム挿入
            Item targetItem = null;
            if (invePos >= 100)
            {
                targetItem = bank.TemporaryEquipment[invePos - 100];
                bank.TemporaryEquipment[invePos - 100] = new Item();
            }
            else
            {
                targetItem = bank.TemporaryInventory[invePos];
                bank.TemporaryInventory[invePos] = new Item();
            }
            bank.Items[bankPos] = targetItem;

            context.SendPacket(new StoreItemToTheBankPacket(bankCheckSum, invePos, bankPos));
        }
    }
}
