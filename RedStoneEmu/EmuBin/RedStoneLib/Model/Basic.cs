using RedStoneLib.Packets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Model
{
    //Model名前空間で普遍的に使われる構造体など

    /// <summary>
    /// 魔法属性
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Magic : IFormattable
    {
        public short this[MagicType index]
        {
            get
            {
                switch (index)
                {
                    case MagicType.Fire:
                        return Fire;
                    case MagicType.Water:
                        return Water;
                    case MagicType.Wind:
                        return Wind;
                    case MagicType.Earth:
                        return Earth;
                    case MagicType.Light:
                        return Light;
                    case MagicType.Dark:
                        return Dark;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// 火
        /// </summary>
        public short Fire;

        /// <summary>
        /// 水
        /// </summary>
        public short Water;

        /// <summary>
        /// 風
        /// </summary>
        public short Wind;

        /// <summary>
        /// 土
        /// </summary>
        public short Earth;

        /// <summary>
        /// 光
        /// </summary>
        public short Light;

        /// <summary>
        /// 闇
        /// </summary>
        public short Dark;

        public int Sum
            => Fire + Water + Wind + Earth + Light + Dark;
        
        public override string ToString()
            => $"Fire:{Fire}, Water:{Water}, Wind:{Wind}, Earth:{Earth}, Light:{Light}, Darj:{Dark}";

        public string ToString(string format, IFormatProvider formatProvider)
                => ToString();

        public static Magic operator +(Magic a, double b)
        {
            return new Magic
            {
                Fire = (short)(a.Fire + b),
                Water = (short)(a.Water + b),
                Wind = (short)(a.Wind + b),
                Earth = (short)(a.Earth + b),
                Light = (short)(a.Light + b),
                Dark = (short)(a.Dark + b),
            };
        }

        public static Magic operator +(Magic a, Magic b)
        {
            return new Magic
            {
                Fire = (short)(a.Fire + b.Fire),
                Water = (short)(a.Water + b.Water),
                Wind = (short)(a.Wind + b.Wind),
                Earth = (short)(a.Earth + b.Earth),
                Light = (short)(a.Light + b.Light),
                Dark = (short)(a.Dark + b.Dark),
            };
        }

        public static Magic operator -(Magic a, Magic b)
        {
            return new Magic
            {
                Fire = (short)(a.Fire - b.Fire),
                Water = (short)(a.Water - b.Water),
                Wind = (short)(a.Wind - b.Wind),
                Earth = (short)(a.Earth - b.Earth),
                Light = (short)(a.Light - b.Light),
                Dark = (short)(a.Dark - b.Dark),
            };
        }
    }

    /// <summary>
    /// 魔法属性（ジェネリックタイプ）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct Magic<T> : IFormattable
        where T : IConvertible
    {
        public T this[MagicType index]
        {
            get
            {
                switch (index)
                {
                    case MagicType.Fire:
                        return Fire;
                    case MagicType.Water:
                        return Water;
                    case MagicType.Wind:
                        return Wind;
                    case MagicType.Earth:
                        return Earth;
                    case MagicType.Light:
                        return Light;
                    case MagicType.Dark:
                        return Dark;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }
        
        /// <summary>
        /// 火
        /// </summary>
        public T Fire;

        /// <summary>
        /// 水
        /// </summary>
        public T Water;

        /// <summary>
        /// 風
        /// </summary>
        public T Wind;

        /// <summary>
        /// 土
        /// </summary>
        public T Earth;

        /// <summary>
        /// 光
        /// </summary>
        public T Light;

        /// <summary>
        /// 闇
        /// </summary>
        public T Dark;

        /// <summary>
        /// 各属性の合計値
        /// </summary>
        public int Sum
            => Fire.ToInt32(null) + Water.ToInt32(null) + Wind.ToInt32(null)
            + Earth.ToInt32(null) + Light.ToInt32(null) + Dark.ToInt32(null);

        /// <summary>
        /// 各属性の合計値
        /// </summary>
        public double SumDouble
            => Fire.ToDouble(null) + Water.ToDouble(null) + Wind.ToDouble(null)
            + Earth.ToDouble(null) + Light.ToDouble(null) + Dark.ToDouble(null);

        public static explicit operator Magic(Magic<T> a)
            => new Magic
            {
                Fire = a.Fire.ToInt16(null),
                Water = a.Water.ToInt16(null),
                Wind = a.Wind.ToInt16(null),
                Earth = a.Earth.ToInt16(null),
                Light = a.Light.ToInt16(null),
                Dark = a.Dark.ToInt16(null),
            };

        public override string ToString()
            => $"Fire:{Fire}, Water:{Water}, Wind:{Wind}, Earth:{Earth}, Light:{Light}, Darj:{Dark}";

        public string ToString(string format, IFormatProvider formatProvider)
                => ToString();
    }
    
    /// <summary>
    /// 魔法属性のEnum
    /// </summary>
    public enum MagicType : int
    {
        /// <summary>
        /// 火
        /// </summary>
        Fire = 0,

        /// <summary>
        /// 水
        /// </summary>
        Water = 1,

        /// <summary>
        /// 風
        /// </summary>
        Wind = 2,

        /// <summary>
        /// 土
        /// </summary>
        Earth = 3,

        /// <summary>
        /// 光
        /// </summary>
        Light = 4,

        /// <summary>
        /// 闇
        /// </summary>
        Dark = 5,
    }

    /// <summary>
    /// ステータス
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ActorStatus : IFormattable
    {
        public ushort this[ActorStatusType index]
        {
            get
            {
                switch (index)
                {
                    case ActorStatusType.Strength:
                        return Strength;
                    case ActorStatusType.Agility:
                        return Agility;
                    case ActorStatusType.Condition:
                        return Condition;
                    case ActorStatusType.Wisdom:
                        return Wisdom;
                    case ActorStatusType.Inteligence:
                        return Inteligence;
                    case ActorStatusType.Charisma:
                        return Charisma;
                    case ActorStatusType.Luckey:
                        return Luckey;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public ActorStatus Set(ActorStatusType index, ushort value)
        {
            switch (index)
            {
                case ActorStatusType.Strength:
                    Strength = value;
                    break;
                case ActorStatusType.Agility:
                    Agility = value;
                    break;
                case ActorStatusType.Condition:
                    Condition = value;
                    break;
                case ActorStatusType.Wisdom:
                    Wisdom = value;
                    break;
                case ActorStatusType.Inteligence:
                    Inteligence = value;
                    break;
                case ActorStatusType.Charisma:
                    Charisma = value;
                    break;
                case ActorStatusType.Luckey:
                    Luckey = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return this;
        }

        /// <summary>
        /// 全代入
        /// </summary>
        /// <param name="value"></param>
        public void SetAll(ushort value)
        {
            Strength = value;
            Agility = value;
            Condition = value;
            Wisdom = value;
            Inteligence = value;
            Charisma = value;
            Luckey = value;
        }

        /// <summary>
        /// 力
        /// </summary>
        public ushort Strength;

        /// <summary>
        /// 敏捷
        /// </summary>
        public ushort Agility;

        /// <summary>
        /// 健康
        /// </summary>
        public ushort Condition;

        /// <summary>
        /// 知恵
        /// </summary>
        public ushort Wisdom;

        /// <summary>
        /// 知識
        /// </summary>
        public ushort Inteligence;

        /// <summary>
        /// カリスマ
        /// </summary>
        public ushort Charisma;

        /// <summary>
        /// 運
        /// </summary>
        public ushort Luckey;

        /// <summary>
        /// 配列に変換
        /// </summary>
        /// <returns></returns>
        public double[] ToArray()
            => new double[] {Strength, Agility, Condition, Wisdom, Inteligence, Charisma, Luckey };

        /// <summary>
        /// 知恵から魔法抵抗を得る
        /// </summary>
        /// <returns></returns>
        public double GetResistanceByWisdom()
            => Wisdom / 20.0;

        /// <summary>
        /// 知識ボーナスを得る
        /// </summary>
        /// <returns></returns>
        public double GetKnowledgeBonus()
        {
            switch (Inteligence)
            {
                case ushort n when n < 100:
                    return 1.0;
                case ushort n when n < 132:
                    return 1.02;
                case ushort n when n < 174:
                    return 1.04;
                case ushort n when n < 230:
                    return 1.06;
                case ushort n when n < 304:
                    return 1.08;
                case ushort n when n < 402:
                    return 1.1;
                case ushort n when n < 531:
                    return 1.12;
                case ushort n when n < 702:
                    return 1.14;
                case ushort n when n < 928:
                    return 1.16;
                case ushort n when n < 1227:
                    return 1.18;
                case ushort n when n < 4727:
                    return Math.Ceiling((n - 1227) / 350.0) * 0.02 + 1.2;
                case ushort n:
                    return Math.Ceiling((n - 4727) / 400.0) * 0.02 + 1.4;
            }
        }

        public override string ToString()
            => $"STR:{Strength}, AGI:{Agility}, CON:{Condition}, WIS:{Wisdom}, INT:{Inteligence}, CHA:{Charisma}, LUC:{Luckey}";

        /// <summary>
        /// ２つの間の操作
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static ActorStatus Operation(ActorStatus s1, ActorStatus s2, Func<ushort, ushort, ushort> f)
        {
            return new ActorStatus
            {
                Strength = f(s1.Strength, s2.Strength),
                Inteligence = f(s1.Inteligence, s2.Inteligence),
                Agility = f(s1.Agility, s2.Agility),
                Wisdom = f(s1.Wisdom, s2.Wisdom),
                Condition = f(s1.Condition, s2.Condition),
                Charisma = f(s1.Charisma, s2.Charisma),
                Luckey = f(s1.Luckey, s2.Luckey),
            };
        }

        public string ToString(string format, IFormatProvider formatProvider)
                => ToString();

        public static ActorStatus operator *(ActorStatus s, int i)
        {
            return new ActorStatus
            {
                Strength = (ushort)(s.Strength * i),
                Inteligence = (ushort)(s.Inteligence * i),
                Agility = (ushort)(s.Agility * i),
                Wisdom = (ushort)(s.Wisdom * i),
                Condition = (ushort)(s.Condition * i),
                Charisma = (ushort)(s.Charisma * i),
                Luckey = (ushort)(s.Luckey * i)
            };
        }

        public static ActorDoubleStatus operator /(ActorStatus s, double d)
        {
            return new ActorDoubleStatus
            {
                Strength = s.Strength / d,
                Inteligence = s.Inteligence / d,
                Agility = s.Agility / d,
                Wisdom = s.Wisdom / d,
                Condition = s.Condition / d,
                Charisma = s.Charisma / d,
                Luckey = s.Luckey / d
            };
        }

        /// <summary>
        /// 加算
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        public static ActorStatus operator +(ActorStatus s1, ActorStatus s2)
            => Operation(s1, s2, (t, u) => (ushort)(t + u));

        /// <summary>
        /// 精度が高いステータス
        /// </summary>
        public struct ActorDoubleStatus
        {
            public double Strength;
            public double Agility;
            public double Condition;
            public double Wisdom;
            public double Inteligence;
            public double Charisma;
            public double Luckey;

            public static ActorStatus operator *(ActorDoubleStatus s, int i)
            {
                return new ActorStatus
                {
                    Strength = (ushort)Math.Floor(s.Strength * i),
                    Inteligence = (ushort)Math.Floor(s.Inteligence * i),
                    Agility = (ushort)Math.Floor(s.Agility * i),
                    Wisdom = (ushort)Math.Floor(s.Wisdom * i),
                    Condition = (ushort)Math.Floor(s.Condition * i),
                    Charisma = (ushort)Math.Floor(s.Charisma * i),
                    Luckey = (ushort)Math.Floor(s.Luckey * i)
                };
            }
        }
    }

    /// <summary>
    /// ステータス（ジェネリック）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct ActorStatus<T> : IFormattable 
        where T: IFormattable, IConvertible
    {
        public T this[ActorStatusType index]
        {
            get
            {
                switch (index)
                {
                    case ActorStatusType.Strength:
                        return Strength;
                    case ActorStatusType.Agility:
                        return Agility;
                    case ActorStatusType.Condition:
                        return Condition;
                    case ActorStatusType.Wisdom:
                        return Wisdom;
                    case ActorStatusType.Inteligence:
                        return Inteligence;
                    case ActorStatusType.Charisma:
                        return Charisma;
                    case ActorStatusType.Luckey:
                        return Luckey;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// 力
        /// </summary>
        public T Strength;

        /// <summary>
        /// 敏捷
        /// </summary>
        public T Agility;

        /// <summary>
        /// 健康
        /// </summary>
        public T Condition;

        /// <summary>
        /// 知恵
        /// </summary>
        public T Wisdom;

        /// <summary>
        /// 知識
        /// </summary>
        public T Inteligence;

        /// <summary>
        /// カリスマ
        /// </summary>
        public T Charisma;

        /// <summary>
        /// 運
        /// </summary>
        public T Luckey;

        public override string ToString()
            => $"STR:{Strength}, AGI:{Agility}, CON:{Condition}, WIS:{Wisdom}, INT:{Inteligence}, CHA:{Charisma}, LUC:{Luckey}";

        public string ToString(string format, IFormatProvider formatProvider)
                => ToString();

        public static explicit operator ActorStatus (ActorStatus<T> a)
        {
            return new ActorStatus
            {
                Strength = a.Strength.ToUInt16(null),
                Inteligence = a.Inteligence.ToUInt16(null),
                Agility = a.Agility.ToUInt16(null),
                Wisdom = a.Wisdom.ToUInt16(null),
                Condition = a.Condition.ToUInt16(null),
                Charisma = a.Charisma.ToUInt16(null),
                Luckey = a.Luckey.ToUInt16(null),
            };
        }
    }

    /// <summary>
    /// ステータスのEnum
    /// </summary>
    public enum ActorStatusType : int
    {
        Strength,
        Agility,
        Condition,
        Wisdom,
        Inteligence,
        Charisma,
        Luckey,
    }

    /// <summary>
    /// キャラクターの向き
    /// </summary>
    [Flags]
    public enum ActorDirect : ushort
    {
        Up = 0,
        UpRight = 1,
        Right = 2,
        DownRight = 3,
        Down = 4,
        DownLeft = 5,
        Left = 6,
        UpLeft = 7,

        /// <summary>
        /// リスポーンフラグ
        /// </summary>
        Spawn = 8
    }

    /// <summary>
    /// サイズ（W*H）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Size<T>
        where T:IFormattable, IConvertible
    {
        public T Width;
        public T Height;
        
        public Size(T w, T h)
        {
            Width = w;
            Height = h;
        }

    public override string ToString() => $"Width={Width}, Height={Height}";
    }

    /// <summary>
    /// 最大・最小値のスケール
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Scale<T> : IConvertible
        where T : IConvertible, IComparable
    {
        public T Max;
        public T Min;

        public Scale(T min, T max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// 値を持っている
        /// </summary>
        public bool HasValue
            => Max.CompareTo(default(T)) != 0 || Min.CompareTo(default(T)) != 0;

        /// <summary>
        /// 範囲の限定
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public T Clip(T value)
        {
            if (value.CompareTo(Max) > 0) return Max;
            if (value.CompareTo(Min) < 0) return Min;
            return value;
        }

        /// <summary>
        /// 最小値を代入
        /// </summary>
        /// <param name="value"></param>
        public void SetMin(T value)
            => Min = value;

        /// <summary>
        /// 最大値を代入
        /// </summary>
        /// <param name="value"></param>
        public void SetMax(T value)
            => Max = value;

        /// <summary>
        /// 最大最小値の範囲でランダム
        /// </summary>
        public T RandomValue
        {
            get
            {
                if (Min is double)
                {
                    //double
                    return Helper.StaticRandom.NextDouble() * (double)((dynamic)Max - Min) + (dynamic)Min;
                }
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter == null) throw new InvalidCastException("ジェネリックの型のコンバーターが取得できませんでした．");

                var randomVal = Helper.StaticRandom.Next((int)Convert.ChangeType(Min, typeof(int)), (int)Convert.ChangeType(Max, typeof(int)));
                return (T)converter.ConvertTo(randomVal, typeof(T));
            }
        }

        /// <summary>
        /// MinとMaxを逆にする
        /// </summary>
        public void ReverseMinMax()
        {
            T tmp = Min;
            Min = Max;
            Max = tmp;
        }

        /// <summary>
        /// doubleとの和
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Scale<T> operator +(T b, Scale<T> a)
            => new Scale<T>()
            {
                Min = (T)((dynamic)a.Min + b),
                Max = (T)((dynamic)a.Max + b)
            };
        public static Scale<T> operator +(Scale<T> a, T b)
            => b + a;

        /// <summary>
        /// doubleとの積
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Scale<double> operator *(Scale<T> a, double b)
            => new Scale<double>()
            {
                Min = (dynamic)a.Min * b,
                Max = (dynamic)a.Max * b
            };

        /// <summary>
        /// doubleとの商
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Scale<double> operator /(Scale<T> a, double b)
            => new Scale<double>()
            {
                Min = (dynamic)a.Min / b,
                Max = (dynamic)a.Max / b
            };

        /// <summary>
        /// 要素同士の和
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Scale<T> operator +(Scale<T> a, Scale<T> b)
            => new Scale<T>()
            {
                Min = (T)((dynamic)a.Min + b.Min),
                Max = (T)((dynamic)a.Max + b.Max),
            };

        /// <summary>
        /// 要素同士の差
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Scale<T> operator -(Scale<T> a, Scale<T> b)
            => new Scale<T>()
            {
                Min = (T)((dynamic)a.Min - b.Min),
                Max = (T)((dynamic)a.Max - b.Max),
            };

        public static Scale<T> operator +(Scale<T> a, Scale<int> b)
            => new Scale<T>()
            {
                Min = (T)((dynamic)a.Min + b.Min),
                Max = (T)((dynamic)a.Max + b.Max),
            };

        public override string ToString() => $"Min={Min}, Max={Max}";

        public TypeCode GetTypeCode()
            => Min.GetTypeCode();

        public bool ToBoolean(IFormatProvider provider) => Min.ToBoolean(provider);

        public char ToChar(IFormatProvider provider) => Min.ToChar(provider);

        public sbyte ToSByte(IFormatProvider provider) => Min.ToSByte(provider);

        public byte ToByte(IFormatProvider provider) => Min.ToByte(provider);

        public short ToInt16(IFormatProvider provider) => Min.ToInt16(provider);

        public ushort ToUInt16(IFormatProvider provider) => Min.ToUInt16(provider);

        public int ToInt32(IFormatProvider provider) => Min.ToInt32(provider);

        public uint ToUInt32(IFormatProvider provider) => Min.ToUInt32(provider);

        public long ToInt64(IFormatProvider provider) => Min.ToInt64(provider);

        public ulong ToUInt64(IFormatProvider provider) => Min.ToUInt64(provider);

        public float ToSingle(IFormatProvider provider) => Min.ToSingle(provider);

        public double ToDouble(IFormatProvider provider) => Min.ToDouble(provider);

        public decimal ToDecimal(IFormatProvider provider) => Min.ToDecimal(provider);

        public DateTime ToDateTime(IFormatProvider provider) => Min.ToDateTime(provider);

        public string ToString(IFormatProvider provider) => Min.ToString(provider);

        public object ToType(Type conversionType, IFormatProvider provider) => Min.ToType(conversionType, provider);
    }

    /// <summary>
    /// SendPacketのデリゲート
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="flush"></param>
    public delegate void SendPacketDelegate(Packet packet, bool flush = false);

    /// <summary>
    /// 座標
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Point<T>
        where T: IConvertible, IFormattable
    {
        public T X;
        public T Y;

        /// <summary>
        /// 加法
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Point<uint> operator +(Point<T> a, Point<uint> b) =>new Point<uint>() { X = a.X.ToUInt32(null) + b.X, Y = a.Y.ToUInt32(null) + b.Y };

        public static Point<uint> operator /(Point<T> a, double b) => new Point<uint>() {X= (uint)(a.X.ToUInt32(null) / b), Y= (uint)(a.Y.ToUInt32(null) / b) };

        public override string ToString() => string.Format("({0}.{1})", X, Y);
    }
}
