using RedStoneLib.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Packets.RSPacket.GameLogin
{
    /// <summary>
    /// ゲーム接続結果
    /// GameLoginPacket: 1st
    /// </summary>
    public class ResultGameConnectPacket : Packet
    {
        /// <summary>
        /// GameConnect結果
        /// </summary>
        public enum GameConnectResult : ushort
        {
            /// <summary>
            /// 成功
            /// </summary>
            Success = 0,

            /// <summary>
            /// ゲームサーバーと接続できません
            /// </summary>
            ConnectionFailed = 1,

            /// <summary>
            /// 接続人数を超えました
            /// </summary>
            OverConnection = 2
        }

        ushort MapSerial;

        GameConnectResult Result;

        /// <summary>
        /// 成功用
        /// </summary>
        /// <param name="result"></param>
        /// <param name="player"></param>
        public ResultGameConnectPacket(ushort mapSerial)
        {
            Result = GameConnectResult.Success;
            MapSerial = mapSerial;
        }

        /// <summary>
        /// 失敗用
        /// </summary>
        /// <param name="result"></param>
        public ResultGameConnectPacket(GameConnectResult result)
        {
            if (result == GameConnectResult.Success) throw new ArgumentException("成功の場合は成功用コンストラクタを使用して下さい．");
            Result = result;
        }

        /// <summary>
        /// 現在時刻を得る
        /// </summary>
        /// <returns></returns>
        private static uint GetDatatimeNowUint32()
        {
            var now = DateTime.Now;
            byte year = (byte)((now.Year - 2000) & 0x3F);
            byte month = (byte)(now.Month & 0x0F);
            byte day = (byte)(now.Day & 0x1F);
            byte hour = (byte)(now.Hour & 0x1F);
            byte minute = (byte)(now.Minute & 0x3F);
            byte second = (byte)(now.Second & 0x3F);
            return (uint)(year | (month << 0x06) | (day << 0x0A) | (hour << 0x0F) | (minute << 0x14) | (second << 0x1A));
        }

        public override byte[] Build()
        {

            var writer = new PacketWriter();

            //接続結果
            writer.Write((ushort)Result);
            if (Result != GameConnectResult.Success)
            {
                //成功以外はここで終わり
                packetSize = (ushort)writer.ToArray().Count();
                return writer.ToArray();
            }

            writer.Write(GetDatatimeNowUint32());//server time
            writer.Write((ushort)MapSerial);//Field serial
            writer.Write((ushort)0);//??
            writer.Write((ushort)0);//??
            writer.Write((ushort)0xCCCC);//no define
            writer.Write((uint)PublicHelper.SeasonVariable);//season variable
            writer.WriteSjis(Map.AllMapInfos[MapSerial].fileName, 0x40);
            writer.WriteBytes(0, 0x17);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1120);
        }
    }
}
