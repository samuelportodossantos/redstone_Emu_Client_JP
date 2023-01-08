using RedStoneLib.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Karmas
{

    /// <summary>
    /// 識別用
    /// </summary>
    public class KarmaItemAttr : Attribute
    {
        public ushort Index;
        public int[] ToUseValueIndex;

        public KarmaItemAttr(ushort index, params int[] toUseValueIndex)
        {
            Index = index;
            ToUseValueIndex = toUseValueIndex;
        }
    }

    /// <summary>
    /// コマンドハンドラの抽象体
    /// </summary>
    public abstract class KarmaItemExecuteService
    {
        public abstract void HandleKarmaItem(Player player, uint[] value, ushort npcCharID, SendPacketDelegate sendPacket);
    }

    /// <summary>
    /// 条件ハンドラの抽象体
    /// </summary>
    public abstract class KarmaItemConditionService
    {
        public abstract bool HandleKarmaItem(Player player, uint[] value);
    }

    public static class KarmaItemServices
    {
        /// <summary>
        /// コマンドハンドラの辞書
        /// </summary>
        private static readonly Dictionary<ushort, KarmaItemExecuteService> CommandHandlers = new Dictionary<ushort, KarmaItemExecuteService>();

        /// <summary>
        /// 条件ハンドラの辞書
        /// </summary>
        private static readonly Dictionary<ushort, KarmaItemConditionService> ConditionHandlers = new Dictionary<ushort, KarmaItemConditionService>();

        /// <summary>
        /// アセンブリからハンドラのロード
        /// </summary>
        public static void Load()
        {
            //パケットハンドラとなるクラスを抽出
            var classes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsClass);

            //ハンドラセット
            void SetHandler<T>(Dictionary<ushort, T> dic, string name)
            {
                foreach (var t in classes.Where(t => t.IsSubclassOf(typeof(T))))
                {
                    var attrs = t.GetCustomAttributes(typeof(KarmaItemAttr), false) as KarmaItemAttr[];

                    if (attrs.Length > 0)
                    {
                        var attr = attrs[0];
                        PublicHelper.WriteInternal?.Invoke("[{0}] [{1:X}] {2}", name, attr.Index, t.Name);
                        if (!dic.ContainsKey(attr.Index))
                            dic.Add(attr.Index, (T)Activator.CreateInstance(t));
                    }
                }
            }

            //コマンド
            SetHandler(CommandHandlers, "Commands");
            //コマンド
            SetHandler(ConditionHandlers, "Conditions");
        }

        /// <summary>
        /// 定義されてないハンドラチェック&定義されてるkarmaItemを出力
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dic"></param>
        /// <param name="items"></param>
        /// <param name="noticeItemStr"></param>
        /// <returns>定義されてるkarmaItemの配列</returns>
        static List<KarmaItem> Notice<T>(Dictionary<ushort, T> dic, KarmaItem[] items, string noticeItemStr)
        {
            if (items.Any(karmaItem => !dic.ContainsKey(karmaItem._Index)))
            {
                //列挙
                foreach (var karmaItem in items.Where(t => !dic.ContainsKey(t._Index)))
                {
                    PublicHelper.WriteWarning?.Invoke($"[{noticeItemStr}]no define: 0x{karmaItem._Index:X} ({string.Join(",", karmaItem.Value)})");
                }
            }
            return items.Where(t => dic.ContainsKey(t._Index)).ToList();
        }

        /// <summary>
        /// チェック
        /// </summary>
        /// <param name="player"></param>
        /// <param name="conditions"></param>
        /// <param name="conditionFlag"></param>
        /// <returns></returns>
        public static bool CheckConditions(Player player,KarmaItemCondition[] conditions, Karma.ConditionFlag conditionFlag)
        {
            if (conditions.Length == 0) return true;

            //定義チェック
            var safety = Notice(ConditionHandlers, conditions, "CONDITION");

            //not
            bool isTrue = !conditionFlag.HasFlag(Karma.ConditionFlag.Not);

            switch (conditionFlag)
            {
                case Karma.ConditionFlag flag when flag.HasFlag(Karma.ConditionFlag.And):
                    {
                        var result = safety.All(t => ConditionHandlers[t._Index]?.HandleKarmaItem(player, t.Value) ?? true);
                        return result && isTrue || !result && !isTrue;
                    }
                case Karma.ConditionFlag flag when flag.HasFlag(Karma.ConditionFlag.Or):
                    {
                        var result = safety.Any(t => ConditionHandlers[t._Index]?.HandleKarmaItem(player, t.Value) ?? false);
                        return result && isTrue || !result && !isTrue;
                    }
            }
            throw new ArgumentException("ConditionFlagが異常です．");
        }

        /// <summary>
        /// 実行
        /// </summary>
        /// <param name="player"></param>
        /// <param name="commands"></param>
        /// <param name="sendPacket"></param>
        public static void ExecuteCommands(Player player, KarmaItemCommand[] commands, SendPacketDelegate sendPacket, ushort npcCharID)
        {
            //定義チェック
            var safety = Notice(CommandHandlers, commands, "COMMAND");

            //全て実行
            safety.ForEach(t => CommandHandlers[t._Index].HandleKarmaItem(player, t.Value, npcCharID, sendPacket));
        }

        /// <summary>
        /// 指定されたパケットタイプとサブタイプのPacketHandlerを取得して作成
        /// </summary>
        /// <returns>PacketHandlerのインスタンスまたはnull</returns>
        /// <param name="index">Type a.</param>
        public static KarmaItemExecuteService GetHandlerFor(ushort index)
        {
            KarmaItemExecuteService handler = null;

            CommandHandlers.TryGetValue(index, out handler);

            return handler;
        }
    }
}
