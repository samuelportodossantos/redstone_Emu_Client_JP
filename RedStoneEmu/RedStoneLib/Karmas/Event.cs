using RedStoneLib.Model;
using RedStoneLib.Packets;
using RedStoneLib.Packets.RSPacket.KarmaPacket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Karmas
{
    /// <summary>
    /// 会話+KARMA
    /// </summary>
    public struct Event
    {
        /// <summary>
        /// 選択肢のタイプ　enum要作成
        /// </summary>
        readonly ushort SpeechType ;

        /// <summary>
        /// NPCとの会話
        /// </summary>
        readonly string Message ;

        /// <summary>
        /// イベント発生条件
        /// </summary>
        readonly KarmaItemCondition[] OccurrenceCondition;

        /// <summary>
        /// イベント発生条件関係
        /// </summary>
        public readonly Karma.ConditionFlag OccurrenceReletion;

        /// <summary>
        /// イベント
        /// </summary>
        public readonly Selection[] Selections ;

        public readonly ushort Unknown_0 ;

        /// <summary>
        /// 自動スタートフラグ
        /// </summary>
        public readonly bool AutoStart ;
        
        /// <summary>
        /// PacketReaderから生成
        /// </summary>
        /// <param name="pr"></param>
        /// <param name="speechType"></param>
        public Event(PacketReader pr, ushort speechType)
        {
            SpeechType = speechType;
            using (PacketReader baseReader = new PacketReader(
                pr.EncryptionReads<byte>(0x0C)))
            {
                Unknown_0 = baseReader.ReadUInt16();

                //メッセージ
                Message = Helper.SjisByteToString(pr.EncryptionReads<byte>(baseReader.ReadInt16()));
                //Console.WriteLine(Message);

                //選択肢の長さ
                Selections = new Selection[baseReader.ReadUInt16()];

                //イベント発生条件
                OccurrenceCondition = new KarmaItemCondition[baseReader.ReadUInt16()];
                for (int i = 0; i < OccurrenceCondition.Length; i++)
                {
                    OccurrenceCondition[i] = new KarmaItemCondition(pr);
                }

                OccurrenceReletion = (Karma.ConditionFlag)baseReader.ReadUInt16();
                AutoStart = Convert.ToBoolean(baseReader.ReadUInt16());

                //イベント
                for (int i = 0; i < Selections.Length; i++)
                {
                    Selections[i] = new Selection(pr);

                }
            }
        }
        
        /// <summary>
        /// 実行
        /// </summary>
        /// <param name="player"></param>
        /// <param name="sendPacket"></param>
        /// <param name="charID">NPCのcharID</param>
        public bool Execute(Player player, SendPacketDelegate sendPacket, ushort npcCharID)
        {
            if(!KarmaItemServices.CheckConditions(player, OccurrenceCondition, OccurrenceReletion))
            {
                //発生条件満たさない
                player.PlayerEvent.Progress++;

                //進捗オーバーフロー
                if (player.PlayerEvent.Events.Length <= player.PlayerEvent.Progress) return false;

                player.PlayerEvent.Events[player.PlayerEvent.Progress].Execute(player, sendPacket, npcCharID);
            }
            if (AutoStart)
            {
                //自動ですべての選択肢を選んで実行
                foreach (var selection in Selections)
                {
                    if (selection.Execute(player, sendPacket, npcCharID))
                    {
                        return true;
                    }
                }
                PublicHelper.WriteWarning?.Invoke("[EventAutoStart] nothing");
                return false;
            }
            else
            {
                //普通に表示
                sendPacket(new ComplexSpeechPacket(npcCharID, this));
                return true;
            }
        }

        /// <summary>
        /// PacketWriterに書き出し
        /// </summary>
        /// <param name="pw"></param>
        public void Write(PacketWriter pw)
        {
            pw.Write((uint)Selections.Length);
            pw.Write(SpeechType);
            pw.WriteSjis(Message);
            foreach(var selection in Selections)
            {
                pw.WriteSjis(selection.Message);
            }
        }

        public override string ToString() => Message;

        /// <summary>
        /// 選択肢
        /// </summary>
        public struct Selection
        {
            /// <summary>
            /// 表示文章
            /// </summary>
            public string Message;

            /// <summary>
            /// イベント
            /// </summary>
            Karma[] Karmas;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="pr"></param>
            public Selection(PacketReader pr)
            {
                Karmas = new Karma[pr.ReadUInt16()];

                //選択肢の文章
                Message = Helper.SjisByteToString(pr.EncryptionReads<byte>(pr.ReadUInt16()));

                //イベントの内容
                for (int i = 0; i < Karmas.Length; i++)
                {
                    Karmas[i] = new Karma(pr);
                }
            }

            /// <summary>
            /// Karmaの実行
            /// </summary>
            public bool Execute(Player player, SendPacketDelegate sendPacket, ushort npcCharID)
            {
                foreach(var karma in Karmas)
                {
                    if (karma.Check(player))
                    {
                        karma.ExecuteCommands(player, sendPacket, npcCharID);
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
