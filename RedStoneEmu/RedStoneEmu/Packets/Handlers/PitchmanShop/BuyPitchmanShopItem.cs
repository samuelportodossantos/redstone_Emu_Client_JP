using RedStoneEmu.Games;
using RedStoneLib.Model;
using RedStoneLib.Model.Base;
using RedStoneLib.Packets;
using RedStoneLib.Packets.RSPacket.PitchmanShop;
using RedStoneLib.Packets.RSPacket.Speech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedStoneEmu.Packets.Handlers.PitchmanShop
{
    /// <summary>
    /// 露店アイテムお買い上げ
    /// </summary>
    [PacketHandlerAttr(0x1092)]
    class BuyPitchmanShopItem : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            ushort shopID = reader.ReadUInt16();
            ushort itemPos = reader.ReadUInt16();
            ushort unk = reader.ReadUInt16();
            uint price = reader.ReadUInt32();
            byte isIngot = reader.ReadByte();
            Item item = reader.ReadItem();
            
            MAPServer map = MAPServer.AllMapServers[context.User.MapSerial];

            //店がない
            if (!map.PitchmanShops.Exists(t => t.ShopID == shopID))
            {
                context.SendPacket(new PitchmanShopMessagePacket(PitchmanShopMessagePacket.PitchmanShopResult.ChangedInformation));
                return;
            }

            //アイテムない
            Pitchman shop = map.PitchmanShops.Single(t => t.ShopID == shopID);
            if (shop.ItemSlots[itemPos].slot==-1)
            {
                context.SendPacket(new PitchmanShopMessagePacket(PitchmanShopMessagePacket.PitchmanShopResult.ChangedInformation));
                return;
            }

            //アイテムチェック

            //空きがない
            if (context.User.InventoryItems.EmptySpaceCount <= 0)
            {
                context.SendPacket(new PitchmanShopMessagePacket(PitchmanShopMessagePacket.PitchmanShopResult.FullInventory));
                return;
            }

            //露店主系
            Player pitchman = shop.GetPlayer();
            byte isTargetIngot = shop.ItemSlots[itemPos].isIngot;
            if (isIngot != isTargetIngot)
            {
                context.SendPacket(new Chat(context.User.CharID, Chat.ChatType.noticeChat, "GM", "インゴバグやめてください", true));
                return;
            }

            int ingotStack = ItemBase.AllItemBases[0x108D].StackableNum;
            if (isIngot == 0)
            {
                //ゴールドが足りない
                if (context.User.Gold < price)
                {
                    context.SendPacket(new PitchmanShopMessagePacket(PitchmanShopMessagePacket.PitchmanShopResult.NotEnoughMoney));
                    return;
                }
                //ゴールド多すぎ
                if (pitchman.Gold + price >= 2000000000)
                {
                    context.SendPacket(new PitchmanShopMessagePacket(PitchmanShopMessagePacket.PitchmanShopResult.FullMoney_Master));
                    shop.SendPacket(new PitchmanShopMessagePacket(PitchmanShopMessagePacket.PitchmanShopResult.FullMoney));
                    return;
                }
            }
            else
            {
                //インゴが足りない
                if (context.User.InventoryItems.GetItemCountByItemIndex(0x108D) < price)
                {
                    context.SendPacket(new PitchmanShopMessagePacket(PitchmanShopMessagePacket.PitchmanShopResult.NotEnoughIngot));
                    return;
                }
                //インゴの空きがない
                if (pitchman.InventoryItems.EmptySpaceCount < Math.Floor((double)price / ingotStack))
                {
                    context.SendPacket(new PitchmanShopMessagePacket(PitchmanShopMessagePacket.PitchmanShopResult.NotEnoughInventory_Master));
                    return;
                }
            }

            //実態取得
            Item targetItem = pitchman.InventoryItems[shop.ItemSlots[itemPos].slot];
            uint targetPrice = shop.ItemSlots[itemPos].price;

            if (isTargetIngot == 0)
            {
                //ゴールド移動
                context.User.Gold -= targetPrice;
                pitchman.Gold += targetPrice;
            }
            else
            {
                //インゴット移動
                //減算
                if(!context.User.InventoryItems.EjectItem(0x108D, (int)targetPrice))
                {
                    context.SendPacket(new PitchmanShopMessagePacket(PitchmanShopMessagePacket.PitchmanShopResult.NotEnoughIngot));
                    return;
                }
                //加算
                int fullStackCount = (int)Math.Floor((double)targetPrice / ingotStack);//インゴ10本が何個
                List<Item> addIngots = Enumerable.Range(0, fullStackCount).Select(_ => new Item(0x108D, (byte)ingotStack)).ToList();//追加されるインゴットのリスト
                addIngots.Add(new Item(0x108D, (byte)((int)targetPrice - fullStackCount * ingotStack)));
                foreach(var ingot in addIngots)//追加
                {
                    pitchman.InventoryItems.InsertItem(ingot);
                }
            }
            //アイテム移動
            pitchman.InventoryItems[shop.ItemSlots[itemPos].slot] = new Item();
            context.User.InventoryItems.InsertItem(targetItem);

            //購入成功通知
            context.SendPacket(new BuyPitchmanShopItemPacket(shopID, itemPos, item, price, isIngot, shop.PlayerName));

            //売却成功通知
            shop.SendPacket(new SoldPitchmanShopItemPacket(itemPos, price, isIngot, context.User.Name));
            
        }
    }
}
