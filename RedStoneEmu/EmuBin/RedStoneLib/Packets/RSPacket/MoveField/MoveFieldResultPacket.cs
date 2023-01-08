using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Packets.RSPacket.MoveField
{
    /// <summary>
    /// フィールド移動結果
    /// </summary>
    public class MoveFieldResultPacket : Packet
    {
        public enum MoveFieldResult : ushort
        {
            /// <summary>
            /// OK
            /// </summary>
            Success = 1,

            /// <summary>
            /// この位置では移動ができません
            /// </summary>
            CantMoveInNowPos = 2,

            /// <summary>
            /// 人数多すぎ
            /// </summary>
            TooMuchPlayers = 3,

            /// <summary>
            /// 同じ名前のプレイヤーが存在
            /// </summary>
            SamePlayerNameExist = 4,

            /// <summary>
            /// スフィアが必要
            /// </summary>
            NeedPremium = 6,

            /// <summary>
            /// 一方通行
            /// </summary>
            OneWay = 7,

            /// <summary>
            ///テストサーバーでテスト中のフィールド
            /// </summary>
            TestField = 8,
        }

        MoveFieldResult Result;

        /// <summary>
        /// 移動先ファイル名
        /// </summary>
        string FileName;

        /// <summary>
        /// 成功
        /// </summary>
        /// <param name="fileName"></param>
        public MoveFieldResultPacket(string fileName)
        {
            FileName = fileName;
            Result = MoveFieldResult.Success;
        }

        /// <summary>
        /// 失敗
        /// </summary>
        /// <param name="result"></param>
        public MoveFieldResultPacket(MoveFieldResult result)
        {
            this.Result = result;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write((ushort)Result);//結果
            if (Result != MoveFieldResult.Success)
            {
                packetSize = (ushort)writer.ToArray().Count();
                return writer.ToArray();
            }
            writer.WriteSjis("127.0.0.1", 0x10);//接続先ＩＰ
            writer.WriteSjis(FileName, 0x40);//マップ名
            writer.Write((ushort)0xFFFF);//??
            writer.Write((ushort)0xa);//Field serial
            writer.Write((ushort)0x0);//GH flag

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1155);
        }
    }
}
