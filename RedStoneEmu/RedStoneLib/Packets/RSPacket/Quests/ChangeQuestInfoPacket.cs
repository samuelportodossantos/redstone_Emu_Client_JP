using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedStoneLib.Packets.RSPacket.Quests
{
    public class ChangeQuestInfoPacket : Packet
    {
        public enum QuestStatus : ushort
        {
            Start = 0x0000,
            Update = 0x6000,
            Complete = 0x0060
        }

        /// <summary>
        /// クエスト欄の位置
        /// </summary>
        [BitField(0, 0x04)]
        byte TargetIndex { get; set; }

        [BitField(1, 0x0C)]
        ushort Junk => 0xCCC;

        /// <summary>
        /// クエストID
        /// </summary>
        [BitField(2)]
        uint QuestInfo
        {
            get
            {
                if (Status == QuestStatus.Complete) return 0;
                else return _QuestInfo;
            }
        }
        uint _QuestInfo;

        /// <summary>
        /// クエストの情報
        /// </summary>
        [BitField(3, 0x10, type: typeof(ushort))]
        QuestStatus Status { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="targetIndex"></param>
        /// <param name="status"></param>
        /// <param name="questInfo"></param>
        public ChangeQuestInfoPacket(byte targetIndex, QuestStatus status, uint questInfo)
        {
            TargetIndex = targetIndex;
            Status = status;
            _QuestInfo = questInfo;
        }

        /// <summary>
        /// 完了
        /// </summary>
        /// <param name="targetIndex"></param>
        public ChangeQuestInfoPacket(byte targetIndex)
        {
            TargetIndex = targetIndex;
            Status = QuestStatus.Complete;
            _QuestInfo = 0;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();
            
            writer.Write(BitField.ToBytes<ChangeQuestInfoPacket, BitField>(this, GetType()));

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x11BD);
        }
    }
}
