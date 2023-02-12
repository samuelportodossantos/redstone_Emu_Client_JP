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
    /// 銀行アイテム移動
    /// </summary>
    [PacketHandlerAttr(0x1061)]
    class MoveBankItem : PacketHandler
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
            var targetItem = bank.Items[fromPos];
            bank.Items[fromPos] = bank.Items[toPos];
            bank.Items[toPos] = targetItem;

            context.SendPacket(new MoveBankItemPacket(fromPos, toPos, bankCheckSum));
        }
    }
}
