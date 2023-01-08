using Microsoft.EntityFrameworkCore;
using RedStoneEmu.Database.RedStoneEF;
using RedStoneLib.Karmas;
using RedStoneLib.Model;
using RedStoneLib.Model.Base;
using RedStoneEmu.Packets.Handlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using RedStoneLib;
using System.Reflection;

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
        
        /// <summary>
        /// Win32 API版強制終了ハンドラ
        /// </summary>
        /// <param name="Handler"></param>
        /// <param name="Add"></param>
        /// <returns></returns>
        [DllImport("Kernel32")]
        static extern bool
        SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

        /// <summary>
        /// SetConsoleCtrlHandler関数にメソッドを渡すためのデリゲート
        /// </summary>
        /// <param name="CtrlType"></param>
        /// <returns></returns>
        delegate bool HandlerRoutine(CtrlTypes CtrlType);
        static HandlerRoutine myHandlerDele;


        static void Main(string[] args)
        {
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
            myHandlerDele = new HandlerRoutine(Exit);
            SetConsoleCtrlHandler(myHandlerDele, true);
            
            //ハンドラーのロード
            PacketHandlers.LoadPacketHandlers();

            //KarmaServiceロード
            KarmaItemServices.Load();

            //ログファイルの名前を設定
            Logger.SetLogFileName(includeServer);

            //データ読み込み
            if (includeServer.HasFlag(ServerType.Game))
            {
                //スキル読み込み
                Skill.Load(StreamFromAssembly("skill2.dat"));

                //アイテム読み込み
                ItemBase.Load(StreamFromAssembly("Scenario.Red_Stone.item.dat"));

                //OP読み込み
                OPBase.Load(StreamFromAssembly("Scenario.Red_Stone.item.dat"));

                //モンスター読み込み
                Breed.Load(StreamFromAssembly("Scenario.Red_Stone.job2.dat"));

                //マップ読み込み
                Map.LoadMapList(StreamFromAssembly("mapList.dat"));
                var nameAndStreamList = Map.AllMapInfos.Select(t => (t.Key, t.Value.fileName, StreamFromAssembly($"Scenario.Red_Stone.Map.{t.Value.fileName}"))).ToList();
                Map.LoadMaps(nameAndStreamList);

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
                        .ContinueWith((t)=>Logger.WriteException(string.Format("[!!{0} SERVER!!]", instanceType),t.Exception.InnerException), TaskContinuationOptions.OnlyOnFaulted));
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
        private static bool Exit(CtrlTypes ctrlType)
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

            return false;
        }


        /// <summary>
        /// pathから埋め込みデータのストリーム取得（RedStoneEmu.Data.～）
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static Stream StreamFromAssembly(string path)
            => Assembly.GetExecutingAssembly()
                .GetManifestResourceStream($"RedStoneEmu.Data.{path}");
    }

    /// <summary>
    /// ハンドラ・ルーチンに渡される定数の定義
    /// </summary>
    public enum CtrlTypes
    {
        CTRL_C_EVENT = 0,
        CTRL_BREAK_EVENT = 1,
        CTRL_CLOSE_EVENT = 2,
        CTRL_LOGOFF_EVENT = 5,
        CTRL_SHUTDOWN_EVENT = 6
    }
}
