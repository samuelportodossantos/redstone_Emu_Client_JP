using RedStoneLib.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RedStoneLib.Model
{
    /// <summary>
    /// 進行中のクエスト
    /// </summary>
    public class ProgressQuest
    {
        public int Index;
        public int Progress;

        /// <summary>
        /// クエストの状況
        /// </summary>
        public uint QuestInfo
            => (uint)((Progress * 8) + (Index << 7) + 3);

        public ProgressQuest(int qIndex, int progress = 0)
        {
            Index = qIndex;
            Progress = progress;
        }

        public override string ToString()
        {
            return Quest.GeneralQuests[(ushort)Index].Name;
        }
    }

    /// <summary>
    /// 存在するクエスト
    /// </summary>
    public class Quest
    {
        /// <summary>
        /// 全てのメインクエスト
        /// </summary>
        public static Dictionary<ushort, Quest> MainQuests { get; private set; } = new Dictionary<ushort, Quest>();

        /// <summary>
        /// 全ての一般クエスト
        /// </summary>
        public static Dictionary<ushort, Quest> GeneralQuests { get; private set; } = new Dictionary<ushort, Quest>();

        /// <summary>
        /// インデックス
        /// </summary>
        ushort Index;

        /// <summary>
        /// クエスト名
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 説明
        /// </summary>
        string[] Descriptions;

        /// <summary>
        /// readerから取得
        /// </summary>
        /// <param name="questReader"></param>
        private Quest(PacketReader questReader)
        {
            Index = questReader.ReadUInt16();
            Name = questReader.ReadSjis(0x40);

            List<string> descs = new List<string>();
            for(int i = 0; i < 6; i++)
            {
                descs.Add(questReader.ReadSjis(0x800));
            }
            Descriptions = descs.ToArray();

            //Skip
            questReader.BaseStream.Seek(0x42, SeekOrigin.Current);
            ushort unk_s1 = questReader.ReadUInt16();
            uint unk_i1 = questReader.ReadUInt32();
            ushort unk_s2 = questReader.ReadUInt16();
            uint unk_i2 = questReader.ReadUInt32();
            ushort unk_s3 = questReader.ReadUInt16();
            string npc_name = questReader.ReadSjis();
        }

        /// <summary>
        /// 全てのアイテムを読み込む
        /// </summary>
        public static void Load()
        {
            using (PacketReader br = new PacketReader(Helper.StreamFromAssembly("Scenario.Red Stone.rpd")))
            {
                br.ReadUInt32();//0xCCCCCCCC
                string fileversion_str = br.ReadSjis(0x32);
                string redstone_str = br.ReadSjis(0x0A);//Red Stone
                string redstone_one_str = br.ReadSjis(0x80);//Red Stone.one

                //復号化キーのセット
                br.SetDataEncodeTable(br.ReadInt32());

                //クエスト読み込み
                void ReadQuests(Action<Quest> insert)
                {
                    uint len = br.EncryptionRead<uint>();
                    for (int i = 0; i < len; i++)
                    {
                        using (PacketReader questReader =
                            new PacketReader(new MemoryStream(br.EncryptionReads<byte>(0x31E4))))
                        {
                            insert(new Quest(questReader));
                        }
                    }
                }

                //メインクエ読み込み
                ReadQuests(q => MainQuests[q.Index] = q);

                //一般クエ読み込み
                ReadQuests(q => GeneralQuests[q.Index] = q);
            }
        }

        public override string ToString()
            => Name;
    }
}
