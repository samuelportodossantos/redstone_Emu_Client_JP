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
    /// 銀行取引中にインベアイテム移動
    /// </summary>
    [PacketHandlerAttr(0x1062)]
    class MoveInventoryItemWhenBankTransaction : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            ushort fromPos = reader.ReadUInt16();
            ushort toPos = reader.ReadUInt16();
            ushort junk = reader.ReadUInt16();
            uint checkSum = reader.ReadUInt32();
            ushort bankCode = reader.ReadUInt16();

            var bank = RedStoneLib.Model.Bank.AllBanks[context.User.BankSessionID.Value];
            var bankCheckSum = bank.GetCheckSum(context.User);
            if (checkSum != bankCheckSum) return;
            if (bankCode != bank.BankKey) return;

            //移動
            var targetItem = bank.TemporaryInventory[fromPos];
            bank.TemporaryInventory[fromPos] = bank.TemporaryInventory[toPos];
            bank.TemporaryInventory[toPos] = targetItem;

            context.SendPacket(new MoveInventoryItemWhenBankTransactionPacket(fromPos, toPos, bankCheckSum));
        }
    }
}
