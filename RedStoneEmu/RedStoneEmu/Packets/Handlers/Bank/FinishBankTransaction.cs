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
    /// 銀行取引完了
    /// </summary>
    [PacketHandlerAttr(0x1063)]
    class FinishBankTransaction : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            ushort unk1 = reader.ReadUInt16();
            uint checkSum = reader.ReadUInt32();
            ushort bankCode = reader.ReadUInt16();
            ushort unk2 = reader.ReadUInt16();

            var bank = RedStoneLib.Model.Bank.AllBanks[context.User.BankSessionID.Value];
            var bankCheckSum = bank.GetCheckSum(context.User);
            if (checkSum != bankCheckSum) return;
            if (bankCode != bank.BankKey) return;


            //context.SendPacket(new MoveBankItemPacket(fromPos, toPos, bankCheckSum));
        }
    }
}
