using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Packets.RSPacket.Login
{
    /// <summary>
    /// ログイン結果
    /// </summary>
    public class ResultLoginPacket : Packet
    {
        private LoginResult Result;
        private uint Pass_auth_code;
        private ushort Packet_security_code;
        private string reason = "";

        public enum LoginResult : uint
        {
            LoginSuccess = 0,//	ログイン成功
            LoginFailures = 1,//	ログイン失敗
            AlreadyConnected = 2,//	既に接続しているID
            Disappear = 3,//	一瞬で消える
            GameVersionDifferent = 4,//	ゲームのバージョンが違います
            OneTimeKey = 5,//	ワンタイムキー
            LoginSuccess_require_onetime = 6,//	ワインタイムキーの仕様を推奨
            LoginSuccess2 = 7,//	ログイン成功2
            PC_authentication = 8,//	PC認証
            BAN = 9,//	BAN
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="result"></param>
        /// <param name="pass_auth_code"></param>
        /// <param name="packet_security_code"></param>
        public ResultLoginPacket(LoginResult result, uint pass_auth_code, ushort packet_security_code)
        {
            Result = result;
            Pass_auth_code = pass_auth_code;
            Packet_security_code = packet_security_code;
        }

        /// <summary>
        /// BANの場合
        /// </summary>
        /// <param name="time">非負の数字で時間指定</param>
        /// <param name="pass_auth_code"></param>
        /// <param name="packet_security_code"></param>
        /// <param name="str"></param>
        public ResultLoginPacket(uint time, uint pass_auth_code, ushort packet_security_code, string str)
        {
            Result = (LoginResult)time;
            Pass_auth_code = pass_auth_code;
            Packet_security_code = packet_security_code;
            reason = str;
        }

        public override byte[] Build()
        {

            var writer = new PacketWriter();

            writer.Write((uint)Result);//Network event
            writer.Write(Pass_auth_code);//password auth code
            writer.Write(Packet_security_code);
            writer.WriteSjis(reason);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1101);
        }
    }
}
