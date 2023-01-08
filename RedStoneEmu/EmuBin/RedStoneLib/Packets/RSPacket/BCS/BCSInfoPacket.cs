using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Packets.RSPacket.BCS
{
    /// <summary>
    /// ブロードキャストサーバーのIPアドレスを送信
    /// </summary>
    public class BCSInfoPacket : Packet
    {
        string IpAddr;
        ushort Port;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="ipAddr"></param>
        /// <param name="port"></param>
        public BCSInfoPacket(string ipAddr, ushort port)
        {
            IpAddr = ipAddr;
            Port = port;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write(Port);
            writer.WriteSjis(IpAddr, 15);
            writer.Write((byte)0);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1270);
        }
    }
}
