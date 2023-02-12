using RedStoneLib.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Model.Base
{
    /// <summary>
    /// Actorの型
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct Breed
    {
        /// <summary>
        /// 全actorの基底
        /// </summary>
        public static Breed[] AllMonsters { get; private set; } = null;

        /// <summary>
        /// データ内の全てを読み込む
        /// </summary>
        public static void Load()
        {
            using (PacketReader br = new PacketReader(Helper.StreamFromAssembly("Scenario.Red_Stone.job2.dat")))
            {
                AllMonsters = new Breed[br.ReadInt32()];
                AllMonsters = br.Reads<Breed>(AllMonsters.Length).ToArray();
                //1327 サキュ
                //1145 レッドアイ
            }
        }

        /// <summary>
        /// インデックス
        /// </summary>
        public uint Index;

        /// <summary>
        /// 名前
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
        public string Name;

        /// <summary>
        /// 不明
        /// </summary>
        public uint Effect;

        public ushort Unknown_0;
        public uint Unknown_1;
        public ushort Unknown_2;
        public ushort Unknown_3;

        /// <summary>
        /// 種族
        /// </summary>
        public ActorRace Race;

        /// <summary>
        /// ステータスベース
        /// </summary>
        public ushort StatusFactor;

        /// <summary>
        /// 攻撃力ボーナス最大・最小値
        /// </summary>
        public Scale<ushort> AttackValueBonusScale;

        /// <summary>
        /// 防御力ボーナス
        /// </summary>
        public ushort DefenceValueBonus;

        /// <summary>
        /// 移動速度
        /// </summary>
        public ushort MoveSpeed;

        /// <summary>
        /// 攻撃速度
        /// </summary>
        public ushort AttackSpeed;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public ushort[] Unknown_4;

        /// <summary>
        /// ブロック率
        /// </summary>
        public ushort Blocking;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public ushort[] Unknown_5;

        /// <summary>
        /// 系統
        /// </summary>
        public MonsterLineage Lineage;

        /// <summary>
        /// 致命打抵抗
        /// </summary>
        public short FatalResistance;

        /// <summary>
        /// 決定打抵抗
        /// </summary>
        public short DecisionStrikeResistance;

        /// <summary>
        /// 基本経験値
        /// </summary>
        public uint DefaultEXP;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public ushort[] Unknown_6;

        /// <summary>
        /// ドロップアイテム
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public ActorDropItem[] DropItems;

        /// <summary>
        /// スキル
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public MonsterSkill[] MonsterSkills;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
        public ushort[] Unknown_7;

        public uint Unknown_8;

        /// <summary>
        /// HPのスケール
        /// </summary>
        public uint DefaultHP;

        /// <summary>
        /// CPのスケール?
        /// </summary>
        public uint DefaultCP;

        public uint Unknown;

        /// <summary>
        /// レベルによるステータスの向上
        /// </summary>
        public ushort LevelUpBonus;

        /// <summary>
        /// 健康によるステータスの向上
        /// </summary>
        public ushort StateBonus;

        /// <summary>
        /// ステータス
        /// </summary>
        public ActorStatus MonsterStatus;

        /// <summary>
        /// アクティブになる範囲
        /// </summary>
        public ushort ActiveRange;

        /// <summary>
        /// 攻撃力の最大・最小値
        /// </summary>
        public Scale<ushort> AtackValueScale;

        /// <summary>
        /// 防御力値
        /// </summary>
        public ushort DefenceValue;

        public short Unknown_9;

        /// <summary>
        /// 各種魔法抵抗
        /// </summary>
        public Magic MagicResistance;

        /// <summary>
        /// 各状態異常抵抗
        /// </summary>
        public Actor.StatusAbnormal StatusAbnormalResistance;

        /// <summary>
        /// 全状態異常抵抗
        /// </summary>
        public short AllStatusAbnormalResistance;

        /// <summary>
        /// 低下系抵抗
        /// </summary>
        public short AllStatusDeclineResistance;

        /// <summary>
        /// 呪い抵抗
        /// </summary>
        public short AllStatusSpellResistance;


        /// <summary>
        /// ドロップアイテム型
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ActorDropItem
        {
            /// <summary>
            /// アイテムタイプ
            /// </summary>
            public ItemBase.ItemType Type;

            ushort Unknown;

            /// <summary>
            /// ドロップ確率
            /// </summary>
            public uint Prob;

            public override string ToString() => $"[{Prob}] {Type.ToString()}";
        }

        /// <summary>
        /// モンスターのスキル
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MonsterSkill
        {
            /// <summary>
            /// スキルID
            /// </summary>
            public ushort SkillIndex;

            ushort Unknown;
        }

        /// <summary>
        /// モンスターの種族
        /// </summary>
        public enum ActorRace : ushort
        {
            /// <summary>
            /// アンデッド型
            /// </summary>
            Undead = 0,

            /// <summary>
            /// 人間型
            /// </summary>
            Human = 1,

            /// <summary>
            /// 悪魔型
            /// </summary>
            Devil = 2,

            /// <summary>
            /// 動物型
            /// </summary>
            Animal = 3,

            /// <summary>
            /// 神獣型
            /// </summary>
            GodAnimal = 4,
        }

        /// <summary>
        /// モンスターの系統
        /// </summary>
        public enum MonsterLineage : ushort
        {
            一般１ = 0,
            一般２ = 1,
            一般３ = 2,
            一般４ = 3,
            セミボス１ = 4,
            セミボス２ = 5,
            セミボス３ = 6,
            ボス１ = 7,
            ボス２ = 8,
            ボス３ = 9,
        }

        public override string ToString()
            => Name;
    }
}
