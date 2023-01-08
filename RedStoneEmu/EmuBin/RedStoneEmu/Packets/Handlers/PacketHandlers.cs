using RedStoneLib.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneEmu.Packets.Handlers
{
    /// <summary>
    /// パケットハンドラの属性
    /// </summary>
    public class PacketHandlerAttr : Attribute
    {
        public UInt32 Type;

        public PacketHandlerAttr(UInt32 type)
        {
            Type = type;
        }
    }

    /// <summary>
    /// ハンドラの抽象体
    /// </summary>
    public abstract class PacketHandler
    {
        public abstract void HandlePacket(Client context, PacketReader reader, uint size);
    }

    public static class PacketHandlers
    {
        /// <summary>
        /// パケットコードとハンドラ（抽象体）の辞書
        /// </summary>
        private static readonly Dictionary<UInt32, PacketHandler> Handlers = new Dictionary<UInt32, PacketHandler>();

        /// <summary>
        /// パケットコードとハンドラ名の辞書
        /// </summary>
        public static readonly Dictionary<uint, string> HandlerNames = new Dictionary<uint, string>();

        /// <summary>
        /// アセンブリからハンドラのロード
        /// </summary>
        public static void LoadPacketHandlers()
        {
            //パケットハンドラとなるクラスを抽出
            var classes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsClass && (t.Namespace?.Contains("RedStoneEmu.Packets.Handlers") ?? false) && t.IsSubclassOf(typeof(PacketHandler)))
                .ToList();

            foreach (var t in classes)
            {
                var attrs = t.GetCustomAttributes(typeof(PacketHandlerAttr), false) as PacketHandlerAttr[];

                if (attrs.Length > 0)
                {
                    var attr = attrs[0];
                    Logger.Write("[PKT] [{0:X}] {1}", attr.Type, t.Name);
                    HandlerNames[attr.Type] = t.Name;
                    if (!Handlers.ContainsKey(attr.Type))
                        Handlers.Add(attr.Type, (PacketHandler)Activator.CreateInstance(t));
                }
            }
        }

        /// <summary>
        /// 指定されたパケットタイプとサブタイプのPacketHandlerを取得して作成
        /// </summary>
        /// <returns>PacketHandlerのインスタンスまたはnull</returns>
        /// <param name="type">Type a.</param>
        /// <param name="subtype">Type b.</param>
        public static PacketHandler GetHandlerFor(uint type)
        {
            PacketHandler handler = null;

            Handlers.TryGetValue(type, out handler);

            return handler;
        }
    }
}
