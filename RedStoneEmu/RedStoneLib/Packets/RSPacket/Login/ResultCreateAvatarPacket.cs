using RedStoneLib.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Packets.RSPacket.Login
{
    /// <summary>
    /// キャラ作成結果
    /// </summary>
    public class ResultCreateAvatarPacket : Packet
    {
        public enum CreateAvatarResult : ushort
        {
            /// <summary>
            /// 成功
            /// </summary>
            Success = 0,

            /// <summary>
            /// 原因不明の失敗
            /// </summary>
            UnknownFailed1 = 1,

            /// <summary>
            /// 同じ名前のキャラクター
            /// </summary>
            NameExist = 2,

            /// <summary>
            /// これ以上キャラクター作成は作成できない
            /// </summary>
            NoMoreCreate = 3,

            /// <summary>
            /// 原因不明の失敗
            /// </summary>
            UnknownFailed2 = 4
        }

        private CreateAvatarResult Result;
        private ushort Packet_security_code;

        /// <summary>
        /// アバターのインデックス
        /// </summary>
        private ushort AvatarIndex = 0xFFFF;

        /// <summary>
        /// 新しいアバター
        /// </summary>
        private Player NewPlayer;

        /// <summary>
        /// サーバーIP
        /// </summary>
        private string ServerIP;

        /// <summary>
        /// 成功用
        /// </summary>
        /// <param name="newPlayer"></param>
        /// <param name="avatarIndex"></param>
        /// <param name="serverIP"></param>
        /// <param name="packet_security_code"></param>
        public ResultCreateAvatarPacket(Player newPlayer, ushort avatarIndex, string serverIP, ushort packet_security_code)
        {
            Result = CreateAvatarResult.Success;
            NewPlayer = newPlayer;
            AvatarIndex = avatarIndex;
            ServerIP = serverIP;
            Packet_security_code = packet_security_code;
        }

        /// <summary>
        /// 失敗用
        /// </summary>
        /// <param name="result"></param>
        /// <param name="packet_security_code"></param>
        public ResultCreateAvatarPacket(CreateAvatarResult result, ushort packet_security_code)
        {
            Result = result;
            Packet_security_code = packet_security_code;
        }

        public override byte[] Build()
        {

            var writer = new PacketWriter();

            writer.Write((ushort)Result);//Network event

            //アバターのインデックス
            writer.Write(AvatarIndex);

            writer.Write(Packet_security_code);

            if (Result != CreateAvatarResult.Success)
            {
                packetSize = (ushort)writer.ToArray().Count();
                return writer.ToArray();
            }

            writer.Write(AvatarIndex);

            //アバター書き込み
            writer.Write(NewPlayer.WriteAvatar(ServerIP));
            
            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1104);
        }
    }
}
