using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneEmu
{
    /// <summary>
    /// 様々なログを表示・保存する
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// ログ保存用
        /// </summary>
        private static StreamWriter Writer;

        /// <summary>
        /// 保存先を決める
        /// </summary>
        /// <param name="logdir"></param>
        /// <param name="logname"></param>
        public static void SetLogFileName(ServerType serverType)
        {
            //サーバータイプの名前
            string serverTypeName = string.Join("+", ((ServerType[])Enum.GetValues(typeof(ServerType))).Where(t => serverType.HasFlag(t)).Select(t => t.ToString()));

            //ログファイルのパス
            string logdir = Path.Combine(".", "[LOG]" + serverTypeName);

            //ログファイルの名前
            string logname = DateTime.Now.ToString("yyyy.MM.dd_HH.mm.ss.fff");

            if (!Directory.Exists(logdir))
            {
                //保存ディレクトリがない場合は作成
                Directory.CreateDirectory(logdir);
            }

            Writer = new StreamWriter(Path.Combine(logdir, logname + ".log"), true);
        }

        public static void Write(string text, params object[] args)
        {
            AddLine(ConsoleColor.White, string.Format(text, args));
            WriteFile(text, args);
        }
        public static void WriteGreen(string text, params object[] args)
        {
            AddLine(ConsoleColor.Green, string.Format(text, args));
            WriteFile(text, args);
        }

        public static void WriteDB(string text, params object[] args)
        {
            AddLine(ConsoleColor.DarkGreen, string.Format(text, args));
            WriteFile(text, args);
        }

        public static void WriteInternal(string text, params object[] args)
        {
            AddLine(ConsoleColor.Cyan, string.Format(text, args));
            WriteFile(text, args);
        }

        public static void WriteCommand(Client client, string text, params object[] args)
        {
            if (client == null)
            {
                AddLine(ConsoleColor.Green, string.Format(text, args));
                WriteFile(text, args);
            }
            else
                WriteClient(client, text, args);
        }

        public static void WriteClient(Client client, string text, params object[] args)
        {
            /*
            var message = string.Format(text, args).Replace('\\', '/');
            var packet = new SystemMessagePacket(message, SystemMessagePacket.MessageType.SystemMessage);
            client.SendPacket(packet);*/
        }

        public static void WriteWarning(string text, params object[] args)
        {
            AddLine(ConsoleColor.Yellow, string.Format(text, args));
            WriteFile(text, args);
        }

        public static void WriteError(string text, params object[] args)
        {
            AddLine(ConsoleColor.Red, string.Format(text, args));
            WriteFile(text, args);
        }

        /// <summary>
        /// 例外の書き込み
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public static void WriteException(string message, Exception ex)
        {
            var text = string.Empty;

            text += string.Format("[ERR] {0} - {1}: {2}", message, ex.GetType(), ex);
            if (ex.InnerException != null)
                text += string.Format("\n[ERR] Inner Exception: {0}", ex.InnerException);

            WriteFile(text);

            AddLine(ConsoleColor.Red, text);
        }

        /// <summary>
        /// コンソールに書き込み
        /// </summary>
        /// <param name="color"></param>
        /// <param name="text"></param>
        private static void AddLine(ConsoleColor color, string text)
        {
            ConsoleSystem.AddLine(color, text);
        }

        /// <summary>
        /// ファイルに書き込み
        /// </summary>
        /// <param name="text"></param>
        /// <param name="args"></param>
        public static void WriteFile(string text, params object[] args)
        {
            //writerがnullなら書き込まない
            if (Writer == null) return;

            if (args.Length > 0)
                Writer.WriteLine(DateTime.Now + " - " + text, args);
            else
                Writer.WriteLine(DateTime.Now + " - " + text);

            // その場で書き込む
            Writer.Flush();
        }

        /// <summary>
        /// arrayを書き込み
        /// </summary>
        /// <param name="text"></param>
        /// <param name="array"></param>
        public static void WriteHex(string text, byte[] array)
        {
            AddLine(ConsoleColor.DarkCyan, text);

            // Calculate lines
            var lines = 0;
            for (var i = 0; i < array.Length; i++)
                if ((i % 16) == 0)
                    lines++;

            for (var i = 0; i < lines; i++)
            {
                var hexString = string.Empty;

                // Address
                hexString += string.Format("{0:X8} ", i * 16);

                // Bytes
                for (var j = 0; j < 16; j++)
                {
                    if (j + (i * 16) >= array.Length)
                        break;

                    hexString += string.Format("{0:X2} ", array[j + (i * 16)]);
                }

                // Spacing
                while (hexString.Length < 16 * 4)
                    hexString += ' ';

                // ASCII
                for (var j = 0; j < 16; j++)
                {
                    if (j + (i * 16) >= array.Length)
                        break;

                    var asciiChar = (char)array[j + (i * 16)];

                    if (asciiChar == (char)0x00)
                        asciiChar = '.';

                    hexString += asciiChar;
                }

                // Strip off unnecessary stuff
                hexString = hexString.Replace('\a', ' '); // Alert beeps
                hexString = hexString.Replace('\n', ' '); // Newlines
                hexString = hexString.Replace('\r', ' '); // Carriage returns
                hexString = hexString.Replace('\\', ' '); // Escape break

                AddLine(ConsoleColor.White, hexString);
                WriteFile(hexString);
            }
        }
    }
}
