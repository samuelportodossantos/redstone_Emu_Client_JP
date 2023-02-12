using RedStoneLib.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Packets
{
    /// <summary>
    /// 送信パケットの抽象体
    /// </summary>
    public abstract class Packet
    {
        public abstract byte[] Build();
        public abstract PacketHeader GetHeader();

        /// <summary>
        /// パケットサイズ
        /// </summary>
        public ushort packetSize = 0;
    }

    /// <summary>
    /// 送信パケットヘッダの構造体
    /// </summary>
    public struct PacketHeader
    {
        public UInt16 Size;
        public UInt32 Type;

        public PacketHeader(UInt16 size, UInt32 type)
        {
            this.Size = size;
            this.Type = type;
        }
    }
}
