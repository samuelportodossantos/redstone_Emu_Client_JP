using RedStoneLib.Model.Base;
using RedStoneLib.Model.Effect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Model
{
    /// <summary>
    /// アイテムのオプション
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct OP : IComparable
    {
        /// <summary>
        /// Indexの指定
        /// </summary>
        /// <param name="itemOPIndex">OPのIndex</param>
        public OP(ushort itemOPIndex)
        {
            ItemOPIndex = itemOPIndex;
            Value1 = 0;
            Value2 = 0;
        }

        /// <summary>
        /// IndexとValueの指定
        /// </summary>
        /// <param name="itemOPIndex"></param>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        public OP(ushort itemOPIndex, byte value1, byte value2)
        {
            ItemOPIndex = itemOPIndex;
            Value1 = value1;
            Value2 = value2;
        }

        /// <summary>
        /// オプションのインデックス
        /// </summary>
        public ushort ItemOPIndex;

        /// <summary>
        /// オプションの値１
        /// </summary>
        public byte Value1;

        /// <summary>
        /// オプションの値２
        /// </summary>
        public byte Value2;

        /// <summary>
        /// 空フラグ
        /// </summary>
        public bool IsEmpty 
            => ItemOPIndex == 0xFFFF;

        /// <summary>
        /// フライウェイト
        /// </summary>
        public OPBase Base => OPBase.AllOPBases.Length > ItemOPIndex ? OPBase.AllOPBases[ItemOPIndex] : new OPBase();

        /// <summary>
        /// OPの効果を取得
        /// </summary>
        /// <param name="level"></param>
        /// <param name="job"></param>
        /// <returns></returns>
        public (PlayerEffect.OPEffect effect, ushort[] v) Effect
            => (Base.Effect, new ushort[] { Value1, Value2 });

        public override string ToString()
            => Base.ToString();

        public int CompareTo(object obj)
        {
            switch (obj)
            {
                case OP op when op.IsEmpty && IsEmpty:
                    return 0;
                case OP op when op.IsEmpty || IsEmpty:
                    return IsEmpty ? 1 : -1;
                case OP op when op.ItemOPIndex == ItemOPIndex:
                    return (op.Value1 - Value1) + (op.Value2 - Value2);
                case OP op:
                    return op.ItemOPIndex - ItemOPIndex;
                default:
                    throw new InvalidCastException();
            }
        }
    }
}
