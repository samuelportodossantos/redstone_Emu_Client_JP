using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RedStoneEmu.Games;
using RedStoneLib;
using RedStoneLib.Model;
using RedStoneLib.Packets;
using RedStoneLib.Packets.RSPacket.Trade;

namespace RedStoneEmu.Packets.Handlers.Trade
{
    /// <summary>
    /// 取引承認
    /// </summary>
    [PacketHandlerAttr(0x1057)]
    class PermitTrade : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            ushort unk = reader.ReadUInt16();
            uint checkSum = reader.ReadUInt32();

            //取引モデル
            var trade = Games.Trade.AllTrades[context.User.TradeID.Value];

            //パケット
            (var sendMe, var sendMate) = trade.GetSendPackets(context.User.Name);

            Packet myPacket = null;
            Packet matePacket = null;
            if (trade.Permit)
            {
                //取引開始
                var me = trade.GetMe(context.User.Name);
                var mate = trade.GetMate(context.User.Name);
                var myItems = trade.MyItems(me.Name);
                var mateItems = trade.MyItems(mate.Name);

                //ゴールド渡す
                uint gaveGold = trade.GetGiveGold(me.Name);
                me.Gold -= gaveGold;
                mate.Gold += gaveGold;

                //ゴールド受け取る
                uint receiveGold = trade.GetGiveGold(mate.Name);
                mate.Gold -= receiveGold;
                me.Gold += receiveGold;

                //アイテム渡す
                var gaveItems = MoveItem(me, mate, myItems);
                matePacket = new TradePacket(mateItems.Select(t => (t.inveSlot, t.count)).ToArray(), gaveItems, receiveGold, gaveGold);

                //アイテム受け取る
                var receiveItems = MoveItem(mate, me, mateItems);
                myPacket = new TradePacket(myItems.Select(t => (t.inveSlot, t.count)).ToArray(), receiveItems, gaveGold, receiveGold);
            }
            else
            {
                //初承認
                trade.Permit = true;
                myPacket = new PermitTradePacket(context.User.CharID);
                matePacket = new PermitTradePacket(context.User.CharID);
            }
            sendMe(myPacket);
            sendMate(matePacket);
        }

        /// <summary>
        /// 移動
        /// </summary>
        /// <param name="me"></param>
        /// <param name="mate"></param>
        /// <param name="items"></param>
        Item[] MoveItem(Player me, Player mate, List<(int,int,Item)> items)
        {
            List<Item> addedItems = new List<Item>();
            foreach ((var inveSlot, var count, var item) in items)
            {
                //追加されるアイテム
                Item toAddItem = (Item)item.Clone();
                toAddItem.Count = (byte)count;
                
                if (item.Count != count)
                {
                    //減少
                    if (inveSlot >= 100) me.EquipmentItems[inveSlot - 100].Count -= (byte)count;
                    else me.InventoryItems[inveSlot].Count -= (byte)count;
                }
                else
                {
                    //取り除く
                    if (inveSlot >= 100) me.EquipmentItems[inveSlot - 100] = new Item();
                    else me.InventoryItems[inveSlot] = new Item();
                }

                //追加
                mate.InventoryItems.InsertItem(toAddItem);
                addedItems.Add(toAddItem);
            }
            return addedItems.ToArray();
        }
    }
}
