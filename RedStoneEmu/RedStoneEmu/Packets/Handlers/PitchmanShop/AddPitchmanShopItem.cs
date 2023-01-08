using RedStoneEmu.Games;
using RedStoneLib.Model;
using RedStoneLib.Model.Base;
using RedStoneLib.Packets;
using RedStoneLib.Packets.RSPacket.PitchmanShop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedStoneEmu.Packets.Handlers.PitchmanShop
{
    /// <summary>
    /// 露店にアイテム追加
    /// </summary>
    [PacketHandlerAttr(0x108F)]
    class AddPitchmanShopItem : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            ushort unk1 = reader.ReadUInt16();

            ushort invepos = reader.ReadUInt16();
            ushort shoppos = reader.ReadUInt16();
            uint price = reader.ReadUInt32();
            byte isIngot = reader.ReadByte();

            byte unk4 = reader.ReadByte();
            ushort unk5 = reader.ReadUInt16();

            //該当アイテムチェック
            Item item = context.User.InventoryItems[invepos];
            if (item.IsEmpty)
            {
                //空きスロット
                context.SendPacket(new PitchmanShopMessagePacket(PitchmanShopMessagePacket.PitchmanShopResult.EmptySlot1));
                return;
            }
            //取引不可能チェック



            //スロットチェック
            MAPServer map = MAPServer.AllMapServers[context.User.MapSerial];
            Pitchman pitchmanShop = map.PitchmanShops.First(t => t.PlayerName == context.User.Name);

            if (pitchmanShop.ItemSlots.Select(t => t.slot).Contains(invepos))
            {
                //既に登録されたアイテム
                context.SendPacket(new PitchmanShopMessagePacket(PitchmanShopMessagePacket.PitchmanShopResult.AlreadyRegistered));
                return;
            }

            //アイテム追加
            if (shoppos > 9) return;
            pitchmanShop.ItemSlots[shoppos] = (invepos, context.User.InventoryItems[invepos], price, isIngot);

            context.SendPacket(new AddPitchmanShopItemPacket(invepos, shoppos, price, isIngot));
        }
    }
}
