using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneEmu
{
    /// <summary>
    /// コンフィグファイルにコメントするための属性
    /// </summary>
    public class ConfigComment : Attribute
    {
        public string Comment;
        public ServerType[] ServerTypes;

        public ConfigComment(string comment, params ServerType[] serverTypes)
        {
            Comment = comment;
            ServerTypes = serverTypes;
        }
    }

    /// <summary>
    /// 設定ファイルを扱うクラス
    /// </summary>
    class ServerConfig
    {
        /// <summary>
        /// 設定ファイルのフルパス
        /// </summary>
        private readonly string ConfigFile = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                                              Path.DirectorySeparatorChar;

        /// <summary>
        /// 自分のサーバーのタイプ
        /// </summary>
        private readonly ServerType MyServerType;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ServerConfig(ServerType serverType)
        {
            MyServerType = serverType;
            //設定ファイルの名前
            switch (serverType)
            {
                case ServerType.Community:
                    ConfigFile += "CommunityServer.cfg";
                    break;
                case ServerType.Game:
                    ConfigFile += "GameServer.cfg";
                    break;
                case ServerType.Login:
                    ConfigFile += "LoginServer.cfg";
                    break;
                default:
                    throw new ArgumentException("undefined servertype");
            }
        }

        //DB
        [ConfigComment("DBのホスト", ServerType.Game, ServerType.Login, ServerType.Community)]
        public string DatabaseHost = "localhost";

        [ConfigComment("DBサーバーにログインするためのユーザー名", ServerType.Game, ServerType.Login, ServerType.Community)]
        public string DatabaseUsername = "user";

        [ConfigComment("DBサーバーにログインするためのパスワード", ServerType.Game, ServerType.Login, ServerType.Community)]
        public string DatabasePassword = "pass";

        [ConfigComment("DBサーバーに接続するためのDB名", ServerType.Game, ServerType.Login, ServerType.Community)]
        public string DatabaseName = "redstone";

        [ConfigComment("BDサーバーに接続するためのポート", ServerType.Game, ServerType.Login, ServerType.Community)]
        public int DatabasePort = 5432;

        //ゲーム鯖情報
        [ConfigComment("サーバーのID ※建てた後変更しないこと", ServerType.Game)]
        public int ServerID = 0;

        [ConfigComment("表示されるサーバー名", ServerType.Game)]
        public string ServerName = "TEST";

        [ConfigComment("GameServerのタイプ", ServerType.Game)]
        public int serverType = 0;

        [ConfigComment("経験値倍率[倍]", ServerType.Game)]
        public double EXPrate = 1.0;

        [ConfigComment("ドロップ倍率[倍]", ServerType.Game)]
        public double dropRate = 1.0;

        [ConfigComment("ドロップゴールド倍率[倍]", ServerType.Game)]
        public double GOLDrate = 1.0;

        [ConfigComment("レベルキャップ", ServerType.Game)]
        public ushort LevelCap = 999;

        [ConfigComment("接続可能人数", ServerType.Game)]
        public int Connectable = 100;

        [ConfigComment("シーズン変数", ServerType.Game)]
        public int SeasonVariable = 2;

        [ConfigComment("国 0:韓国 1:日本", ServerType.Game)]
        public int Country = 1;

        [ConfigComment("ログイン時にユーザーに表示する日のメッセージ")]
        public string motd = "";

        [ConfigComment("接続されているすべてのクライアントからサーバーへのpingを実行する時間（秒単位）")]
        public double PingTime = 60;



        /// <summary>
        /// 設定ファイルのロード
        /// </summary>
        /// <returns>true:ロード成功　false:デフォルト設定を保存</returns>
        public bool Load()
        {
            try
            {
                // デフォルト設定を保存
                if (!File.Exists(ConfigFile))
                {
                    Save(true);
                    return false;
                }

                //全てのコメント属性付きのフィールド
                var fields = GetType().GetFields();

                //ファイルをすべて読み込み
                var lines = File.ReadAllLines(ConfigFile);

                foreach (var option in lines)
                {
                    //空白ラインは飛ばす
                    if (option.Length == 0)
                        continue;

                    //コメント分は飛ばす
                    if (option.StartsWith("//"))
                        continue;

                    //=で分解
                    var split = option.Split('=');

                    //全てのsplitをトリム（前後の空白を除去）する
                    for (var i = 0; i < split.Length; i++)
                        split[i] = split[i].Trim();

                    // Check length
                    if (split.Length != 2)
                    {
                        Logger.WriteWarning("[CFG] 不適切な分割サイズの設定行が見つかりました（ex. =が３つ）");
                        continue;
                    }

                    //optionのnameと一致するフィールド変数
                    var field = fields.FirstOrDefault(o => o.Name == split[0]);

                    //フィールド変数にセット
                    if (field != null)
                        ParseField(field, split[1]);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteException("設定を読み込む際のエラー", ex);
            }

            //一部の設定では手動による更新が必要
            SettingsChanged();

            Logger.WriteInternal("[CFG] 設定が読み込まれました");
            return true;
        }

        /// <summary>
        /// 設定のセーブ
        /// </summary>
        /// <param name="silent">セーブ完了メッセージを出さない</param>
        public void Save(bool silent = false)
        {
            //全てのコメント属性付きのフィールド変数を書式化する
            try
            {
                var data = new List<string>();

                //全てのコメント属性付きのフィールド
                var fields = GetType().GetFields();

                foreach (var field in fields)
                    SaveField(field, data);

                File.WriteAllLines(ConfigFile, data);
            }
            catch (Exception ex)
            {
                Logger.WriteException("設定の保存中にエラーが発生しました", ex);
            }

            if (!silent)
                Logger.WriteInternal("[CFG] 設定が保存されました");
        }

        /// <summary>
        /// 手動による更新
        /// </summary>
        public void SettingsChanged()
        {
            //Program.Instance.Server.PingTimer.Interval = 1000 * PingTime;
        }

        public bool SetField(string name, string value)
        {
            var fields = GetType().GetFields();
            var field = fields.FirstOrDefault(o => o.Name == name);

            if (field != null)
            {
                ParseField(field, value);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 文字列valueをfieldの型にパースしてフィールド変数にセット
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        private void ParseField(FieldInfo field, string value)
        {
            // Bool
            if (field.GetValue(this) is bool)
                field.SetValue(this, bool.Parse(value));

            // Int32
            if (field.GetValue(this) is int)
                field.SetValue(this, int.Parse(value));

            // Float
            if (field.GetValue(this) is float)
                field.SetValue(this, float.Parse(value));

            // Double
            if (field.GetValue(this) is double)
                field.SetValue(this, double.Parse(value));

            // String
            if (field.GetValue(this) is string)
            {
                value = value.Replace("\\n", "\n");
                field.SetValue(this, value);
            }

            // IP Address
            if (field.GetValue(this).GetType() == typeof(IPAddress))
                field.SetValue(this, IPAddress.Parse(value));

            // byte
            if (field.GetValue(this) is byte)
                field.SetValue(this, byte.Parse(value));

            // ushort
            if (field.GetValue(this) is ushort)
                field.SetValue(this, ushort.Parse(value));

            // Add more handling for special/custom types as needed
        }

        /// <summary>
        /// dataにfieldのconfigを書き込む
        /// </summary>
        /// <param name="field"></param>
        /// <param name="data"></param>
        private void SaveField(FieldInfo field, List<string> data)
        {
            //コメントなどの属性を抜き出す
            var attributes = (Attribute[])field.GetCustomAttributes(typeof(ConfigComment), false);

            //属性のインスタンス
            var commentAttr = (ConfigComment)attributes[0];

            //違うタイプのサーバーの場合は加えない
            if (!commentAttr.ServerTypes.Any(t => t == MyServerType))
            {
                return;
            }

            //属性が1つ以上ついている場合
            if (attributes.Length > 0)
            {
                //コメントつける
                data.Add("// " + commentAttr.Comment);
            }
            
            if (field.GetValue(this).GetType() == typeof(IPAddress))
            {
                //IP Address型のデータを加える
                var address = (IPAddress)field.GetValue(this);
                data.Add(field.Name + " = " + address);
            }
            else if (field.GetValue(this).GetType() == typeof(string))
            {
                //string型のデータを加える
                var str = (string)field.GetValue(this);
                data.Add(field.Name + " = " + str.Replace("\n", "\\n"));
            }
            else
            {
                //その他の型のデータを加える
                data.Add(field.Name + " = " + field.GetValue(this));
            }

            //オプションの間に空白行を残す
            data.Add(string.Empty);

            // Add more handling for special/custom types as needed
        }
    }
}
