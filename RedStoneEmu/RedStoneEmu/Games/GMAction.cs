using RedStoneEmu.Packets.Handlers.MoveFieldSystem;
using RedStoneLib.Model;
using RedStoneLib.Model.Base;
using RedStoneLib.Packets.RSPacket.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RedStoneEmu.Games
{
    /// <summary>
    /// GMコマンド
    /// </summary>
    static class GMAction
    {
        /// <summary>
        /// GMコマンド受け付け
        /// </summary>
        /// <param name="client"></param>
        /// <param name="speech"></param>
        /// <returns>コマンド抜いた文字列</returns>
        public static string ReceiveGMCommand(GameClient client, string speech)
        {
            //コマンド判定
            var matches = Regex.Matches(speech, @"@\w+(\s\w+)*");
            foreach (Match m in matches)
            {
                //パース
                string[] statements = m.Value.ToLower().Split(' ', '　');

                //実行
                ExecuteGMCommand(client, string.Concat(statements[0].Skip(1)), statements.Skip(1).ToArray());

                //除去
                speech = speech.Replace(m.Value, "");
            }
            return speech;
        }

        /// <summary>
        /// GMコマンド実行
        /// </summary>
        /// <param name="client"></param>
        /// <param name="command">コマンド</param>
        /// <param name="args">引数</param>
        static void ExecuteGMCommand(GameClient client, string command, params string[] args)
        {
            switch (command)
            {
                case "goto"://移動

                    //候補検索
                    IEnumerable<MAPServer> maps = MAPServer.AllMapServers.Values;
                    foreach(string subname in args)
                    {
                        maps = maps.Where(t => t.MAP.Name.ToLower().Contains(subname.ToLower()));
                    }
                    if (maps.Count() > 0)
                    {
                        MoveField.Execute(client, maps.First().MAP.FileName);
                    }
                    break;
                case "give"://アイテム取得

                    //sendPacket検索
                    var map = MAPServer.AllMapServers[client.User.MapSerial];
                    var target = map.Actors.Values.FirstOrDefault(t => t.Name == args[0]) as Player;
                    if (target!=null)
                    {
                        //arg[0]がPlayerの名前
                        args = args.Skip(1).ToArray();
                    }
                    else
                    {
                        target = client.User;
                    }
                    var sendPacket = map.SendPackets[client.User.CharID];

                    //個数検索
                    int count = 1;
                    if(Regex.IsMatch(args[0], @"^\d+$"))//全て数字
                    {
                        count = Convert.ToInt32(args[0]);
                        args = args.Skip(1).ToArray();
                    }

                    //アイテム検索
                    IEnumerable<ItemBase> targetItemBases = ItemBase.AllItemBases;
                    foreach(var itemName in args)
                    {
                        targetItemBases = targetItemBases.Where(t => t.Name.ToLower().Contains(itemName.ToLower()));
                    }
                    if (targetItemBases.Count() > 0)
                    {
                        var targetItemBase = targetItemBases.First();
                        Item item = new Item((ushort)targetItemBase.Index, (byte)Math.Min(count, targetItemBase.StackableNum));
                        sendPacket(new AddItemPacket(item, AddItemPacket.ItemGetType.AddItem));
                        target.InventoryItems.InsertItem(item);
                        client.AddDBItems.Add(item);
                    }
                    break;
                case "money"://お金取得
                    uint money = Convert.ToUInt32(args[0]);
                    client.User.Gold += money;
                    client.SendPacket(new AddMoneyPacket(money, AddMoneyPacket.MoneyGetType.getMoney));
                    break;
            }
        }
    }
}
