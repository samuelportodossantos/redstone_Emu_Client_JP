using Microsoft.EntityFrameworkCore;
using RedStoneEmu.Database.RedStoneEF;
using RedStoneEmu.Games;
using RedStoneEmu.NetWork;
using RedStoneLib;
using RedStoneLib.Packets.RSPacket.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace RedStoneEmu
{
    /// <summary>
    /// サーバーのタイプ
    /// </summary>
    [Flags]
    public enum ServerType : byte
    {
        Login = 1,
        Game = 2,
        Community = 4
    }

    /// <summary>
    /// ログイン鯖
    /// </summary>
    class Login : Server
    {
        protected override int port { get; } = 55661;

        protected override ServerType MyServerType
            => ServerType.Login;

        private Timer GameServerObserverTimer;

        public Login() : base()
        {
            //ゲームサーバー監視
            GameServerObserverTimer = new Timer(new TimerCallback(GameServerObserver));
            GameServerObserverTimer.Change(0, 10000);
        }

        /// <summary>
        /// ゲームサーバー監視
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameServerObserver(object args)
        {
            try
            {
                using (var db = new loginContext())
                {
                    var task = db.GameServerInfos.ToListAsync();
                    task.Wait();
                    Packets.Handlers.LoginSystem.ServerList.GameServerInfos = task.Result;
                }
            }
            catch (Exception ex) when (ex is System.Net.Sockets.SocketException || ex is Npgsql.NpgsqlException)
            {
                Logger.WriteWarning("[Game Server Observer] {0}", ex.Message);
            }
            catch (Exception ex)
            {
                Logger.WriteException("[Game Server Observer]", ex);
            }
        }

        protected override void HandleNewClient(SocketClient client)
        {
            var newClient = new LoginClient(client);
            Clients.Add(newClient);
        }
    }

    /// <summary>
    /// ゲーム鯖
    /// </summary>
    class Game : Server
    {
        protected override int port { get; } = 54631;

        protected override ServerType MyServerType
            => ServerType.Game;

        public Game() : base()
        {
            //ライブラリの設定
            PublicHelper.Country = (uint)Config.Country;
            PublicHelper.SeasonVariable = (uint)Config.SeasonVariable;

            //EXPテーブル計算
            StatusController.CalcEXPTables(Config.LevelCap);

            //ゲームサーバー情報をDBに追加
            loginContext db = new loginContext();
            while (!NotifyEnableSaveGameServer(db))
            {
                db = new loginContext();
            }
            db.Dispose();
        }
        
        /// <summary>
        /// ゲームサーバー有効化を通知
        /// </summary>
        private bool NotifyEnableSaveGameServer(loginContext db)
        {
#if DEBUG
            string ip = IPAddressProvider.LocalIP;
#else
            string ip = IPAddressProvider.GlobalIP;
#endif
            //新規サーバー情報
            var newServer = new GameServerInfo()
            {
                Enable = true,
                ServerId = Config.ServerID,
                ServerName = Config.ServerName,
                Host = ip,
                ServerType = (GameServerType)Config.serverType
            };

            //検索
            var task = db.GameServerInfos.SingleOrDefaultAsync(t => t.ServerId == newServer.ServerId);
            task.Wait();
            var oldServer = task.Result;

            if (oldServer != null)
            {
                //すでにサーバーがある場合は削除
                db.GameServerInfos.Remove(oldServer);
            }
            //追加
            db.GameServerInfos.AddAsync(newServer).Wait();

            Logger.WriteDB("ゲームサーバーをセーブ中...");
            try
            {
                db.SaveChangesAsync().Wait();
                Logger.WriteDB("完了");
                return true;
            }
            catch
            {
                Logger.WriteWarning("失敗");
                return false;
            }
        }

        /// <summary>
        /// ゲームサーバー無効化を通知
        /// </summary>
        private void NotifyDisableGameServer(loginContext db)
        {
            //自分のServerIDを持つものを削除
            db.GameServerInfos.Single(t => t.ServerId == Config.ServerID).Enable = false;

            Logger.WriteDB("ゲームサーバー停止中...");
            db.SaveChangesAsync().Wait();
            Logger.WriteDB("完了");
        }

        public override void HandleClosing()
        {
            base.HandleClosing();
            using (var db = new loginContext())
            {
                NotifyDisableGameServer(db);
            }
        }

        protected override void HandleNewClient(SocketClient client)
        {
            var newClient = new GameClient(client);
            Clients.Add(newClient);
        }
    }

    /// <summary>
    /// コミュニティ鯖
    /// </summary>
    class Community : Server
    {
        protected override int port { get; } = 56621;

        protected override ServerType MyServerType
            => ServerType.Community;

        public Community() : base()
        { }

        protected override void HandleNewClient(SocketClient client)
        {
            var newClient = new CommunityClient(client);
            Clients.Add(newClient);
        }
    }

    /// <summary>
    /// ソケット受け入れサーバーの管理やPing管理
    /// </summary>
    abstract class Server
    {
        /// <summary>
        /// ポート
        /// </summary>
        protected abstract int port { get; }

        /// <summary>
        /// インスタンス
        /// </summary>
        public RedStoneApp Instance { get; set; }

        /// <summary>
        /// 設定ファイル
        /// </summary>
        public ServerConfig Config { get; set; }

        /// <summary>
        /// クライアント全員
        /// </summary>
        public List<Client> Clients { get; private set; } = new List<Client>();

        /// <summary>
        /// サーバー開始時間
        /// </summary>
        public DateTime StartTime { get; private set; } = DateTime.Now;

        /// <summary>
        /// 自分のタイプ
        /// </summary>
        protected abstract ServerType MyServerType { get; }

        /// <summary>
        /// ソケットサーバー
        /// </summary>
        private readonly SocketServer SocketServer = null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Server()
        {
            //設定ファイルのインスタンスを作成
            Config = new ServerConfig(MyServerType);

            //設定ファイルを読み込む
            if (!Config.Load())
            {
                Logger.WriteWarning("設定ファイルを作成しました。\n必要な項目を記入して再度実行してください。");
                return;
            }

            //スタート日時を記録
            Logger.WriteInternal("[{0}]Server starting at {1}", GetType(), DateTime.Now);

            //ソケットサーバーの開始
            SocketServer = new SocketServer(port);
            SocketServer.NewClient += HandleNewClient;
        }

        /// <summary>
        /// サーバーを実行
        /// </summary>
        public void Run()
        {
            while (true)
            {
                //基になるSocketServerを実行
                SocketServer.Run();

                //2分前
                var before2m = DateTime.Now - new TimeSpan(0, 2, 0);

                foreach (var client in Clients)
                {
                    if (client.IsClosed)
                    {
                        //クライアントの存在を確認
                        Clients.Remove(client);
                        break;
                    }

#if DEBUG == false
                    if (MyServerType == ServerType.Game && client.BeatTime < before2m)
                    {
                        //ビートタイムが2分前以前はタイムアウト
                        client.Socket.Disconnect();
                    }
#endif
                }

            }
        }

        /// <summary>
        /// 新規接続受け入れハンドラ
        /// </summary>
        /// <param name="client"></param>
        protected abstract void HandleNewClient(SocketClient client);

        /// <summary>
        /// サーバー終了ハンドラ
        /// </summary>
        public virtual void HandleClosing()
        {
            //設定を保存
            //Instances[key].Config?.Save();
        }
    }
}
