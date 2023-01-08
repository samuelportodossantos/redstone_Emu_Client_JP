using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Packets.RSPacket.Login
{
    /// <summary>
    /// キャラデリ結果
    /// </summary>
    public class ResultRemoveAvatarPacket : Packet
    {
        public enum RemoveAvatar : ushort
        {
            /// <summary>
            /// 一瞬で削除
            /// </summary>
            Success = 0,

            /// <summary>
            /// キャラクターの削除に失敗しました
            /// </summary>
            Failed = 1
        }

        private RemoveAvatar Result;
        private ushort packet_security_code;

        public ResultRemoveAvatarPacket(RemoveAvatar result, ushort packet_security_code)
        {
            Result = result;
            this.packet_security_code = packet_security_code;
        }
        public override byte[] Build()
        {

            var writer = new PacketWriter();

            writer.Write((ushort)Result);//Network event
            writer.Write(packet_security_code);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1105);
        }
    }
}
