using RedStoneLib.Model;
using RedStoneLib.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Karmas
{
    /// <summary>
    /// NPC・モンスター・エリアのイベント
    /// </summary>
    public struct Karma
    {
        public ushort unknown_1 { get; private set; }

        /// <summary>
        /// 複数条件間の関係
        /// </summary>
        public ConditionFlag ConditionRelation { get; private set; }

        /// <summary>
        /// 発生確率
        /// </summary>
        public ushort Probabirity { get; private set; }

        public ushort unknown_2 { get; private set; }

        public ushort unknown_3 { get; private set; }

        /// <summary>
        /// 韓国語のコメント
        /// </summary>
        public string Comment { get; private set; }

        /// <summary>
        /// 条件用
        /// </summary>
        public KarmaItemCondition[] Conditions { get; private set; }

        /// <summary>
        /// 実行用
        /// </summary>
        public KarmaItemCommand[] Commands { get; private set; }

        [Flags]
        public enum ConditionFlag : ushort
        {
            Or = 0,
            And = 1,
            Not = 2,
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="pr"></param>
        /// <param name="encryptLevel"></param>
        public Karma(PacketReader pr)
        {
            using (PacketReader baseReader = new PacketReader(
                pr.EncryptionReads<byte>(0x10)))
            {
                unknown_1 = baseReader.ReadUInt16();

                //条件
                ConditionRelation = (ConditionFlag)baseReader.ReadUInt16();

                //実行の長さ
                Commands = new KarmaItemCommand[baseReader.ReadUInt16()];
                
                //条件の長さ
                Conditions = new KarmaItemCondition[baseReader.ReadUInt16()];
                
                //メッセージの長さ
                ushort messageLength = baseReader.ReadUInt16();

                //確率
                Probabirity = baseReader.ReadUInt16();

                unknown_2 = baseReader.ReadUInt16();
                unknown_3 = baseReader.ReadUInt16();

                //メッセージ
                Comment = Helper.SjisByteToString(pr.EncryptionReads<byte>(messageLength), "EUC-KR");
            }

            //条件
            for (int i = 0; i < Conditions.Length; i++)
            {
                Conditions[i] = new KarmaItemCondition(pr);
            }

            //実行
            for (int i = 0; i < Commands.Length; i++)
            {
                Commands[i] = new KarmaItemCommand(pr);
            }
        }

        /// <summary>
        /// 条件チェック
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool Check(Player player)
        {
            if (Helper.StaticRandom.Next(100) > Probabirity)
            {
                //確率でふるい落とし
                return false;
            }
            else if (KarmaItemServices.CheckConditions(player, Conditions, ConditionRelation))
            {
                //条件勝ち
                return true;
            }
            else
            {
                //条件負け
                return false;
            }
        }

        /// <summary>
        /// 実行
        /// </summary>
        /// <param name="player"></param>
        /// <param name="sendPacket"></param>
        public void ExecuteCommands(Player player, SendPacketDelegate sendPacket, ushort npcCharID)
            => KarmaItemServices.ExecuteCommands(player, Commands, sendPacket, npcCharID);

        /// <summary>
        /// override
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string result = string.Format("[{0}]", Comment);

            //条件文
            if (Conditions != null)
                result += string.Format(" If {0}", string.Join(ConditionRelation.HasFlag(ConditionFlag.And) ? " && " : " || ", Conditions.Select(t => t.ToString())));
            //実行文
            if (Commands != null)
                result += string.Format(" Do {0}", string.Join(" + ", Commands.Select(t => t.ToString())));
            return result;
        }

    }
}
