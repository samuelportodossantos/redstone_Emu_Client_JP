using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace RedStoneEmu
{
    /// <summary>
    /// コンソールコマンド用デリゲート
    /// </summary>
    /// <param name="args"></param>
    /// <param name="length"></param>
    /// <param name="full"></param>
    /// <param name="client"></param>
    public delegate void ConsoleCommandDelegate(string[] args, int length, string full, Client client);

    /// <summary>
    /// コンソールに関するクラス
    /// </summary>
    public static class ConsoleSystem
    {
        /// <summary>
        /// コンソールに同時書き込みを防ぐためのロッカー
        /// </summary>
        private static object consoleLock = new object();

        /// <summary>
        /// コンソールコマンド
        /// </summary>
        public static List<ConsoleCommand> Commands = new List<ConsoleCommand>();

        /// <summary>
        /// タイトルバーにアセンブリ情報のせる用のタイマ
        /// </summary>
        private static Timer timer;

        /// <summary>
        /// インプットに関するスレッド
        /// </summary>
        public static Thread Thread;

        /// <summary>
        /// 現在の行
        /// </summary>
        private static int CommandRowInConsole;

        /// <summary>
        /// 最後のコマンドラインのサイズ
        /// </summary>
        private static int LastDrawnCommandLineSize;

        /// <summary>
        /// プロンプト
        /// </summary>
        private const string Prompt = "RedStone> ";

        /// <summary>
        /// コマンドインデックス
        /// </summary>
        private static int CommandIndex;
        
        /// <summary>
        /// コマンドライン上のコマンド
        /// </summary>
        private static string CommandLine = string.Empty;
        private static int MaxCommandLineSize = -1;

        private static readonly List<string> History = new List<string>();
        private static int HistoryIndex;
        private static readonly List<ConsoleKey> ControlKeys = new List<ConsoleKey>
        {
            ConsoleKey.Backspace,
            ConsoleKey.Enter,
            ConsoleKey.UpArrow,
            ConsoleKey.DownArrow,
            ConsoleKey.LeftArrow,
            ConsoleKey.RightArrow,
            ConsoleKey.Home,
            ConsoleKey.End,
            ConsoleKey.Delete
        };
        private static ConsoleKeyInfo Key;

        /// <summary>
        /// 画面横幅
        /// </summary>
        public static int Width { get; private set; }

        /// <summary>
        /// 画面縦幅
        /// </summary>
        public static int Height { get; private set; }

        /// <summary>
        /// スタート状況
        /// </summary>
        public static bool IsStart = false;

        /// <summary>
        /// コンソール開始
        /// </summary>
        public static void Start()
        {
            Console.Title = "RedStoneEmu";
            Console.CursorVisible = true;
            Console.Clear();
            SetSize();

            timer = new Timer(1000);
            timer.Elapsed += TimerRefresh;
            timer.Start();
            
            CreateCommands();
            Thread = new Thread(StartThread);
            Thread.Start();
        }
        
        /// <summary>
        /// コンソール終了
        /// </summary>
        /// <param name="args"></param>
        /// <param name="length"></param>
        /// <param name="full"></param>
        /// <param name="client"></param>
        public static void Stop(string[] args, int length, string full, Client client)
        {
            /*foreach (Client player in Program.Instance.Server.Clients.ToList())
            {
                player.SendClose();
                player.HandleConnectionLost();
            }
            if (Program.isGameServer)
            {
                Program.serverlist_db.delete_server(Program.gameConfig.ServerName);
            }*/
            Console.Clear();
            Console.WriteLine("bye");
            Environment.Exit(0);
        }

        /// <summary>
        /// コンソールコマンド用
        /// </summary>
        public static void CreateCommands()
        {
            var exit_E = new ConsoleCommand(Stop, "stop", "exit", "close", "bye");
            Commands.Add(exit_E);
        }

        /// <summary>
        /// タイマー（アセンブリ情報のせる）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void TimerRefresh(object sender, ElapsedEventArgs e)
        {
            lock (consoleLock)
            {
                Console.Title = $"RedStone {RedStoneApp.includeServer} server - {AssembleInfoBar()}";
            }
        }

        /// <summary>
        /// アセンブリインフォ
        /// </summary>
        /// <returns></returns>
        private static string AssembleInfoBar()
        {
            IEnumerable<string> clients()
            {
                if (RedStoneApp.LoginServer != null)
                {
                    yield return $"[{nameof(Login)}] {RedStoneApp.LoginServer.Clients.Count}";
                }
                if (RedStoneApp.GameServer != null)
                {
                    yield return $"[{nameof(Game)}] {RedStoneApp.GameServer.Clients.Count}";
                }
                if (RedStoneApp.CommunityServer != null)
                {
                    yield return $"[{nameof(Community)}] {RedStoneApp.CommunityServer.Clients.Count}";
                }
            }

            float usage = Process.GetCurrentProcess().PrivateMemorySize64 / 1024 / 1024;

            var time = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString();

            return $"Clients: {string.Join(",", clients())} | Memory: {usage} MB | {time}";
        }

        /// <summary>
        /// 画面とコマンドラインのサイズをセット
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        private static void SetSize()
        {
            int width = Console.WindowWidth;
            int height = Console.WindowHeight;
            Width = width;
            Height = height;

            MaxCommandLineSize = width - Prompt.Length;
        }

        /// <summary>
        /// コンソールに書き込み
        /// </summary>
        /// <param name="color"></param>
        /// <param name="text"></param>
        public static void AddLine(ConsoleColor color, string text)
        {
            /*lock (consoleLock)
            {
                BlankDrawnCommandLine();
                
                var saveColor = Console.ForegroundColor;

                //カーソルポジションのセット
                Console.SetCursorPosition(0, CommandRowInConsole);

                //色を変えて書き込んで色戻す
                Console.ForegroundColor = color;
                Console.WriteLine(text);
                Console.ForegroundColor = saveColor;
                
                Console.WriteLine();
                CommandRowInConsole = Console.CursorTop - 1;

                RefreshCommandLine();
                FixCursorPosition();
            }*/
            var saveColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = saveColor;
        }

        /// <summary>
        /// コマンドラインに空白を挿入
        /// </summary>
        private static void BlankDrawnCommandLine()
        {
            if (LastDrawnCommandLineSize > 0)
            {
                Console.SetCursorPosition(0, CommandRowInConsole);

                for (int i = 0; i < LastDrawnCommandLineSize; i++)
                    Console.Write(' ');

                LastDrawnCommandLineSize = 0;
            }
        }

        /// <summary>
        /// コマンドラインのリフレッシュ
        /// </summary>
        private static void RefreshCommandLine()
        {
            BlankDrawnCommandLine();

            Console.SetCursorPosition(0, CommandRowInConsole);
            Console.Write(Prompt);
            Console.Write(CommandLine);

            LastDrawnCommandLineSize = Prompt.Length + CommandLine.Length;
        }

        /// <summary>
        /// カーソルポジションの修正
        /// </summary>
        private static void FixCursorPosition()
        {
            Console.SetCursorPosition(Prompt.Length + CommandIndex, CommandRowInConsole);
        }

        /// <summary>
        /// CheckInput用スレッド
        /// </summary>
        public static void StartThread()
        {
            while (true)
            {
#if !DEBUG
                    Thread.Yield();
                    continue;
#endif
                try
                {
                    CheckInput();
                }
                catch (ThreadInterruptedException ex)
                {
                    Logger.WriteException("Thread inturrupted", ex);
                }
                catch (ThreadStateException ex)
                {
                    Logger.WriteException("Thread state error", ex);
                }
                catch (InvalidOperationException)
                {
                    Thread.Yield();
                    continue;
                }
                catch (Exception ex)
                {
                    Logger.WriteException(string.Empty, ex);
                }
            }
        }

        /// <summary>
        /// コンソール中のインプットのチェック
        /// </summary>
        private static void CheckInput()
        {
            // Read key
            Key = Console.ReadKey(true);

            // Check to make sure this is a valid key to append to the command line
            var validKey = ControlKeys.All(controlKey => Key.Key != controlKey);

            // Append key to the command line
            if (validKey && (CommandLine.Length + 1) < MaxCommandLineSize)
            {
                CommandLine = CommandLine.Insert(CommandIndex, Key.KeyChar.ToString());
                CommandIndex++;
            }

            // Backspace
            if (Key.Key == ConsoleKey.Backspace && CommandLine.Length > 0 && CommandIndex > 0)
            {
                CommandLine = CommandLine.Remove(CommandIndex - 1, 1);
                CommandIndex--;
            }

            // Cursor movement
            if (Key.Key == ConsoleKey.LeftArrow && CommandLine.Length > 0 && CommandIndex > 0)
                CommandIndex--;
            if (Key.Key == ConsoleKey.RightArrow && CommandLine.Length > 0 && CommandIndex <= CommandLine.Length - 1)
                CommandIndex++;
            if (Key.Key == ConsoleKey.Home)
                CommandIndex = 0;
            if (Key.Key == ConsoleKey.End)
                CommandIndex = CommandLine.Length;

            // History
            if (Key.Key == ConsoleKey.UpArrow && History.Count > 0)
            {
                HistoryIndex--;

                if (HistoryIndex < 0)
                    HistoryIndex = History.Count - 1;

                CommandLine = History[HistoryIndex];
                CommandIndex = History[HistoryIndex].Length;
            }
            if (Key.Key == ConsoleKey.DownArrow && History.Count > 0)
            {
                HistoryIndex++;

                if (HistoryIndex > History.Count - 1)
                    HistoryIndex = 0;

                CommandLine = History[HistoryIndex];
                CommandIndex = History[HistoryIndex].Length;
            }

            // Run Command
            if (Key.Key == ConsoleKey.Enter)
            {
                var valid = false;

                // Stop if the command line is blank
                if (string.IsNullOrEmpty(CommandLine))
                    Logger.WriteWarning("[CMD] No command specified");
                else
                {
                    // Iterate commands
                    foreach (var command in Commands)
                    {
                        var full = CommandLine;
                        var args = full.Split(' ');

                        if (command.Names.Any(name => args[0].ToLower() == name.ToLower()))
                        {
                            command.Run(args, args.Length, full, null);
                            valid = true;
                        }

                        if (valid)
                            break;
                    }

                    if (!valid)
                        Logger.WriteError("[CMD] {0} - Command not found", CommandLine.Split(' ')[0].Trim('\r'));

                    // Add the command line to history and wipe it
                    History.Add(CommandLine);
                    HistoryIndex = History.Count;
                    CommandLine = string.Empty;
                }

                CommandIndex = 0;
            }

            lock (consoleLock)
            {
                RefreshCommandLine();
                FixCursorPosition();
            }
        }

    }

    /// <summary>
    /// コンソールコマンドに関するクラス
    /// </summary>
    public class ConsoleCommand
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="cmdNames"></param>
        public ConsoleCommand(ConsoleCommandDelegate cmd, params string[] cmdNames)
        {
            if (cmdNames == null || cmdNames.Length < 1)
                throw new NotSupportedException();

            Command = cmd;
            Names = new List<string>(cmdNames);
            Arguments = new List<ConsoleCommandArgument>();
        }

        public ConsoleCommandDelegate Command { get; set; }
        public List<string> Names { get; set; }
        public List<ConsoleCommandArgument> Arguments { get; set; }
        public string Help { get; set; }

        /// <summary>
        /// コマンド実行
        /// </summary>
        /// <param name="args"></param>
        /// <param name="length"></param>
        /// <param name="full"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public bool Run(string[] args, int length, string full, Client client)
        {
            try
            {
                Command(args, length, full, client);
            }
            catch (IndexOutOfRangeException ex)
            {
                Logger.WriteException("Invalid command parameter", ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.WriteException("error in command", ex);
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// コンソールコマンドの引数に関するクラス
    /// </summary>
    public class ConsoleCommandArgument
    {
        public ConsoleCommandArgument(string name, bool optional)
        {
            Name = name;
            Optional = optional;
        }

        /// <summary>
        /// 名前
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 引数
        /// </summary>
        public bool Optional { get; set; }
    }
}
