using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Packets.RSPacket.Login
{
    /// <summary>
    /// キャラ選択結果
    /// </summary>
    public class ResultSelectAvatarPacket : Packet
    {
        public enum SelectAvatarResult : ushort
        {
            /// <summary>
            /// 成功
            /// </summary>
            Success = 0,

            /// <summary>
            /// ゲーム参加に失敗しました．
            /// </summary>
            Failed = 1,

            /// <summary>
            /// 接続が完全に切れていません．もう一度お試し下さい．
            /// </summary>
            NotDisconnected = 2
        }

        private SelectAvatarResult Result;
        private ushort packet_security_code;

        public ResultSelectAvatarPacket(SelectAvatarResult result, ushort packet_security_code)
        {
            Result = result;
            this.packet_security_code = packet_security_code;
        }

        public override byte[] Build()
        {

            var writer = new PacketWriter();

            writer.Write((ushort)0x00);
            writer.Write(packet_security_code);
            writer.Write((ushort)Result);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1109);
        }
    }
}
