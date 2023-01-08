using RedStoneLib.Model.Effect;
using RedStoneLib.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Model.Base
{
    /// <summary>
    /// ゲームに存在するOP情報
    /// </summary>
    public class OPBase
    {
        /// <summary>
        /// OP全て
        /// </summary>
        public static OPBase[] AllOPBases { get; private set; } = null;

        /// <summary>
        /// 全てのOPを読み込む
        /// </summary>
        public static void Load()
        {
            using (PacketReader br = new PacketReader(Helper.StreamFromAssembly("Scenario.Red_Stone.item.dat")))
            {
                //復号化キーのセット
                br.SetDataEncodeTable(br.ReadInt32());

                //アイテムのサイズ
                int itemLength = br.EncryptionRead<int>();

                //どこかへのオフセット
                uint unknownOffset = br.ReadUInt32();

                //全アイテムスキップ
                br.BaseStream.Seek(itemLength * ItemBase.GetItemBaseInfoSize(), System.IO.SeekOrigin.Current);

                //OPのサイズ
                int OptionLength = br.EncryptionRead<int>();

                //全OP取得
                AllOPBases = br.EncryptionReads<OPBaseInfo>(OptionLength, sequentially: true).Select(t => new OPBase(t)).ToArray();
            }
        }

        /// <summary>
        /// 実態
        /// </summary>
        private OPBaseInfo m_value;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="opBase">他のOPBase</param>
        public OPBase(OPBase opBase)
        {
            m_value = opBase.m_value;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="opBaseInfo">他のOP実態</param>
        private OPBase(OPBaseInfo opBaseInfo)
        {
            m_value = opBaseInfo;
        }

        /// <summary>
        /// デフォルト
        /// </summary>
        public OPBase()
        {
            m_value = new OPBaseInfo();
            m_value.Index = 0xFFFF;
            m_value.Name1 = "NULL";
            m_value.Name2 = "NULL";
        }

        /// <summary>
        /// OPのインデックス
        /// </summary>
        public ushort Index => m_value.Index;
        
        /// <summary>
        /// OPの効果
        /// </summary>
        public PlayerEffect.OPEffect Effect => m_value.Effect;

        /// <summary>
        /// OP値の最大最小値
        /// </summary>
        public Scale<ushort>[] OPValueScales => m_value.OPValueScales;

        /// <summary>
        /// OP名1
        /// </summary>
        public string Name1 => m_value.Name1;

        /// <summary>
        /// OP名2
        /// </summary>
        public string Name2 => m_value.Name2;

        /// <summary>
        /// 要求レベル
        /// </summary>
        public ushort RequireLevel => m_value.RequireLevel;
        
        /// <summary>
        /// 原価
        /// </summary>
        public uint PriceBase => m_value.PriceBase;

        /// <summary>
        /// PriceBaseから販売価格を計算する種類
        /// </summary>
        public SellingPriceCalculationType PriceType => m_value.PriceType;

        /// <summary>
        /// 付加フラグ？
        /// </summary>
        public byte[] UnknownAdditionFlags => m_value.UnknownAdditionFlags;

        /// <summary>
        /// 装備に付加するフラグ
        /// </summary>
        public byte[] EquipmentAdditionFlags => m_value.EquipmentAdditionFlags;

        /// <summary>
        /// ドロップ係数
        /// </summary>
        public ushort DropCoefficient => m_value.DropCoefficient;

        /// <summary>
        /// 武器の色
        /// </summary>
        public byte WeaponColor => m_value.WeaponColor;

        /// <summary>
        /// 空フラグ
        /// </summary>
        public bool IsEmpty { get => m_value.Index >= 0xFFFF; }

        /// <summary>
        /// 弱効果フラグ
        /// </summary>
        public bool IsWeak => m_value.Name1.Contains("弱効果");

        /// <summary>
        /// Override
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return m_value.Name1;
        }

        /// <summary>
        /// OP
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        private struct OPBaseInfo
        {
            /// <summary>
            /// OPのインデックス
            /// </summary>
            public ushort Index;

            public ushort Unknown_0;

            /// <summary>
            /// OPの効果
            /// </summary>
            public PlayerEffect.OPEffect Effect;

            /// <summary>
            /// OP値の最大最小値
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x02)]
            public Scale<ushort>[] OPValueScales;

            public ushort Unknown_1;

            /// <summary>
            /// OP名1
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x14)]
            public string Name1;

            /// <summary>
            /// OP名2
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x14)]
            public string Name2;

            /// <summary>
            /// 要求レベル
            /// </summary>
            public ushort RequireLevel;

            public ushort Unknown_2;

            /// <summary>
            /// 原価
            /// </summary>
            public uint PriceBase;

            /// <summary>
            /// PriceBaseから販売価格を計算する種類
            /// </summary>
            public SellingPriceCalculationType PriceType;

            public ushort Unknown_3;

            /// <summary>
            /// 付加フラグ？
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x04)]
            public byte[] UnknownAdditionFlags;

            /// <summary>
            /// 装備に付加するフラグ
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x2C)]
            public byte[] EquipmentAdditionFlags;

            /// <summary>
            /// 武器の色
            /// </summary>
            public byte WeaponColor;

            public byte Unknown_4;

            public ushort Unknown_5;

            /// <summary>
            /// ドロップ係数
            /// </summary>
            public ushort DropCoefficient;

            public ushort Unknown_6;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x24)]
            public byte[] Unknown_7;
            
        }

        /// <summary>
        /// PriceBaseから販売価格を計算する種類（OP版）
        /// </summary>
        public enum SellingPriceCalculationType : ushort
        {
            /// <summary>
            /// ベースプライス*100
            /// </summary>
            BasePrice_times_100 = 0,

            /// <summary>
            /// Value1*100
            /// </summary>
            Value1_times_100 = 1,

            /// <summary>
            /// Value1*ベースプライス*100
            /// </summary>
            Value1_times_BasePrice_times_100 = 2,

            /// <summary>
            /// Value1*100/ベースプライス
            /// </summary>
            Value1_times_100_devide_BasePrice = 3,

            /// <summary>
            /// Value1*ベースプライス
            /// </summary>
            Value1_times_BasePrice = 4,

            /// <summary>
            /// Value2*ベースプライス*100/Value1
            /// </summary>
            Value2_times_BasePrice_times_100_devide_Value1 = 5,
        }
    }
}
