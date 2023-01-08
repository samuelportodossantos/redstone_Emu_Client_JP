using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RedStoneLib.Model.Base;
using RedStoneLib.Model.Effect;

namespace RedStoneLib.Model
{
    public sealed class Item : IComparable, ICloneable
    {
        /// <summary>
        /// ItemInfoの実態
        /// </summary>
        private ItemInfo m_value;

        /// <summary>
        /// プレイヤーエフェクト
        /// </summary>
        public PlayerEffect Effect;

        /// <summary>
        /// インデックス指定
        /// </summary>
        /// <param name="itemIndex"></param>
        public Item(ushort itemIndex)
        {
            Init(itemIndex);
        }

        /// <summary>
        /// インデックス・個数指定
        /// </summary>
        /// <param name="itemIndex"></param>
        public Item(ushort itemIndex, byte number)
        {
            Init(itemIndex, number);
        }

        /// <summary>
        /// itemInfoから生成
        /// </summary>
        /// <param name="itemInfo"></param>
        public Item(ItemInfo itemInfo)
        {
            m_value = itemInfo;
        }

        /// <summary>
        /// paramaterLess
        /// </summary>
        public Item()
        {
            Init();
        }

        /// <summary>
        /// アイテムの主キー（シーケンシャル）
        /// </summary>
        [Key]
        public int UniqueID
        {
            get => (int)m_value.UniqueID;
            set => m_value.UniqueID = (uint)value;
        }

        /// <summary>
        /// アイテムのインデックス
        /// </summary>
        public short ItemIndex
        {
            get => (short)m_value.ItemIndex;
            set
            {
                m_value.ItemIndex = (ushort)value;
                Effect = Base.UniqueOP;
            }
        }

        /// <summary>
        /// 個数
        /// </summary>
        public byte Count
        {
            get => m_value.Number;
            set => m_value.Number = value;
        }

        /// <summary>
        /// 耐久力
        /// </summary>
        public byte Endurance
        {
            get => m_value.Endurance;
            set => m_value.Endurance = value;
        }

        /// <summary>
        /// アイテム固有の値
        /// </summary>
        public byte[] Values
        {
            get => m_value.Values;
            set
            {
                if (m_value.Values == null) m_value.Values = new byte[2];
                m_value.Values = value;

                //アイテム効果
                foreach(var ef in Base.ItemEffects)
                {
                    if (Effect == null) Effect = new PlayerEffect();
                    Effect += (ef.effect, ef.paramIndex.Select(t => t != 0xFFFF ? value[t] : (byte)0xFF).ToArray());
                }
            }
        }

        /// <summary>
        /// OP（DB用）
        /// </summary>
        [Column("OPs")]
        public byte[] _ItemOptionInfos
        {
            get => m_value.OPs.SelectMany(t => Helper.StructToBytes(t)).ToArray();
            set
            {
                int opSize = Marshal.SizeOf(typeof(OP));

                //セット
                m_value.OPs = Enumerable.Range(0, 3)
               .Select(t => Helper.BytesToStruct<OP>(value.Skip(t * opSize).Take(opSize).ToArray()))
               .ToArray();

                //効果
                foreach(var op in m_value.OPs.TakeWhile(t => !t.IsEmpty))
                {
                    if (Effect == null) Effect = new PlayerEffect();
                    Effect += op.Effect;
                }
            }
        }

        /// <summary>
        /// OP
        /// </summary>
        [NotMapped]
        public OP[] OPs => m_value.OPs;

        /// <summary>
        /// 不明なフラグ
        /// </summary>
        public bool unk_bool1
        {
            get => (m_value.flags & 1) == 1 ? true : false;
            set => m_value.flags |= (uint)((value ? 1 : 0) & 0x01);
        }

        /// <summary>
        /// 不明なフラグ
        /// </summary>
        public bool unk_bool2
        {
            get => ((m_value.flags >> 0x01) & 1) == 1 ? true : false;
            set => m_value.flags |= (uint)(((value ? 1 : 0) & 0x01) << 0x01);
        }

        /// <summary>
        /// 重ねフラグ
        /// </summary>
        public RepeatField StackableFlag
        {
            get => (RepeatField)((m_value.flags >> 0x02) & 0x0F);
            set => m_value.flags |= (uint)(((byte)value & 0x0F) << 0x2);
        }

