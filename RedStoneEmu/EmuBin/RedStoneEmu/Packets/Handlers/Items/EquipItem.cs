using RedStoneEmu.Packets.Handlers;
using RedStoneLib.Model.Base;
using RedStoneLib.Packets;
using RedStoneLib.Packets.RSPacket.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneEmu.Packets.Handlers.Items
{
    /// <summary>
    /// アイテム装備
    /// </summary>
    [PacketHandlerAttr(0x102D)]
    class EquipItem : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            ushort checkSum = reader.ReadUInt16();
            ushort from = reader.ReadUInt16();//インベの場所
            ushort itemIndex = reader.ReadUInt16();
            ushort moveToItemCollection = reader.ReadUInt16();//アイテムコレクション移動先

            //エラーチェック
            var result = ItemBase.ItemResult.OK;
            ushort to = 0xffff;

            //装備移動先
            switch (ItemBase.AllItemBases[itemIndex].Type)
            {
                case ItemBase.ItemType.OneHandedSword:
                case ItemBase.ItemType.Spear:
                case ItemBase.ItemType.Whistle when context.User.Job == RedStoneLib.Model.Player.JOB.Tamer:
                case ItemBase.ItemType.Dagger:
                case ItemBase.ItemType.Club:
                case ItemBase.ItemType.Rod:
                case ItemBase.ItemType.Sling:
                case ItemBase.ItemType.Whip when context.User.Job == RedStoneLib.Model.Player.JOB.Necro:
                case ItemBase.ItemType.Sickle:
                case ItemBase.ItemType.Book:
                    to = 0;
                    break;
                case ItemBase.ItemType.Brooch:
                case ItemBase.ItemType.ShoulderTattoo:
                case ItemBase.ItemType.Cross:
                case ItemBase.ItemType.Shield:
                case ItemBase.ItemType.Arrow:
                case ItemBase.ItemType.Bottle:
                    to = 1;
                    break;
                case ItemBase.ItemType.Armor:
                case ItemBase.ItemType.Shokuyoroi:
                    to = 2;
                    break;
                case ItemBase.ItemType.Globe:
                case ItemBase.ItemType.Yarito:
                case ItemBase.ItemType.Crow:
                case ItemBase.ItemType.Wrist:
                case ItemBase.ItemType.ArmTattoo:
                    to = 3;
                    break;
                case ItemBase.ItemType.Hat:
                case ItemBase.ItemType.Head:
                    to = 4;
                    break;
                case ItemBase.ItemType.Ear:
                case ItemBase.ItemType.Back:
                    to = 5;
                    break;
                case ItemBase.ItemType.Neck:
                    to = 6;
                    break;
                case ItemBase.ItemType.Belt:
                    to = 7;
                    break;
                case ItemBase.ItemType.Shoes:
                    to = 8;
                    break;
                /*case ItemBase.ItemType.Ring:
                    to = 9//指輪保留*/
                case ItemBase.ItemType.DoubleHandedSword:
                case ItemBase.ItemType.Bow:
                case ItemBase.ItemType.Whistle when context.User.Job == RedStoneLib.Model.Player.JOB.Summoner:
                case ItemBase.ItemType.Wing:
                case ItemBase.ItemType.Fang:
                case ItemBase.ItemType.Steck:
                case ItemBase.ItemType.Whip when context.User.Job == RedStoneLib.Model.Player.JOB.Demon:
                case ItemBase.ItemType.FighterCrow:
                    to = 17;
                    break;
            }

            //装備済み
            if (!context.User.EquipmentItems[to].IsEmpty)
                result = ItemBase.ItemResult.AlreadyEquip;

            //装備つける
            if (result == ItemBase.ItemResult.OK)
            {
                var tmp = context.User.EquipmentItems[to];
                context.User.EquipmentItems[to] = context.User.InventoryItems[from];
                context.User.InventoryItems[from] = tmp;

                //パケ送信
                context.SendPacket(new EquipItemResult(result, from, moveToItemCollection));
            }
        }
    }
}
