using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedStoneEmu.Database.RedStoneEF;
using RedStoneEmu.Games;
using RedStoneLib.Karmas;
using RedStoneLib.Model;
using RedStoneLib.Packets.RSPacket.Bank;
using RedStoneLib.Packets.RSPacket.Items;
using RedStoneLib.Packets.RSPacket.KarmaPacket;
using RedStoneLib.Packets.RSPacket.Quests;

namespace RedStoneEmu.Karmas.Commands
{
    /// <summary>
    /// 文章のジャンプ
    /// </summary>
    [KarmaItemAttr(0x000, 1)]
    class SkipSpeech : KarmaItemExecuteService
    {
        public override void HandleKarmaItem(Player player, uint[] value, ushort npcCharID, SendPacketDelegate sendPacket)
        {
            int progress = (int)value[0];
            player.PlayerEvent.Progress = progress;
            if (player.PlayerEvent.Events[progress].AutoStart)
            {
                //自動実行
                player.PlayerEvent.Events[progress].Execute(player, sendPacket, npcCharID);
            }
            else
            {
                //表示
                sendPacket(new ComplexSpeechPacket(npcCharID, player.PlayerEvent.Events[progress]));
            }
        }
    }

    /// <summary>
    /// 閉じる
    /// </summary>
    [KarmaItemAttr(0x001)]
    class CloseDialog : KarmaItemExecuteService
    {
        public override void HandleKarmaItem(Player player, uint[] value, ushort npcCharID, SendPacketDelegate sendPacket)
        {
            sendPacket(new EndDialogPacket());
        }
    }

    /// <summary>
    /// 店を開く
    /// </summary>
    [KarmaItemAttr(0x002)]
    class OpenShop : KarmaItemExecuteService
    {
        public override void HandleKarmaItem(Player player, uint[] value, ushort npcCharID, SendPacketDelegate sendPacket)
        {
            Map targetmap = Map.AllMaps[player.MapSerial];
            sendPacket(new OpenShopPacket(targetmap.Shops[value[0]]));
        }
    }

    /// <summary>
    /// 銀行開く
    /// </summary>
    [KarmaItemAttr(0x003)]
    class OpenBank : KarmaItemExecuteService
    {
        public override void HandleKarmaItem(Player player, uint[] value, ushort npcCharID, SendPacketDelegate sendPacket)
        {
            //称号持っていない
            if (!player.Titles.TryGetValue(1, out byte bankTitleLevel)) return;

            //DB取得
            using (var gameDB = new gameContext())
            {
                var bank = gameDB.Banks.SingleOrDefault(t => t.UserID == player.UserID);
                if (bank == null)
                {
                    //銀行ない場合は新規作成
                    bank = new Bank(player.UserID);
                    gameDB.Banks.Add(bank);
                    gameDB.SaveChanges();
                }

                //アイテムロード
                foreach ((int uniqueID, int i) in bank.UniqueItemString.Split(',').Select((t, i) => (Convert.ToInt32(t), i)))
                {
                    if (uniqueID == 0)
                    {
                        bank.Items[i] = new Item();
                        continue;
                    }
                    bank.Items[i] = gameDB.Items.Single(t => t.UniqueID == uniqueID);
                }

                //インベ・ベルトの複製
                bank.TemporaryInventory = (Player.ItemCollection)player.InventoryItems.Clone();
                bank.TemporaryEquipment = (Player.ItemCollection)player.EquipmentItems.Clone();

                //Bank登録
                Bank.AllBanks[bank.BankSession] = bank;
                player.BankSessionID = bank.BankSession;

                //パケット
                sendPacket(new OpenBankPacket(bank, bankTitleLevel));
            }
        }
    }

    /// <summary>
    /// クエスト開始
    /// </summary>
    [KarmaItemAttr(0x06F)]
    class StartQuest : KarmaItemExecuteService
    {
        public override void HandleKarmaItem(Player player, uint[] value, ushort npcCharID, SendPacketDelegate sendPacket)
        {
            if (player.ProgressQuests.Keys.Count >= 6)
            {
                //受注不可
                return;
            }

            //受注してないクエスト最小
            int qPos = Enumerable.Range(0, 6).Where(t => !player.ProgressQuests.ContainsKey(t)).Min();

            //追加
            var quest = new ProgressQuest((int)value[0]);
            player.ProgressQuests[qPos] = quest;

            sendPacket(new ChangeQuestInfoPacket((byte)qPos, ChangeQuestInfoPacket.QuestStatus.Start, quest.QuestInfo));
        }
    }

