using RedStoneLib.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Packets.RSPacket.GameLogin
{
    /// <summary>
    /// ゲーム（マップ）参加結果
    /// </summary>
    public class ResultJoinGamePacket : Packet
    {
        /// <summary>
        /// JoinGame結果
        /// </summary>
        public enum JoinGameResult : ushort
        {
            /// <summary>
            /// 成功
            /// </summary>
            Success = 0,

            /// <summary>
            /// 接続希望のフィールドが一杯です．しばらく後でまた接続してください.
            /// </summary>
            FullField = 1,

            /// <summary>
            /// ゲーム参加に失敗しました．
            /// </summary>
            JoinGameFailed1 = 2,

            /// <summary>
            /// ゲーム参加に失敗しました．
            /// </summary>
            JoinGameFailed2 = 3,

            /// <summary>
            /// ゲーム参加に失敗しました．接続が完全に切られていません．
            /// </summary>
            NotYetDisconnected = 4,

            /// <summary>
            /// 最後に接続した秘密ダンジョンが消えたか，パーティーから外されました．
            /// </summary>
            LostSecretDungeon = 5,

            /// <summary>
            /// 接続中に問題が発生しました．
            /// </summary>
            ProblemOccured = 6,

            /// <summary>
            /// とても多くのユーザが接続を試みています．
            /// </summary>
            ManyUsersTryConnect = 7,

            /// <summary>
            /// 間違ったフィールドに接続しました．
            /// </summary>
            InvalidField = 8
        }

        /// <summary>
        /// ミニマップ・その他
        /// </summary>
        [BitField(0, 0x07)]
        private byte MinimapAndOther { get; set; }

        /// <summary>
        /// 攻城戦の状態？
        /// 0だと
        /// 味方のギルド戦略ポイントが破壊されました。
        /// ギルド紋章の耐久力の1/4減少します。
        /// 最大体力増加ボーナスが5％減少します。
        /// </summary>
        [BitField(1, 0x03)]
        private byte SiegeWarfareState { get; set; } = 7;

        [BitField(2, 0x03)]
        private byte GMLevel { get; set; }

        [BitField(3, 0x0C)]
        private ushort DeathPenarty { get; set; }

        [BitField(4, 0x01)]
        private byte IsProgressEvent { get; set; } = 0;

        [BitField(5, 0x01)]
        private byte Unknown2 { get; set; } = 0;

        [BitField(6, 0x01)]
        private byte IsBossZone { get; set; } = 0;

        [BitField(7, 0x01)]
        private byte IsSiegeWarfareField { get; set; } = 0;

        [BitField(8, 0x01)]
        private byte IsGuildPointBattleField { get; set; } = 0;

        [BitField(9, 0x01)]
        private byte Unknown3 { get; set; } = 1;

        /// <summary>
        /// g_iWorldServerType==4の場合
        /// </summary>
        [BitField(10, 0x01)]
        private byte IsWaitGetGVGAvatarOrgData { get; set; } = 0;

        JoinGameResult Result;

        ushort CharID;
        ushort FieldSerial;

        /// <summary>
        /// 成功
        /// </summary>
        /// <param name="result"></param>
        /// <param name="player"></param>
        /// <param name="mapHeader"></param>
        public ResultJoinGamePacket(Player player, Map.MapHeader mapHeader)
        {
            Result = JoinGameResult.Success;
            CharID = player.CharID;
            FieldSerial = player.MapSerial;

            MinimapAndOther = mapHeader.MinimapAndOther;
            GMLevel = (byte)player.GetSpecialState().GMLevel;
            DeathPenarty = (ushort)player.GetSpecialState().DeathPenarty;
        }

        /// <summary>
        /// 失敗
        /// </summary>
        /// <param name="result"></param>
        public ResultJoinGamePacket(JoinGameResult result)
        {
            if (result == JoinGameResult.Success) throw new ArgumentException("成功の場合は成功用コンストラクタを使用して下さい．");
            Result = result;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write((ushort)Result);//Network event

            if (Result != JoinGameResult.Success)
            {
                //成功以外はここで終わり
                packetSize = (ushort)writer.ToArray().Count();
                return writer.ToArray();
            }

            writer.Write(CharID);
            writer.Write((uint)0x0);//自分+0x2F0
            writer.Write((ushort)0x0);

            //フラグ書き込み
            writer.Write(BitField.ToBytes<ResultJoinGamePacket, BitField>(this, GetType()));

            //最終HP+（％）2で+10％ | mask:0x0F は RemainGuildStrategyPoint
            writer.Write((ushort)(0x00 & 0x0F));
            
            //秘密ダンジョン用MAPID　0x800以上で秘密ダンジョン
            writer.Write((ushort)FieldSerial);
            
            // 0F以外=上２つが0x0800,0x0801の時は間違ったフィールド
            // 0FだとGH扱い？
            // 0x0Fより上位ビットはg_word_E1152C（雪・雨のエフェクトの有無）にはいる
            writer.Write((byte)0x00);
            writer.Write((byte)0x00);//??

            //g_dword_2A5F9D0
            writer.Write((ushort)0);

            writer.WriteBytes(0, 10);

            writer.Write((ushort)0x0000);//BoostEXPSecondByTreasureMapEventInServer
            writer.Write((ushort)0x0000);//BoostGoldSecondByTreasureMapEventInServer

            writer.WriteBytes(0, 10);

            writer.Write((uint)100);//HeavenReadStoneCount
            writer.Write((uint)100);//HellReadStoneCount
            writer.Write((uint)100);//ReadDevilReadStoneCount

            writer.Write((uint)0x0);//???
            writer.Write((uint)(0x0 & 0x1F) << 0x0B);//自分+0x12C

            foreach (byte b in Enumerable.Range(0, 0x44))
            {
                writer.Write((byte)0);
            }
            writer.Write((ushort)0);//ステ・レベル固定フラグ
            writer.Write((ushort)2);//固定値

            writer.Write((ushort)0);
            writer.Write((ushort)0);
            writer.Write((ushort)0);
            for (int i = 0; i < 8; i++)//ギルド縁故地　８か所
            {
                writer.Write((ushort)0xFFFF);
            }

            foreach (byte b in Enumerable.Range(0, 0x0F))
            {
                writer.Write((byte)0);
            }
            for (int i = 0; i < 0x100; i++)
            {
                writer.Write((uint)0);
            }
            

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1121);
        }
    }
}
