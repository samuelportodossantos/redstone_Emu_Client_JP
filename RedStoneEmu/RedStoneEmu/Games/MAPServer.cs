#define PARALLEL
using RedStoneLib;
using RedStoneLib.Model;
using RedStoneLib.Model.Base;
using RedStoneLib.Packets;
using RedStoneLib.Packets.RSPacket.Action;
using RedStoneLib.Packets.RSPacket.ActorGenerator;
using RedStoneLib.Packets.RSPacket.ActorInfo;
using RedStoneLib.Packets.RSPacket.GameLogin;
using RedStoneLib.Packets.RSPacket.Items;
using RedStoneLib.Packets.RSPacket.Move;
using RedStoneLib.Packets.RSPacket.MoveField;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RedStoneEmu.Games
{
    /// <summary>
    /// マップ単位の鯖処理
    /// </summary>
    class MAPServer
    {
        /// <summary>
        /// 全MAPServerここに保存
        /// </summary>
        public static Dictionary<ushort, MAPServer> AllMapServers { get; private set; }
            = new Dictionary<ushort, MAPServer>();

        /// <summary>
        /// 実態
        /// </summary>
        public Map MAP { get; private set; }

        /// <summary>
        /// 移動用MAP
        /// </summary>
        public readonly bool[,] MovingMap;

        /// <summary>
        /// キャラ（key:charID）
        /// </summary>
        public Dictionary<ushort, Actor> Actors { get; private set; } 
            = new Dictionary<ushort, Actor>();

        /// <summary>
        /// 死んだモンスター
        /// charid:復活時刻
        /// </summary>
        private Dictionary<ushort, DateTime> DiedMonsters = new Dictionary<ushort, DateTime>();

        /// <summary>
        /// マップスレッドタイマー
        /// </summary>
        private System.Timers.Timer MapThread;

        /// <summary>
        /// ドロップアイテム
        /// </summary>
        private Dictionary<int, DropItem> DropItems = new Dictionary<int, DropItem>();

        /// <summary>
        /// 各ClientのSendPacket
        /// </summary>
        public Dictionary<ushort, SendPacketDelegate> SendPackets { get; set; }
            = new Dictionary<ushort, SendPacketDelegate>();

        /// <summary>
        /// 露店
        /// </summary>
        public List<Pitchman> PitchmanShops = new List<Pitchman>();

        /// <summary>
        /// 全マップサーバーの構築
        /// </summary>
        public static void AllMapServersBuilder()
        {
#if PARALLEL
            Parallel.ForEach(Map.AllMaps.Keys, mapID =>
#else
            foreach(var mapID in Map.AllMaps.Keys)
#endif
            {
                AllMapServers[mapID] = new MAPServer(Map.AllMaps[mapID]);
#if PARALLEL
            });
#else
            }
#endif
        }
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MAPServer(Map map)
        {
            //実態代入
            MAP = map;

            //タイマー初期化
            MapThread = new System.Timers.Timer(1000);
            MapThread.Elapsed += new System.Timers.ElapsedEventHandler(MapServerThread);

            //NPC生成
            ushort charid = 0;
            foreach (var single in MAP.NpcSingles.Where(t => t.CharType != Actor.MapActorSingle.CType.Monster))
            {
                Actors[charid] = new NPC(single, MAP.NpcGroups[single.InternalID], charid, MAP.SerialID);
                charid++;
            }

            //Monster生成
            foreach (var single in MAP.NpcSingles.Where(t => t.CharType == Actor.MapActorSingle.CType.Monster))
            {
                Monster monster = new Monster(single, MAP.NpcGroups[single.InternalID], charid, MAP.SerialID);
                monster.SetRandomPos(MAP);
                Actors[charid] = monster;
                charid++;
            }

            //MovingMap作成
            MovingMap = new bool[map.Size.Width, map.Size.Height];
            for (int x = 0; x < map.Size.Width; x++)
            {
                for (int y = 0; y < map.Size.Height; y++)
                {
                    MovingMap[x, y] = map.GetBlock(x, y) == 0;
                }
            }
            RedStoneLib.Algorithm.MovingMap.Transform(ref MovingMap);
        }

        /// <summary>
        /// マップ内にパケット送信
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="ignoreCharID">パケット送らない対象のcharID</param>
        public void SendPacketsToPlayersOnTheMap(Packet packet, ushort? ignoreCharID = null, bool isNear = false, bool flush = false)
        {
            foreach (ushort charID in Actors.Keys)
            {
                //プレイヤー＆charIDチェック
                if (Actors[charID] is Player && (!ignoreCharID.HasValue || ignoreCharID.HasValue && charID != ignoreCharID.Value))
                {
                    SendPackets[charID].Invoke(packet, flush);
                }
            }
        }

        /// <summary>
        /// 全マップ内でプレイヤー名を検索
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool IsPlayerExistInServerByName(string name, out ushort mapSerial)
        {
            foreach (var serial in AllMapServers.Keys)
            {
                if (AllMapServers[serial].Actors.Values.Any(t => t is Player && t.Name == name))
                {
                    mapSerial = serial;
                    return true;
                }
            }
            mapSerial = ushort.MaxValue;
            return false;
        }

        /// <summary>
        /// Actors取得
        /// </summary>
        /// <returns></returns>
        public List<Actor> HandleGetActors(ushort? IgnoreCharID = null) =>
            Actors.Where(t => !IgnoreCharID.HasValue || t.Key != IgnoreCharID.Value).Select(t => t.Value).ToList();

        /// <summary>
        /// 新しいCharIDのリスト
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ushort> NewCharIDs
            => Enumerable.Range(0, 0x7FF).Where(t => !Actors.Keys.Contains((ushort)t)).Select(t => (ushort)t);


        /// <summary>
        /// マップヘッダ取得
        /// </summary>
        /// <returns></returns>
        public Map.MapHeader HandleGetHeader() => MAP.Header;

        /// <summary>
        /// マップに参加
        /// </summary>
        /// <param name="player"></param>
        /// <param name="previousPlayer">参加済みの時記録されたユーザ</param>
        /// <returns>参加済みはfalse</returns>
        public bool Join(Client client, out Player previousPlayer, bool moveFieldFlag = false)
        {
            //sendpacket charid その他のセット
            void setUser(Player prevPlayer=null)
            {
                //匿名メソッド内で消えるので保存
                ushort charID = (prevPlayer ?? client.User).CharID;

                //SendPacket追加
                SendPackets[charID] = new SendPacketDelegate(client.SendPacket);

                //マップ離脱時の処理登録
                client.Socket.ConnectionLost += () =>
                {
                    //消去通知
                    SendPacketsToPlayersOnTheMap(new RemoveActorPacket(charID));

                    //消去
                    Actors.Remove(charID);
                    SendPackets.Remove(charID);
                };

                //GetActors
                client.GetMapActors += HandleGetActors;

                //参加成功
                client.SendPacket(new ResultJoinGamePacket((prevPlayer ?? client.User), HandleGetHeader()));
                

                //最初のプレイヤーだったらSpawnTimerスタート
                if (!MapThread.Enabled && Actors.Count(t => t.Value is Player) == 1)
                {
                    MapThread.Start();
                }

                //ドロップアイテム送信
                if (DropItems.Count > 0)
                {
                    SendPackets[charID].Invoke(new DroppingItems((ushort)DropItems.Count, DropItems.Values.SelectMany(t => t.ToBytes()).ToArray()));
                }

                //Actor出現通知
                SendPacketsToPlayersOnTheMap(new VerySimpleActorInfoListPacket(new List<Actor> { client.User }), ignoreCharID: charID);
            }

            if (Actors.Values.Any(t => (t as Player)?.Name == client.User.Name))
            {
                //登録済み
                previousPlayer = (Player)Actors.Values.Single(t => (t as Player)?.Name == client.User.Name);
                setUser(previousPlayer);
                return false;
            }

            lock (Actors)
            {
                //参加
                client.User.CharID = NewCharIDs.First();
                Actors[client.User.CharID] = client.User;
                //Handleセット
                client.User.MapHeader = MAP.Header;
            }

            //登録
            if (!moveFieldFlag)
            {
                setUser();
            }

            previousPlayer = null;
            return true;
        }

        /// <summary>
        /// マップ間の移動
        /// </summary>
        /// <param name="player"></param>
        public void MoveField(Player player, string moveToIPAddress)
        {
            var posX = player.PosX;
            var posY = player.PosY;
            var charID = player.CharID;

            //近いポータル
            var portal = MAP.AreaInfos.Where(t => t.ObjectInfo == Map.AreaInfo.ObjectType.WarpPortal)
                .Select(t => new { v = t, dist = Helper.Hypot(posY - (int)t.CenterPos.Y, (posX - (int)t.CenterPos.X) / 2.0) })
                .OrderBy(t => t.dist).First().v;

            //移動通知
            SendPackets[charID].Invoke(new MoveFieldResultPacket(portal.MovetoFileName, moveToIPAddress));
            //シリアル変更
            player.MapSerial = Map.AllMapInfos.First(t => t.Value.fileName == portal.MovetoFileName).Key;
            //位置変更
            var targetObject = Map.AllMaps[player.MapSerial].AreaInfos.First(t => t.MovetoFileName == MAP.FileName);
            player.PosX = (ushort)targetObject.CenterPos.X;
            player.PosY = (ushort)targetObject.CenterPos.Y;
        }

        /// <summary>
        /// マップ用スレッド
        /// </summary>
        private void MapServerThread(object sender, System.Timers.ElapsedEventArgs e)
        {
            //MOBリスポーンチェック
            List<ushort> removeList = new List<ushort>();
            foreach(var charID in DiedMonsters.Keys)
            {
                if(DiedMonsters[charID] < DateTime.Now)
                {
                    //復活
                    Monster monster = Actors[charID] as Monster;
                    monster.Spawned = true;

                    //レベルセット
                    var level = (ushort)Helper.StaticRandom.Next(monster.LevelScale.Min, monster.LevelScale.Max);
                    monster.SetStatusByLevel(level);

                    //位置セット
                    monster.SetRandomPos(MAP);

                    //respawn
                    SendPacketsToPlayersOnTheMap(new RegenMonsterPacket(ref monster));

                    //リストから除去
                    removeList.Add(charID);
                }
            }
            //一括除去
            foreach (var charID in removeList)
            {
                DiedMonsters.Remove(charID);
            }

            //MOB移動
            foreach(var monster in Actors.Values.Where(t=>t is Monster m && m.Spawned))
            {
                if(Helper.StaticRandom.NextDouble() < 0.01)
                {
                    ushort posX = monster.PosX, posY = monster.PosY;
                    ushort movetoX, movetoY;
                    do
                    {
                        movetoX = (ushort)Helper.StaticRandom.Next(posX - 64, posX + 64);
                        movetoY = (ushort)Helper.StaticRandom.Next(posY - 32, posY + 32);
                    } while (MAP.GetBlock(movetoX/64, movetoY/32) != 0);

                    SendPacketsToPlayersOnTheMap(new MovePacket(monster.CharID, posX, posY, movetoX, movetoY, monster.MoveSpeed));
                    //wait挟む
                    monster.PosX = movetoX;
                    monster.PosY = movetoY;
                }
            }
        }

        /// <summary>
        /// 死んだActorを登録
        /// </summary>
        /// <param name="charID"></param>
        public void ReserveDeadActor(ushort charID)
        {
            Actor target = Actors[charID];
            if(target is Monster monster)
            {
                //モンスター
                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(1000);
                    monster.Spawned = false;
                    //死んだモンスターリストに加える
                    DiedMonsters[charID] = DateTime.Now + new TimeSpan(0, 0, (int)monster.PopSpeed);
                });
            }
        }

        /// <summary>
        /// マップにドロップアイテム登録
        /// </summary>
        /// <param name="dropItems"></param>
        public void ReseiveDropItems(DropItem[] dropItems)
        {
            if (DropItems.Count == 0)
            {
                //DropItems空
                for (int i = 0; i < dropItems.Length; i++)
                {
                    dropItems[i].SetIndex(i);
                }
            }
            else
            {
                //最小IDから未使用を探す
                foreach (var index in Enumerable.Range(DropItems.Keys.Min(), int.MaxValue).Where(t => !DropItems.ContainsKey(t))
                    .Take(dropItems.Length).Select((v, i) => new { v, i }))
                {
                    dropItems[index.i].SetIndex(index.v);
                }
            }

            //登録
            foreach(DropItem dropItem in dropItems)
            {
                DropItems[dropItem.Index] = dropItem;
            }

            //パケット送信
            SendPacketsToPlayersOnTheMap(new DroppingItems((ushort)dropItems.Length, dropItems.SelectMany(t => t.ToBytes()).ToArray()));
        }

        /// <summary>
        /// ドロップアイテム取得試行
        /// </summary>
        /// <param name="player"></param>
        /// <param name="index">ドロップアイテムのインデックス</param>
        /// <param name="checkSum"></param>
        /// <param name="pickedItem"></param>
        /// <param name="resultPacket"></param>
        /// <returns></returns>
        public bool TryTakeDroppedItem(Player player, ushort index, ushort checkSum, out Item pickedItem, out PickItemResult resultPacket)
        {
            DropItem targetItem = null;
            pickedItem = null;
            
            if (!DropItems.TryGetValue(index, out targetItem))
            {
                //辞書内に存在しない
                resultPacket = new PickItemResult(ItemBase.ItemResult.Item_disappeared);
                return false;
            }

            /*if (checkSum != targetItem.DroppedItem.CheckSum)
            {
                //チェックサム不一致
                resultPacket = new PickItemResult(ItemBase.ItemResult.ItemInfoError);
                return false;
            }*/

            if (targetItem.CharID != 0xFFFF && targetItem.CharID != player.CharID)
            {
                //持ち主不一致
                resultPacket = new PickItemResult(ItemBase.ItemResult.NoOwner);
                return false;
            }

            if (player.InventoryItems.IsFull)
            {
                //インベの空きなし
                resultPacket = new PickItemResult(ItemBase.ItemResult.NoSpace);
                return false;
            }

            if (targetItem.DroppedItem.ItemIndex == 0 && 
                player.Gold + targetItem.DroppedItem.UniqueID > RedStoneApp.GameServer.Config.PossessionGoldLimit)
            {
                //ゴールドいっぱい
                resultPacket = new PickItemResult(ItemBase.ItemResult.Over2Billion);
                return false;
            }

            //成功
            resultPacket = new PickItemResult(targetItem.DroppedItem);
            pickedItem = targetItem.DroppedItem;

            //アイテム消す
            DropItems.Remove(index);
            SendPacketsToPlayersOnTheMap(new RemoveDropItem(index));

            return true;
        }
    }
}
