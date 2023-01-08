#define PARALLEL
using RedStoneLib.Karmas;
using RedStoneLib.Model.Base;
using RedStoneLib.Packets;
using RedStoneLib.Packets.RSPacket.ActorInfo;
using RedStoneLib.Packets.RSPacket.GameLogin;
using RedStoneLib.Packets.RSPacket.MoveField;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RedStoneLib.Model
{
    /// <summary>
    /// マップのモデル
    /// </summary>
    public class Map
    {
        /// <summary>
        /// 全MAPここに保存
        /// </summary>
        public static Dictionary<ushort, Map> AllMaps { get; private set; } 
            = new Dictionary<ushort, Map>();

        /// <summary>
        /// MAPの情報
        /// </summary>
        public static Dictionary<ushort, (Size<ushort> size, MapType type, string name, string fileName)> AllMapInfos { get; private set; }
            =new Dictionary<ushort, (Size<ushort> size, MapType type, string name, string fileName)>();

        /// <summary>
        /// マップ名
        /// </summary>
        public string Name { get => Header.Name; }

        /// <summary>
        /// rmdファイル名
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// 連続ID
        /// </summary>
        public ushort SerialID { get; private set; }

        /// <summary>
        /// サイズ
        /// </summary>
        public Size<uint> Size { get => Header.Size; }

        /// <summary>
        /// マップのタイプ
        /// </summary>
        public MapType Type { get => (MapType)Header.TypeAndFlags; }

        /// <summary>
        /// マップのフラグ
        /// </summary>
        public MapFlags Flags { get => (MapFlags)Header.TypeAndFlags; }

        /// <summary>
        /// ブロック（1次元）
        /// </summary>
        private byte[] Blocks { get; set; }

        /// <summary>
        /// ヘッダ
        /// </summary>
        public readonly MapHeader Header;

        /// <summary>
        /// NPCグループ情報
        /// </summary>
        public Dictionary<ushort, Actor.MapActorGroup> NpcGroups
            = new Dictionary<ushort, Actor.MapActorGroup>();

        /// <summary>
        /// NPC単体情報
        /// </summary>
        public readonly Actor.MapActorSingle[] NpcSingles;

        /// <summary>
        /// エリア情報
        /// </summary>
        public readonly AreaInfo[] AreaInfos;

        /// <summary>
        /// NPCなどの店
        /// </summary>
        public readonly Shop[] Shops;

        public override string ToString() => Name;

        /// <summary>
        /// 全てのマップ読み込み
        /// </summary>
        public static void LoadAllMaps()
        {
            //Mapリスト読み込み
            using (PacketReader br = new PacketReader(Helper.StreamFromAssembly("mapList.dat")))
            {
                while (true)
                {
                    ushort index = br.ReadUInt16();
                    if (index == 0xFFFF) break;
                    AllMapInfos[index] = (size: br.ReadStruct<Size<ushort>>(), type: (MapType)br.ReadUInt16(), name: br.ReadSjis(0x40), fileName: br.ReadSjis(0x40));
                }
            }

            //Map情報読み込み
            List<(ushort serialID, string fileName, Stream stream)> nameAndStreamList = 
                AllMapInfos.Select(t => (t.Key, t.Value.fileName, Helper.StreamFromAssembly($"Scenario.Red_Stone.Map.{t.Value.fileName}"))).ToList();

            //全マップ読み込み
#if PARALLEL
            Parallel.ForEach(nameAndStreamList, nameAndStream =>
#else
            foreach (var nameAndStream in nameAndStreamList)
#endif
            {
                Map map = new Map(nameAndStream.fileName, nameAndStream.serialID, nameAndStream.stream);
                lock (AllMaps)
                {
                    AllMaps[nameAndStream.serialID] = map;
                }
#if PARALLEL
            });
#else
            }
#endif
            AllMaps = AllMaps.OrderBy(t => t.Key).ToDictionary(t => t.Key, t => t.Value);
        }

        /// <summary>
        /// ファイル名からMAPを生成
        /// </summary>
        /// <param name="fileName"></param>
        protected Map(string fileName, ushort serialID, Stream mapStream)
        {
            FileName = fileName;
            SerialID = serialID;

            using (PacketReader br = new PacketReader(mapStream))
            {
                //ファイルサイズ
                uint size = br.ReadUInt32();

                //ポータル情報へのオフセット
                int portalAreaOffset = br.ReadInt32();

                //ファイルシステム情報
                string scenarioInfo = br.ReadSjis(0x38);

                //フィールド情報へのオフセット
                int fieldAreaOffset = br.ReadInt32();

                //マップ基本情報読み込み
                Header = br.ReadStruct<MapHeader>();

                //暗号化レベル
                double version = Convert.ToDouble(string.Concat(scenarioInfo.Skip(24).Take(3)));
                switch (version)
                {
                    case double n when n <= 6.0 && n > 5.7:
                        br.SetDataEncodeTable(-1);
                        break;
                    case double n when n > 6.0:
                        br.SetDataEncodeTable(br.ReadInt32());
                        break;
                }

                //real obj data? 不明
                uint unknown_1 = br.ReadUInt32();

                //read data マップのタイル情報らしきもの
                br.BaseStream.Seek(Header.Size.Width * Header.Size.Height * 6, SeekOrigin.Current);

                //read door list ドア情報
                ulong[] doorList = new ulong[br.ReadInt32()];
                for (int i = 0; i < doorList.Length; i++)
                {
                    doorList[i] = br.ReadUInt64();
                }

                //フィールドマップ
                Blocks = br.ReadBytes((int)(Header.Size.Width * Header.Size.Height));

                //オブジェクト読み込み
                ushort[] charIndexes = null;
                foreach (LoadingTarget target in Enum.GetValues(typeof(LoadingTarget)))
                {
                    //1回飛ばすためのオフセット
                    int nextOffset = br.ReadInt32();
                    switch (target)
                    {
                        case LoadingTarget.Unknown:
                            //後で解析
                            br.BaseStream.Seek(nextOffset, SeekOrigin.Begin);
                            break;
                        case LoadingTarget.NpcLength:
                            //read character data
                            //MAP内のNPC数＆Index取得
                            charIndexes = br.EncryptionReads<ushort>(br._Decryption(nextOffset));
                            break;
                        case LoadingTarget.NpcGroupInfo:
                            //NPCグループ構造体のサイズ
                            int NpcGroupInfoLength = version > 5.4 ? br.ReadInt32() : 0x2C;

                            //NPCグループ詳細
                            for (int i = 0; i < charIndexes.Length; i++)
                            {
                                //敵集団情報
                                var newNPCGroupInfo = new Actor.MapActorGroup(br, NpcGroupInfoLength, charIndexes[i]);
                                NpcGroups[newNPCGroupInfo.InternalID] = newNPCGroupInfo;
                            }
                            break;
                        case LoadingTarget.NpcSingleInfo:
                            //NPC単体情報
                            NpcSingles = new Actor.MapActorSingle[br.EncryptionRead<int>()];

                            for (int i = 0; i < NpcSingles.Length; i++)
                            {
                                //単体情報取得
                                NpcSingles[i] = new Actor.MapActorSingle(br);
                            }
                            break;
                        case LoadingTarget.AreaInfo:
                            //area info（ポータル情報など）
                            AreaInfos = new AreaInfo[br._Decryption(nextOffset)];
                            for (int i = 0; i < AreaInfos.Length; i++)
                            {
                                AreaInfos[i] = new AreaInfo(br, portalAreaOffset, version);
                            }
                            break;
                        case LoadingTarget.ShopInfo:
                            //shop情報
                            Shops = new Shop[br.ReadInt16()];
                            for (int i = 0; i < Shops.Length; i++)
                            {
                                Shops[i] = new Shop(br, version);
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 座標からブロックを求める
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public byte GetBlock(int x, int y)
        {
            if (Blocks.Length <= y * Size.Width + x)
                return 1;
            return Blocks[y * Size.Width + x];
        }

        /// <summary>
        /// 座標間の障害物チェック（精密な座標を使用）
        /// </summary>
        /// <param name="ax"></param>
        /// <param name="ay"></param>
        /// <param name="bx"></param>
        /// <param name="by"></param>
        /// <returns></returns>
        public bool IsBlockedWay(int ax, int ay, int bx, int by)
        {
            double lerp(double x00, double y00, double x11, double y11, double x)
                => y00 + (y11 - y00) * (x - x00) / (x11 - x00);

            int x0 = Math.Min(ax, bx);
            int x1 = Math.Max(ax, bx);
            int y0 = Math.Min(ay, by);
            int y1 = Math.Max(ay, by);
            if (bx - ax > by - ay)
            {
                for (int x = x0; x < x1; x++)
                {
                    if (GetBlock(x / 64, (int)lerp(x0, y0, x1, y1, x) / 32) != 0)
                        return true;
                }
            }
            else
            {
                for (int y = y0; y < y1; y++)
                {
                    if (GetBlock((int)lerp(y0, x0, y1, x1, y) / 64, y / 32) != 0)
                        return true;
                }
            }
            return false;
        }


        /// <summary>
        /// MAPのscenarioで読み込む対象
        /// </summary>
        private enum LoadingTarget : int
        {
            Unknown = 0,
            NpcLength = 1,
            NpcGroupInfo = 2,
            NpcSingleInfo = 3,
            AreaInfo = 4,
            ShopInfo = 5,
        }

        /// <summary>
        /// マップの基本的な情報
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct MapHeader
        {
            public Size<uint> Size;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x40)]
            public string Name;

            public uint unknown_0;

            /// <summary>
            /// マップタイプとマップフラグの共用体
            /// </summary>
            public byte TypeAndFlags;

            /// <summary>
            /// ミニマップレベルとその他のビットフィールド
            /// </summary>
            public byte MinimapAndOther;

            public uint unknown_1;
            public uint unknown_2;

            /// <summary>
            /// all 0xFF
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x3A)]
            public byte[] unknown_3;

            public uint unknown_4;

            /// <summary>
            /// 属性低下
            /// </summary>
            public Magic MagicDecline;

            /// <summary>
            /// 属性上昇
            /// </summary>
            public Magic MagicRising;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x1C)]
            public byte[] unknown_5;
        }

        /// <summary>
        /// エリアインフォ
        /// </summary>
        public struct AreaInfo
        {
            /// <summary>
            /// インデックス
            /// </summary>
            public ushort Index;

            private Point<uint> LeftUpPos;
            private Point<uint> RightDownPos;

            /// <summary>
            /// 位置
            /// </summary>
            public Point<uint> CenterPos { get => (LeftUpPos + RightDownPos) / 2.0; }

            /// <summary>
            /// タイプ
            /// </summary>
            public ObjectType ObjectInfo;

            public ushort SubObjectInfo;

            public ushort unknown_0;
            public ushort unknown_1;

            /// <summary>
            /// 韓国語コメント１
            /// </summary>
            public string Comment1;

            /// <summary>
            /// 韓国語コメント２
            /// </summary>
            public string Comment2;

            /// <summary>
            /// 移動先マップのファイル名
            /// </summary>
            public string MovetoFileName;

            /// <summary>
            /// イベントオブジェクトのKarma
            /// </summary>
            public Event[] AreaEvents;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="pr"></param>
            /// <param name="portalAreaOffset"></param>
            public AreaInfo(PacketReader pr, int portalAreaOffset, double scenarioVersion)
            {
                //基本情報
                using (PacketReader baseReader = new PacketReader(
                    pr.EncryptionReads<byte>(0xA2)))
                {
                    Index = baseReader.ReadUInt16();
                    LeftUpPos = baseReader.ReadStruct<Point<uint>>();
                    RightDownPos = baseReader.ReadStruct<Point<uint>>();
                    ObjectInfo = (ObjectType)baseReader.ReadUInt16();
                    SubObjectInfo = baseReader.ReadUInt16();
                    unknown_0 = baseReader.ReadUInt16();
                    unknown_1 = baseReader.ReadUInt16();

                    //イベントオブジェクトは日本語
                    string code = ObjectInfo == ObjectType.EventObject ? "Shift_JIS" : "EUC-KR";
                    Comment1 = baseReader.ReadSjis(0x21, code);
                    Comment2 = baseReader.ReadSjis(0x67, code);
                }

                //イベントスキップした後のアドレス
                //version4.4以下はイベントがないので何の値か不明
                int skipPos = scenarioVersion > 4.4 ? pr.ReadInt32() : pr.ReadInt16() + 2;

                //イベント取得
                AreaEvents = new Event[skipPos > 1 ? pr.ReadInt16() : 0];
                if (AreaEvents.Length != 0)
                {
                    uint unk = pr.ReadUInt32();
                    for (int i = 0; i < AreaEvents.Length; i++)
                    {
                        AreaEvents[i] = new Event(pr, 0);
                    }
                }

                //移動先のファイル名取ってくる
                int myPortalStringOffset = pr.ReadInt32();
                if (myPortalStringOffset == -1)
                {
                    MovetoFileName = null;
                }
                else
                {
                    //現在地を記憶
                    int returnPosition = (int)pr.BaseStream.Position;

                    //ポータルエリアにジャンプ
                    pr.BaseStream.Seek(myPortalStringOffset + portalAreaOffset, SeekOrigin.Begin);

                    MovetoFileName = pr.ReadSjis(pr.ReadInt32() + 1);

                    //戻る
                    pr.BaseStream.Seek(returnPosition, SeekOrigin.Begin);
                }
            }

            public override string ToString() => Comment1;

            /// <summary>
            /// エリアデータのオブジェクトタイプ
            /// </summary>
            public enum ObjectType : ushort
            {
                System = 0,//システム
                Unk1 = 1,//
                Door = 2,//扉
                WarpPortal = 3,//ワープポータル
                SystemArea = 4,//システム領域
                SystemMovePosition = 5,//システム転送位置
                Area = 6,//エリア
                PvPMovePosition = 7,//PvP転送位置
                OXArea_O = 8,//○×クイズ領域(○)
                OXArea_X = 9,//○×クイズ領域(×)
                Unk2 = 10,// 
                TrapFloor = 11,// トラップ床
                EventObject = 12,// イベントオブジェクト
                Chest = 13,// 宝箱
                Unk3 = 14,// 
                Unk4 = 15,// 
                Unk5 = 16,// 
                HuntingArea = 17,// 冒険家協会推奨狩場
                SystemArea2 = 18,// システムエリア
                Unk6 = 19,// 
                Unk7 = 20,//
            }
        }

        public enum MapType : byte
        {
            Dungeon = 1,
            Village = 2,
            Shop = 3,
        }


        [Flags]
        public enum MapFlags : byte
        {
            Premium = 0x10,
            SiegeWarfareField = 0x20,
            Bank = 128,
            PVP = 0x80 | 0x01
        }

    }
}
