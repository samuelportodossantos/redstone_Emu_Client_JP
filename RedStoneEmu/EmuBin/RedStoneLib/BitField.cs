using RedStoneLib.Model;
using RedStoneLib.Model.Base;
using RedStoneLib.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static RedStoneLib.Model.Base.Actor;

namespace RedStoneLib
{
    /// <summary>
    /// ビットフィールドを作成
    /// </summary>
    public class BitField : Attribute
    {
        protected int Order;
        protected uint BitSize, Mask;
        protected Type Type;

        /// <summary>
        /// ビットフィールド
        /// </summary>
        /// <param name="order">順番</param>
        /// <param name="bitSize">値のサイズ（-1の場合は型のサイズと同じ）</param>
        /// <param name="mask">値に対してAnd演算する値（-1の場合はandSizeのbit長さ）</param>
        public BitField(int order, uint bitSize = 0, uint mask = 0, Type type = null)
        {
            Order = order;
            BitSize = bitSize;
            Mask = mask;
            Type = type;
        }

        /// <summary>
        /// Bitが全て立ってる値を取得
        /// </summary>
        /// <param name="bitLength"></param>
        /// <returns></returns>
        private static uint GetFullBit(uint bitLength)
        {
            uint result = 0;
            for (int i = 0; i < bitLength; i++)
            {
                result |= (uint)(1 << i);
            }
            return result;
        }

        /// <summary>
        /// class or structのBitField属性からビットフィールドを構成しbyte列で返す
        /// </summary>
        /// <typeparam name="ArgType">引数の型</typeparam>
        /// <typeparam name="InheritanceType">BitFieldに継承された型</typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] ToBytes<T, InheritanceType>(T obj, Type targetType) where InheritanceType : BitField
        {
            //4bit毎に値が格納される
            List<uint> delimitedList = new List<uint>();

            //フィールドのリスト
            List<(int order, uint size, uint and, object value)> bitObjects =
                new List<(int order, uint size, uint and, object value)>();

            // Tからフィールド全てを引き出す
            foreach (var field in targetType
                .GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                //フィールドからBitField引き出す
                var attrs = (InheritanceType[])field.GetCustomAttributes(typeof(InheritanceType), false);

                foreach (var attr in attrs)
                {
                    //メンバのタイプ
                    Type type = attr.Type ?? field.GetValue(obj).GetType();
                    if (type != typeof(string))
                    {
                        //nullの場合はタイプのサイズ
                        if (attr.BitSize == 0) attr.BitSize = 8 * (uint)Marshal.SizeOf(type);
                        //nullの場合はlastBit
                        if (attr.Mask == 0) attr.Mask = GetFullBit(attr.BitSize);
                    }

                    //追加
                    bitObjects.Add((attr.Order, attr.BitSize, attr.Mask, Convert.ChangeType(field.GetValue(obj), type)));
                }
            }

            //ソート
            bitObjects = bitObjects.OrderBy(t => t.order).ToList();

            List<byte> result = new List<byte>();
            long temporary = 0;

            //bit数のカウンタ
            int bitCntr = 0;

            //byte列生成
            foreach (var bitObj in bitObjects)
            {
                switch (bitObj.value)
                {
                    case string str:
                        if (bitCntr > 0) throw new ArgumentException("stringに入る前のサイズ合計が8bit未満です");
                        result.AddRange(Helper.StringToSjisByte(str));
                        break;
                    default:
                        temporary |= (((dynamic)bitObj.value & bitObj.and) << bitCntr);
                        bitCntr += (int)bitObj.size;

                        for (; bitCntr >= 8; bitCntr -= 8)
                        {
                            result.Add((byte)(temporary & 0xFF));
                            temporary >>= 8;
                        }
                        break;
                }
            }
            if (bitCntr > 0) throw new ArgumentException("最後が8bit未満です");
            return result.ToArray();
        }

    }


    /// <summary>
    /// SimpleActorInfo用
    /// </summary>
    public class SimpleActorInfo : BitField
    {
        /// <summary>
        /// SimpleActorInfo
        /// </summary>
        /// <param name="order">順番</param>
        /// <param name="bitSize">値のサイズ（-1の場合は型のサイズと同じ）</param>
        /// <param name="mask">値に対してAnd演算する値（-1の場合はandSizeのbit長さ）</param>
        public SimpleActorInfo(int order, uint bitSize = 0, uint mask = 0, Type type = null)
            : base(order, bitSize, mask, type) { }


