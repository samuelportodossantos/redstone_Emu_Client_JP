using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Packets
{
    public class PacketWriter : BinaryWriter
    {
        /// <summary>
        /// base stream付きのコンストラクタ
        /// </summary>
        public PacketWriter()
            : base(new MemoryStream())
        {
        }

        /// <summary>
        /// 配列に返還
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray()
        {
            var ms = (MemoryStream)BaseStream;
            return ms.ToArray();
        }

        /// <summary>
        /// 長さ
        /// </summary>
        public int Length => (int)BaseStream.Length;

        /// <summary>
        /// 単一のByteを複数書き込む
        /// </summary>
        /// <param name="b"></param>
        /// <param name="count"></param>
        internal void WriteBytes(byte b, uint count)
        {
            for (int i = 0; i < count; i++)
            {
                Write(b);
            }
        }

        /// <summary>
        /// sjisを書き込む（0埋め付き）
        /// </summary>
        /// <param name="str"></param>
        /// <param name="maxCount"></param>
        public void WriteSjis(string str, uint maxCount)
        {
            var bs = Helper.StringToSjisByte(str);
            for (var i = 0; i < bs.Length; i++)
                Write(bs[i]);

            if (maxCount < bs.Length) maxCount = (uint)(bs.Length + 1);
            uint nopCount = (uint)(maxCount - bs.Length);
            WriteBytes(0x00, nopCount);
        }

        /// <summary>
        /// sjisを書き込む
        /// </summary>
        /// <param name="str"></param>
        public void WriteSjis(string str)
        {
            var bs = Helper.StringToSjisByte(str);
            WriteSjis(str, (uint)(bs.Length));
        }

        /// <summary>
        /// 構造体を書き込む
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        public void WriteStruct<T>(T obj)
        {
            Write(Helper.StructToBytes<T>(obj));
        }
    }
}