        /// <summary>
        /// 不明なフラグ
        /// </summary>
        public bool unk_bool3
        {
            get => ((m_value.flags >> 0x06) & 1) == 1 ? true : false;
            set => m_value.flags |= (uint)(((value ? 1 : 0) & 0x01) << 0x06);
        }

        /// <summary>
        /// 不明な値
        /// </summary>
        public byte unk_flag1
        {
            get => (byte)((m_value.flags >> 0x07) & 0x0F);
            set => m_value.flags |= (uint)((value & 0x0F) << 0x07);
        }

        /// <summary>
        /// 不明な値
        /// </summary>
        public byte unk_flag2
        {
            get => (byte)((m_value.flags >> 0x0B) & 0x1F);
            set => m_value.flags |= (uint)((value & 0x1F) << 0x0B);
        }

        /// <summary>
        /// 不明な値
        /// </summary>
        public byte unk_flag3
        {
            get => (byte)((m_value.flags >> 0x10) & 0x1F);
            set => m_value.flags |= (uint)((value & 0x1F) << 0x10);
        }

        /// <summary>
        /// 不明な値
        /// </summary>
        public byte unk_flag4
        {
            get => (byte)((m_value.flags >> 0x15) & 0x1F);
            set => m_value.flags |= (uint)((value & 0x1F) << 0x15);
        }

        /// <summary>
        /// 不明な値
        /// </summary>
        public byte unk_flag5
        {
            get => (byte)((m_value.flags >> 0x1A) & 0x0F);
            set => m_value.flags |= (uint)(value << 0x1A);
        }

        /// <summary>
        /// フラグ類
        /// </summary>
        public uint Flags
            => m_value.flags;

        /// <summary>
        /// フライウェイト
        /// </summary>
        public ItemBase Base =>
            ItemBase.AllItemBases.Length > m_value.ItemIndex ? ItemBase.AllItemBases[m_value.ItemIndex] : new ItemBase();

        /// <summary>
        /// 空フラグ
        /// </summary>
        public bool IsEmpty => m_value.IsEmpty;

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="voidItem">空アイテムフラグ</param>
        private void Init(ushort index = 0xFFFF, byte? number = null, ItemValueType itemValueType = ItemValueType.RANDOM)
        {
            m_value.ItemIndex = index;

            //uniqueID
            m_value.Values = new byte[2];
            if (!IsEmpty)
            {
                //静的乱数使わないと混ざらない
                m_value.UniqueID = (uint)Helper.StaticRandom.Next(1 << 30);

                switch (itemValueType)
                {
                    case ItemValueType.MAX:
                        m_value.Values[0] = (byte)Base.ValueScales[0].Max;
                        m_value.Values[1] = (byte)Base.ValueScales[1].Max;
                        break;
                    case ItemValueType.MIN:
                        m_value.Values[0] = (byte)Base.ValueScales[0].Min;
                        m_value.Values[1] = (byte)Base.ValueScales[1].Min;
                        break;
                    case ItemValueType.RANDOM:
                        m_value.Values[0] = (byte)Helper.StaticRandom.Next(Base.ValueScales[0].Min, Base.ValueScales[0].Max);
                        m_value.Values[1] = (byte)Helper.StaticRandom.Next(Base.ValueScales[1].Min, Base.ValueScales[1].Max);
                        break;
                }
            }

            //number
            if (!IsEmpty)
            {
                if (number.HasValue && Base.StackableNum >= number.Value)
                {
                    m_value.Number = number.Value;
                }
                else if (!number.HasValue)
                {
                    m_value.Number = (byte)Helper.StaticRandom.Next(1, Base.StackableNum);
                }
            }

            //OP初期化
            m_value.OPs = new OP[3];
            for (int i = 0; i < 3; i++)
            {
                m_value.OPs[i] = new OP();
                m_value.OPs[i].ItemOPIndex = 0xFFFF;
            }

            //フラグ初期化
            unk_flag1 = 0;
            unk_flag2 = 0;
            StackableFlag = 0;
            unk_flag3 = 0;
            unk_flag4 = 0;
            unk_flag5 = 0;
            unk_bool1 = false;
            unk_bool2 = false;
            unk_bool3 = false;
        }

