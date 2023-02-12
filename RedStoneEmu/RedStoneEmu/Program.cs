using System;
using Microsoft.EntityFrameworkCore;
using RedStoneEmu.Database.RedStoneEF;
using RedStoneLib.Karmas;
using RedStoneLib.Model;
using RedStoneLib.Model.Base;
using RedStoneEmu.Packets.Handlers;
using System.Text;
using RedStoneLib;
using System.Reflection;
using RedStoneEmu.Games;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using RedStoneEmu.Provider;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RedStoneEmu.Database.PhpbbEF;

namespace RedStoneEmu
{
    class RedStoneApp
    {
        /// <summary>
        /// サーバー
        /// </summary>
        public static Login LoginServer { get; private set; } = null;
        public static Game GameServer { get; private set; } = null;
        public static Community CommunityServer { get; private set; } = null;

        /// <summary>
        /// プログラム内に存在するサーバー
        /// </summary>
        public static ServerType includeServer { get; private set; } = ServerType.Login;

        /// <summary>
        /// デバッグフラグ
        /// </summary>
        public static bool isDebug { get; private set; } = false;

        private static async Task ConfigureServices()
        {
            using IHost host = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
                {
                    ServerConfig config = new ServerConfig();
                    var connectionString = string.Format("Host={0};User Id={1};Password={2};Database={3};Port={4}",
                        config.DatabaseHost,
                        config.DatabaseUsername,
                        config.DatabasePassword,
                        config.DatabaseName,
                        config.DatabasePort);

                    services.AddDbContext<loginContext>(builder =>
                    {
                        builder.UseNpgsql(connectionString);
                    });
                    services.AddDbContext<gameContext>(builder =>
                    {
                        builder.UseNpgsql(connectionString);
                    });

                    services.AddDbContext<PhpBBContext>(builder =>
                    {
                        var conn = string.Format("Host={0};User Id={1};Password={2};Database={3};Port={4}",
                        config.DatabaseHost,
                        config.DatabaseUsername,
                        config.DatabasePassword,
                        config.DatabaseName,
                        config.DatabasePort);
                        builder.UseNpgsql(connectionString);
                    });


                })
                .Build();

            await host.RunAsync();
        }

        static void Main(string[] args)
        {
            EnviromentController env = new EnviromentController();
            env.build();
            env.setupDatabases();

            includeServer = ServerType.Login | ServerType.Game | ServerType.Community;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            // Console.WriteLine(IPAddressProvider.GlobalIP);

            //Logger設定
            ConsoleSystem.IsStart = true;
            ConsoleSystem.Start();

            //Set Exit to a handler that ensures that it is executed before exiting
            Console.CancelKeyPress += Exit;

            // Load handler
            PacketHandlers.LoadPacketHandlers();

            //KarmaService load
            KarmaItemServices.Load(Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsClass));

            //Set log file name
            // Logger.SetLogFileName(includeServer);

            //Data read
            if (true)
            {
                Quest.Load();
                Skill.Load();
                ItemBase.Load();
                OPBase.Load();
                Breed.Load();
                Map.LoadAllMaps();
                MAPServer.AllMapServersBuilder();

                //delegate設定
                // PublicHelper.WriteInternal = Logger.WriteInternal;
                // PublicHelper.WriteWarning = Logger.WriteWarning;
            }
            Console.WriteLine("Finish");


            //インスタンス開始
            List<Task> tasks = new List<Task>();
            foreach (ServerType instanceType in Enum.GetValues(typeof(ServerType)))
            {
                //フラグある場合
                if (includeServer.HasFlag(instanceType))
                {
                    //サーバー実行
                    tasks.Add(Task.Factory
                        .StartNew(() => StartServer(instanceType))
                        .ContinueWith((t) => Logger.WriteException(string.Format("[!!{0} SERVER!!]", instanceType), t.Exception.InnerException), TaskContinuationOptions.OnlyOnFaulted));
                }
            }
        }

        static void MainOLD(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Console.WriteLine(IPAddressProvider.GlobalIP);

            //Logger設定
            ConsoleSystem.IsStart = true;
            ConsoleSystem.Start();

            //引数の処理
            try
            {
                for (var i = 0; i < args.Length; i++)
                {
                    switch (args[i].ToLower())
                    {
                        case "-g":
                        case "--game":
                            includeServer = ServerType.Game;
                            break;
                        case "-l":
                        case "--login":
                            includeServer = ServerType.Login;
                            break;
                        case "-c":
                        case "--community":
                            includeServer = ServerType.Community;
                            break;
                        case "-d":
                        case "--debug":
                            includeServer = ServerType.Login | ServerType.Game | ServerType.Community;
                            isDebug = true;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteException("コマンドラインパラメータの解析中にエラーが発生しました", ex);
            }

            //終了する前に実行されることを保証するハンドラにExitをセット
            Console.CancelKeyPress += Exit;

            //ハンドラーのロード
            PacketHandlers.LoadPacketHandlers();

            //KarmaServiceロード
            KarmaItemServices.Load(Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsClass));

            //ログファイルの名前を設定
            Logger.SetLogFileName(includeServer);

            //データ読み込み
            if (includeServer.HasFlag(ServerType.Game))
            {
                //クエスト読み込み
                Quest.Load();

                //スキル読み込み
                Skill.Load();

                //アイテム読み込み
                ItemBase.Load();

                //OP読み込み
                OPBase.Load();

                //モンスター読み込み
                Breed.Load();

                //マップ読み込み
                Map.LoadAllMaps();
                MAPServer.AllMapServersBuilder();

                //delegate設定
                PublicHelper.WriteInternal = Logger.WriteInternal;
                PublicHelper.WriteWarning = Logger.WriteWarning;
            }

            //インスタンス開始
            List<Task> tasks = new List<Task>();
            foreach (ServerType instanceType in Enum.GetValues(typeof(ServerType)))
            {
                //フラグある場合
                if (includeServer.HasFlag(instanceType))
                {
                    //サーバー実行
                    tasks.Add(Task.Factory
                        .StartNew(() => StartServer(instanceType))
                        .ContinueWith((t) => Logger.WriteException(string.Format("[!!{0} SERVER!!]", instanceType), t.Exception.InnerException), TaskContinuationOptions.OnlyOnFaulted));
                }
            }
        }

        /// <summary>
        /// サーバーの開始
        /// </summary>
        /// <param name="serverType"></param>
        private static void StartServer(ServerType serverType)
        {
            switch (serverType)
            {
                case ServerType.Login:
                    LoginServer = new Login();
                    LoginServer.Run();
                    break;
                case ServerType.Game:
                    GameServer = new Game();
                    GameServer.Run();
                    break;
                case ServerType.Community:
                    CommunityServer = new Community();
                    CommunityServer.Run();
                    break;
                default:
                    throw new ArgumentException("undefined servertype");
            }
        }

        /// <summary>
        /// 終了イベント
        /// </summary>
        /// <param name="ctrlType"></param>
        private static void Exit(object sender, ConsoleCancelEventArgs e)
        {
            if (includeServer.HasFlag(ServerType.Login))
            {
                LoginServer.HandleClosing();
            }
            if (includeServer.HasFlag(ServerType.Game))
            {
                GameServer.HandleClosing();
            }
            if (includeServer.HasFlag(ServerType.Community))
            {
                CommunityServer.HandleClosing();
            }
        }
    }
}