        /// <summary>
        /// バイト列を生成
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] ToBytes<T>(T obj) => ToBytes<T, SimpleActorInfo>(obj, obj.GetType());
    }

    /// <summary>
    /// VerySimpleActorInfo用
    /// </summary>
    public class VerySimpleActorInfo : BitField
    {
        /// <summary>
        /// VerySimpleActorInfo
        /// </summary>
        /// <param name="order">順番</param>
        /// <param name="bitSize">値のサイズ（-1の場合は型のサイズと同じ）</param>
        /// <param name="mask">値に対してAnd演算する値（-1の場合はandSizeのbit長さ）</param>
        public VerySimpleActorInfo(int order, uint bitSize = 0, uint mask = 0, Type type = null)
            : base(order, bitSize, mask, type) { }


        /// <summary>
        /// バイト列を生成
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] ToBytes<T>(T obj) => ToBytes<T, VerySimpleActorInfo>(obj, obj.GetType());
    }


    /// <summary>
    /// MinimumActorInfo用
    /// </summary>
    public class MinimumActorInfo : BitField
    {
        /// <summary>
        /// MinimumActorInfo
        /// </summary>
        /// <param name="order">順番</param>
        /// <param name="bitSize">値のサイズ（-1の場合は型のサイズと同じ）</param>
        /// <param name="mask">値に対してAnd演算する値（-1の場合はandSizeのbit長さ）</param>
        public MinimumActorInfo(int order, uint bitSize = 0, uint mask = 0, Type type = null)
            : base(order, bitSize, mask, type) { }


        /// <summary>
        /// バイト列を生成
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] ToBytes(Actor obj, ushort questID = 0xFFFF)
        {
            var result = ToBytes<Actor, MinimumActorInfo>(obj, typeof(Actor));
            if (questID != 0xFFFF)
            {
                //flag
                result[1] |= 0x20;
                //qid
                result[2] = (byte)(questID & 0xFF);
                result[3] = (byte)(questID >> 8);
            }
            return result;
        }
    }

    public class VerySimpleMonster
    {
        [VerySimpleActorInfo(0, 0x02)]
        public byte ActorType => 2;

        /// <summary>
        /// 方向
        /// </summary>
        [VerySimpleActorInfo(1, 0x03, type: typeof(ushort))]
        public ActorDirect Direct { get; private set; }

        /// <summary>
        /// CharID
        /// </summary>
        [VerySimpleActorInfo(2, 0x0B)]
        public ushort CharID { get; private set; }

        /// <summary>
        /// BodySize
        /// </summary>
        [VerySimpleActorInfo(3)]
        public byte BodySize { get; private set; }

        /// <summary>
        /// カーソルタイプ
        /// </summary>
        [VerySimpleActorInfo(4, 0x03, mask:0x02)]
        public byte CursorType { get; private set; } = 2;

        /// <summary>
        /// ペットフラグ
        /// </summary>
        [VerySimpleActorInfo(5, 0x01, type: typeof(byte))]
        public bool IsPet { get; private set; }

        /// <summary>
        /// 隠れる
        /// </summary>
        [VerySimpleActorInfo(6, 0x01, type: typeof(byte))]
        public bool IsHide { get; private set; }

        /// <summary>
        /// 死にフラグ 以上 vs_uint32
        /// </summary>
        [VerySimpleActorInfo(7, 0x03, mask: 0x01, type: typeof(byte))]
        public bool IsDead { get; private set; }


        /// <summary>
        /// 画像のIndex
        /// </summary>
        [VerySimpleActorInfo(8, 0x0B)]
        public ushort Image { get; private set; }

        /// <summary>
        /// ActorColor NameBarBlueと0x80のみ適応 以上 vs_uint16
        /// </summary>
        [VerySimpleActorInfo(9, 0x05)]
        public ushort SmallActorColor => (ushort)((ushort)color >> 5);


        [VerySimpleActorInfo(10)]
        public ushort PosX { get; private set; }

        [VerySimpleActorInfo(11)]
        public ushort PosY { get; private set; }

        [VerySimpleActorInfo(12)]
        public uint junk0 { get; private set; }

        [VerySimpleActorInfo(13)]
        public ushort junk1 { get; private set; }
        
        /// <summary>
        /// 主にスキル用のアニメ +0x2F0
        /// </summary>
        [VerySimpleActorInfo(14, type: typeof(uint))]
        public ActorAnim1 Anim1 { get; set; }

        /// <summary>
        /// +0x2F4
        /// </summary>
        [VerySimpleActorInfo(15, type: typeof(uint))]
        public ActorAnim2 Anim2 { get; set; }

        /// <summary>
        /// +0x2F8
        /// </summary>
        [VerySimpleActorInfo(16, type: typeof(uint))]
        public ActorAnim3 Anim3 { get; set; }

        /// <summary>
        /// +0x2FC
        /// </summary>
        [VerySimpleActorInfo(17, type: typeof(uint))]
        public ActorAnim4 Anim4 { get; set; }

        /// <summary>
        /// +0x300
        /// </summary>
        [VerySimpleActorInfo(18, type: typeof(uint))]
        public ActorAnim5 Anim5 { get; set; }

        [VerySimpleActorInfo(19)]
        public uint junk2 { get; private set; }


        //NameBarBlue未満は使用不可
        public ActorColor color = ActorColor.NameBarBlue;

        public VerySimpleMonster(Monster mob)
        {
            CharID = mob.CharID;
            BodySize = (byte)mob.BodySize.Width;
            Image = mob.Image;
            PosX = mob.PosX;
            PosY = mob.PosY;
            Direct = mob.Direct;
        }
    }

    /// <summary>
    /// 主に色を表す
    /// +0x13C
    /// </summary>
    [Flags]
    public enum ActorColor : ushort
    {
        BodyNormal = 0,
        BodyBlack = 1,
        BodyWhite = 2,
        BodyTransparent = 4,

        /// <summary>
        /// default
        /// </summary>
        NameBarGreen = 0x10,
        NameBarBlue = 0x20,

        /// <summary>
        /// マリオネット（ネクロのスキル）
        /// </summary>
        Marionet = 0x40,

        /// <summary>
        /// 頭上にベル
        /// </summary>
        Bell = 0x4000,

        /// <summary>
        /// 表示されない
        /// </summary>
        None = 0x8000
    }

    /// <summary>
    /// アクターの頭上の状態
    /// +0x13D
    /// </summary>
    [Flags]
    public enum ActorHeadType : byte
    {
        Normal = 0,

        /// <summary>
        /// 頭上にベル
        /// </summary>
        Bell = 0x40,

        /// <summary>
        /// 表示されない
        /// </summary>
        None = 0x80
    }

}