        /// <summary>
        /// 実態の取得
        /// </summary>
        /// <returns></returns>
        public ItemInfo GetM_Value()
            => m_value;

        /// <summary>
        /// アイテムチェックサム
        /// </summary>
        public uint GetCheckSum()
        {
            if (IsEmpty) return 0;
            uint result = (uint)((uint)UniqueID + ItemIndex + Count + Endurance);
            foreach (var v in Values)
            {
                result += v;
            }
            foreach (var op in OPs)
            {
                result += (uint)op.ItemOPIndex + op.Value1 + op.Value2;
            }
            result += (uint)(unk_bool1 ? 1 : 0);
            result += (uint)(unk_bool2 ? 1 : 0);
            result += (uint)(unk_bool3 ? 1 : 0);
            result += (uint)StackableFlag;
            result += (uint)(unk_flag1 + unk_flag2 + unk_flag3 + unk_flag4 + unk_flag5);
            return result;
        }

        /// <summary>
        /// 色取得
        /// </summary>
        /// <returns></returns>
        public ushort GetWeaponColorizeEffect()
        {
            if (IsEmpty) return 0xFFFF;

            ushort result = ushort.MaxValue;
            List<ushort> weaponColors = new List<ushort>();
            if (Base.ColorShape != 0)
            {
                weaponColors.Add(Base.ColorShape);
            }
            weaponColors.AddRange(OPs.TakeWhile(t => !t.IsEmpty).Select(t => (ushort)t.Base.WeaponColor));

            if (weaponColors.Count == 1) return weaponColors[0];
            foreach (var color in weaponColors)
            {
                if (result == ushort.MaxValue || color % 3 > result % 3)
                {
                    result = color;
                }
            }
            return result;
        }

        public override string ToString()
            => Base.ToString();

        public int CompareTo(object obj)
        {
            switch (obj)
            {
                case Item item2 when item2.IsEmpty && IsEmpty ||
                    m_value.UniqueID != 0 && item2.m_value.UniqueID == m_value.UniqueID://UniqueIDおなじ（ゼロをのぞく）
                    return 0;
                case Item item2 when item2.IsEmpty || IsEmpty:
                    return IsEmpty ? 1 : -1;
                //UniqueIDどちらかが0
                case Item item2:
                    return (item2.m_value.ItemIndex - m_value.ItemIndex) +
                    (item2.m_value.Number - m_value.Number) +
                    (item2.m_value.Endurance - m_value.Endurance) +
                    (Enumerable.Range(0, 2).Sum(i => item2.m_value.Values[i] - m_value.Values[i])) +
                    (Enumerable.Range(0, 3).Sum(i => item2.m_value.OPs[i].CompareTo(m_value.OPs[i])));
                default:
                    throw new InvalidCastException("右辺にItemを指定してください");
            }
        }

        /// <summary>
        /// クローン
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new Item(m_value);
        }

        /// <summary>
        /// アイテム情報の構造体
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ItemInfo
        {
            /// <summary>
            /// 固有ID
            /// </summary>
            public uint UniqueID;

            /// <summary>
            /// アイテムのインデックス
            /// </summary>
            public ushort ItemIndex;

            /// <summary>
            /// 個数
            /// </summary>
            public byte Number;

            /// <summary>
            /// 耐久力
            /// </summary>
            public byte Endurance;

            /// <summary>
            /// アイテム固有の値
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] Values;

            /// <summary>
            /// OP
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public OP[] OPs;

            /// <summary>
            /// アイテムの様々なフラグ
            /// </summary>
            public uint flags;

            /// <summary>
            /// empty flag
            /// </summary>
            /// <returns></returns>
            public bool IsEmpty
                => ItemIndex == 0xFFFF;

            /// <summary>
            /// Override
            /// </summary>
            /// <returns></returns>
            public override string ToString() => ItemBase.AllItemBases[ItemIndex].Name;
        }

        /// <summary>
        /// 重ねフラグ
        /// </summary>
        [Flags]
        public enum RepeatField : byte
        {
            Broke1 = 1,
            Broke2 = 2,
            Broke3 = 4,
            Broke4 = 8,
        }

        /// <summary>
        /// アイテムのvalue選択
        /// </summary>
        public enum ItemValueType
        {
            MAX,
            MIN,
            RANDOM
        }
    }
}