    /// <summary>
    /// クエストアップデート
    /// </summary>
    [KarmaItemAttr(0x070)]
    class UpdateQuest : KarmaItemExecuteService
    {
        public override void HandleKarmaItem(Player player, uint[] value, ushort npcCharID, SendPacketDelegate sendPacket)
        {
            ushort questID = (ushort)value[0];
            ushort progress = (ushort)value[1];
            if (!player.ProgressQuests.Values.Select(t=>t.Index).Contains(questID))
            {
                //受注してない
                return;
            }

            //受注したクエストインデックス
            int qPos = Enumerable.Range(0, 6).First(t => player.ProgressQuests.Values.Select(u => u.Index).Contains(questID));

            //アップデート
            player.ProgressQuests[qPos].Progress = progress;
            sendPacket(new ChangeQuestInfoPacket((byte)qPos, ChangeQuestInfoPacket.QuestStatus.Update, player.ProgressQuests[qPos].QuestInfo));
        }
    }

    /// <summary>
    /// EXP取得
    /// </summary>
    [KarmaItemAttr(0x075)]
    class GainEXP : KarmaItemExecuteService
    {
        public override void HandleKarmaItem(Player player, uint[] value, ushort npcCharID, SendPacketDelegate sendPacket)
        {
            MAPServer map = MAPServer.AllMapServers[player.MapSerial];
            StatusController.GaveEXP(value[2], ref player, sendPacket, pkt => map.SendPacketsToPlayersOnTheMap(pkt, isNear: true));
        }
    }

    /// <summary>
    /// クエスト完了
    /// </summary>
    [KarmaItemAttr(0x077)]
    class CompleteQuest : KarmaItemExecuteService
    {
        public override void HandleKarmaItem(Player player, uint[] value, ushort npcCharID, SendPacketDelegate sendPacket)
        {
            ushort questID = (ushort)value[0];
            //受注したクエストインデックス
            int qPos = Enumerable.Range(0, 6).First(t => player.ProgressQuests.Values.Select(u => u.Index).Contains(questID));

            //クエスト完了
            sendPacket(new ChangeQuestInfoPacket((byte)qPos));
            player.ProgressQuests.Remove(qPos);
            player.ClosedQuests.Add(questID);

            //gameClient
            GameClient gameClient = RedStoneApp.GameServer.Clients.Single(t => t.User.Name == player.Name) as GameClient;

            //クエ品消す
            foreach (var item in player.InventoryItems.Select((v, i) => new { v, i }).ToArray())
            {
                if (item.v.Base.QuestID == questID)
                {
                    sendPacket(new RemoveItem((ushort)item.v.ItemIndex, item.v.Count, RemoveItem.ItemRemoveType.RemoveItem));
                    player.InventoryItems[item.i] = new Item();
                    gameClient.RemoveDBItems.Add(item.v);
                }
            }
        }
    }

    /// <summary>
    /// クエストアイテム渡す・消す
    /// </summary>
    [KarmaItemAttr(0x0D1)]
    class QuestItemOperation : KarmaItemExecuteService
    {
        public override void HandleKarmaItem(Player player, uint[] value, ushort npcCharID, SendPacketDelegate sendPacket)
        {
            //クエ品
            Item qItem = new Item((ushort)value[0], (byte)value[1]);
            //gameClient
            GameClient gameClient = RedStoneApp.GameServer.Clients.Single(t => t.User.Name == player.Name) as GameClient;

            switch (value[2])
            {
                case 0:
                    //クエ品与える
                    if (player.InventoryItems.InsertItem(qItem))
                    {
                        gameClient.AddDBItems.Add(qItem);
                        sendPacket(new AddItemPacket(qItem, AddItemPacket.ItemGetType.AddItem));
                    }
                    break;
                case 1:
                    //クエ品消す
                    foreach (var item in player.InventoryItems.Select((v, i) => new { v, i }).ToArray())
                    {
                        if (item.v.ItemIndex == qItem.ItemIndex)
                        {
                            sendPacket(new RemoveItem((ushort)item.v.ItemIndex, item.v.Count, RemoveItem.ItemRemoveType.RemoveItem));
                            player.InventoryItems[item.i] = new Item();
                            gameClient.RemoveDBItems.Add(item.v);
                        }
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// 称号取得
    /// </summary>
    [KarmaItemAttr(0x132)]
    class AddTitle : KarmaItemExecuteService
    {
        public override void HandleKarmaItem(Player player, uint[] value, ushort npcCharID, SendPacketDelegate sendPacket)
        {
            byte titleIndex = (byte)value[2];
            byte titleLevel = (byte)value[1];

            //称号の場所
            var titlesArray = player.Titles.Keys.ToArray();
            ushort index = player.Titles.ContainsKey(titleIndex) ? (ushort)Enumerable.Range(0, player.Titles.Count).First(t => titlesArray[t] == titleIndex) : (ushort)titlesArray.Length;

            //称号取得
            player.Titles[titleIndex] = titleLevel;
            sendPacket(new GetTitlePackt(index, titleIndex, titleLevel));
        }
    }
}
