using RedStoneLib.Karmas;
using RedStoneLib.Packets;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RedStoneLib.Model.Base
{
    /// <summary>
    /// キャラクタの基底
    /// </summary>
    public abstract class Actor
    {
        /// <summary>
        /// ActorのType
        /// </summary>
        protected abstract byte ActorType { get; }

        /// <summary>
        /// ActorのCID
        /// </summary>
        [MinimumActorInfo(0, 13)]
        public abstract ushort CharID { get; set; }

        /// <summary>
        /// クエスト関係者
        /// </summary>
        [MinimumActorInfo(1, 3, type:typeof(byte))]
        protected bool IsQuestParticipants { get; set; } = false;

        /// <summary>
        /// 頭上？ +0xDE
        /// </summary>
        [MinimumActorInfo(2)]
        protected ushort QuestID { get; set; } = 0xFFFF;

        /// <summary>
        /// 現在地
        /// </summary>
        [NotMapped]
        public ushort MapSerial { get; set; }

        /// <summary>
        /// X座標
        /// </summary>
        [MinimumActorInfo(3)]
        public abstract ushort PosX { get; set; }

        /// <summary>
        /// Y座標
        /// </summary>
        [MinimumActorInfo(4)]
        public abstract ushort PosY { get; set; }

        /// <summary>
        /// 名前
        /// </summary>
        public abstract string Name { get; set; }

        /// <summary>
        /// 種族
        /// </summary>
        public abstract Breed.ActorRace Race { get; }

        /// <summary>
        /// 職業
        /// </summary>
        public abstract Player.JOB Job { get; set; }

        /// <summary>
        /// レベル
        /// </summary>
        public abstract ushort Level { get; set; }

        /// <summary>
        /// ステータス
        /// </summary>
        public abstract ActorStatus Status { get; set; }

        /// <summary>
        /// 最大HP
        /// </summary>
        public abstract uint MaxHP { get; set; }

        /// <summary>
        /// 現在HP
        /// </summary>
        public abstract uint NowHP { get; set; }

        /// <summary>
        /// 最大CP
        /// </summary>
        public abstract uint MaxCP { get; set; }

        /// <summary>
        /// 現在CP
        /// </summary>
        public abstract int NowCP { get; set; }

        /// <summary>
        /// 攻撃力（スキル補正考慮）
        /// +n% +n
        /// </summary>
        public abstract Func<double, double, Scale<double>> Attack { get; set; }

        /// <summary>
        /// 防御力
        /// </summary>
        public abstract double Defence { get; set; }

        /// <summary>
        /// 魔法抵抗
        /// </summary>
        public abstract Magic MagicResistance { get; set; }

        /// <summary>
        /// 大きさ
        /// </summary>
        public abstract Size<ushort> BodySize { get; set; }

        /// <summary>
        /// 方向
        /// </summary>
        public abstract ActorDirect Direct { get; set; }

        /// <summary>
        /// 主にスキル用のアニメ +0x2F0
        /// </summary>
        public virtual ActorAnim1 Anim1 { get; set; }

        /// <summary>
        /// +0x2F4
        /// </summary>
        public virtual ActorAnim2 Anim2 { get; set; }

        /// <summary>
        /// +0x2F8
        /// </summary>
        public virtual ActorAnim3 Anim3 { get; set; }

        /// <summary>
        /// +0x2FC
        /// </summary>
        public virtual ActorAnim4 Anim4 { get; set; }

        /// <summary>
        /// +0x300
        /// </summary>
        public virtual ActorAnim5 Anim5 { get; set; }

        /// <summary>
        /// 時間制限付きの能力アップ
        /// </summary>
        public virtual Effect.ActorBuff Buff { get; set; }

        /// <summary>
        /// 取りうるレベルのスケール
        /// </summary>
        public abstract Scale<ushort> LevelScale { get; set; }
        
        /// <summary>
        /// フライウェイト
        /// </summary>
        public Breed ABase => Breed.AllMonsters[(ushort)Job];

        /// <summary>
        /// 座標
        /// </summary>
        public Point<ushort> Pos
            => new Point<ushort> { X = PosX, Y = PosY };
        
        /// <summary>
        /// 移動速度
        /// </summary>
        public abstract ushort MoveSpeed { get; set; }

        /// <summary>
        /// スキル
        /// </summary>
        public virtual Dictionary<ushort, Skill> Skills
            => ABase.MonsterSkills.Where(t=>t.SkillIndex!=0xFFFF).ToDictionary(t => t.SkillIndex, t => Skill.AllSkills[t.SkillIndex]);

        /// <summary>
        /// 戦闘中
        /// </summary>
        [NotMapped]
        public bool IsButtleNow { get; set; }

        /// <summary>
        /// 攻撃速度待ち時間を取得
        /// </summary>
        /// <param name="skill"></param>
        /// <returns></returns>
        public virtual double GetAttackSpeedWaitTime(Skill skill, int skillLevel)
        {
            //攻撃速度+(%)
            double atkSpeedRate = skill.AttackSpeed[skillLevel] + Buff.AttackSpeed;
            //フレーム
            double frame = Math.Floor(Math.Floor(16.0 * ABase.AttackSpeed / 100.0) * (100.0 / (100.0 + atkSpeedRate)));
            return 1000.0 * frame / 12.0;
        }

        /// <summary>
        /// Monster, NPC用
        /// </summary>
        /// <param name="single"></param>
        /// <param name="group"></param>
        /// <param name="charID"></param>
        protected Actor(MapActorSingle single, MapActorGroup group, ushort charID, ushort mapSerial)
            : this()
        {
            LevelScale = new Scale<ushort> { Max = group.MaxLevel, Min = group.MinLevel };

            CharID = charID;
            MapSerial = mapSerial;
            Name = single.Name;
            PosX = (ushort)single.Point.X;
            PosY = (ushort)single.Point.Y;
            BodySize = group.Size;
            Direct = single.Direct;
            Job = (Player.JOB)group.Job;
        }

        /// <summary>
        /// Player用
        /// </summary>
        protected Actor()
        {
            Buff = new Effect.ActorBuff(ChangeAnim);
        }

        /// <summary>
        /// 相手との距離
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public double DistanceTo(Actor target)
            => Helper.Hypot(PosX, PosY, target.PosY, target.PosY);

        /// <summary>
        /// レベルとパラメーターをセット
        /// </summary>
        public void SetStatusByLevel(ushort level)
        {
            //レベル代入
            Level = level;

            //ステータス
            var statusLevelBonus = ABase.MonsterStatus * ABase.StatusFactor / 100000.0;
            Status = statusLevelBonus * (level - 1) + ABase.MonsterStatus;

            //HP
            var hpBase = ABase.DefaultHP / 100.0;
            var levelBonus = ABase.LevelUpBonus / 10.0;
            var stateBonus = ABase.StateBonus / 10.0;
            MaxHP = (uint)Math.Floor((levelBonus * level + hpBase) + (stateBonus * Status.Condition));
            NowHP = MaxHP;

            //CP
            var cpBase = ABase.DefaultCP / 100.0;
            MaxCP = (uint)Math.Floor((levelBonus * level + cpBase) + (stateBonus * Status.Charisma));
            NowCP = (int)MaxCP;

            //攻撃力
            var attackBonus = ABase.AttackValueBonusScale / 100.0 * (level - 1);
            Attack = (np, n) => attackBonus + (ABase.AtackValueScale * (100.0 + np) / 100.0 + n) * (1.0 + Status.Strength / 200.0);

            //防御力
            Defence = (ABase.DefenceValueBonus * (level-1) + ABase.DefenceValue) * (1.0 + Status.Condition / 200.0);

            //魔法抵抗
            MagicResistance = ABase.MagicResistance + Status.GetResistanceByWisdom();
        }

        /// <summary>
        /// 頭上アイコン変更
        /// </summary>
        /// <param name="actorAnim"></param>
        /// <param name="set"></param>
        private void ChangeAnim(object actorAnim, bool set)
        {
            switch (actorAnim)
            {
                case ActorAnim1 n:
                    Anim1 = set ? Anim1 | n : Anim1 & ~n;
                    break;
                case ActorAnim2 n:
                    Anim2 = set ? Anim2 | n : Anim2 & ~n;
                    break;
                case ActorAnim3 n:
                    Anim3 = set ? Anim3 | n : Anim3 & ~n;
                    break;
                case ActorAnim4 n:
                    Anim4 = set ? Anim4 | n : Anim4 & ~n;
                    break;
                case ActorAnim5 n:
                    Anim5 = set ? Anim5 | n : Anim5 & ~n;
                    break;
                default:
                    throw new ArgumentException();
            }
        }

        /// <summary>
        /// HP減らす
        /// </summary>
        /// <param name="sendPacket"></param>
        /// <param name="totalDamage"></param>
        public void DecreaseHP(double totalDamage)
        {
            NowHP = (uint)Math.Max(Math.Round(NowHP - totalDamage), 0);
        }

        public override string ToString() => Name;

        /// <summary>
        /// castしてunckecked
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        protected static T unchecked_cast<T>(object value) => unchecked((T)Convert.ChangeType(value, typeof(T)));

        /// <summary>
        /// マップのActorグループ
        /// </summary>
        public struct MapActorGroup
        {
            /// <summary>
            /// 内部ID
            /// </summary>
            public ushort InternalID;

            /// <summary>
            /// 職業２のIndex
            /// </summary>
            public ushort Job;

            /// <summary>
            /// 出現最低レベル
            /// </summary>
            public ushort MinLevel;

            /// <summary>
            /// 出現名前
            /// </summary>
            public string Name;

            /// <summary>
            /// キャラサイズ
            /// </summary>
            public Size<ushort> Size;

            /// <summary>
            /// 出現最高レベル
            /// </summary>
            public ushort MaxLevel;

            /// <summary>
            ///イメージID
            /// </summary>
            public ushort ImageID;

            public ushort unknown_1;
            public ushort[] ImageSumCandidate;//画像差分候補
            public byte[] unknown_3;
            public ushort unknown_4;//元Image

            /// <summary>
            /// イベント情報
            /// </summary>
            public MapEnemyKarmaInfo[] EnemyKarmaInfos;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="pr"></param>
            /// <param name="encryptLevel"></param>
            /// <param name="structLength"></param>
            public MapActorGroup(PacketReader pr, int structLength, ushort job2Index)
            {
                Job = job2Index;

                //基本情報
                using (PacketReader baseReader = new PacketReader(
                    pr.EncryptionReads<byte>(structLength)))
                {
                    InternalID = baseReader.ReadUInt16();
                    unknown_1 = baseReader.ReadUInt16();
                    MinLevel = baseReader.ReadUInt16();
                    Name = baseReader.ReadSjis(0x14);
                    ImageSumCandidate = baseReader.Reads<ushort>(0x03);
                    unknown_4 = baseReader.ReadUInt16();
                    Size = baseReader.ReadStruct<Size<ushort>>();
                    MaxLevel = baseReader.ReadUInt16();
                    unknown_3 = baseReader.ReadBytes((int)(structLength - baseReader.BaseStream.Position));

                    //画像番号
                    ImageID = (ushort)(100 + job2Index);
                }

                //イベント情報
                EnemyKarmaInfos = new MapEnemyKarmaInfo[pr.ReadInt32()];
                for (int i = 0; i < EnemyKarmaInfos.Length; i++)
                {
                    //モンスターイベント固有情報
                    EnemyKarmaInfos[i] = new MapEnemyKarmaInfo
                    {
                        Timing = (EnemyKarmaTiming)pr.ReadInt32(),//too enough?
                        Karmas = new Karma[pr.ReadUInt16()],
                        Comment = Helper.SjisByteToString(pr.EncryptionReads<byte>(pr.ReadInt16()), "EUC-KR")
                    };

                    //KARMA
                    for (int j = 0; j < EnemyKarmaInfos[i].Karmas.Length; j++)
                    {
                        EnemyKarmaInfos[i].Karmas[j] = new Karma(pr);
                    }
                }
            }

            public override string ToString() => Name;

            /// <summary>
            /// マップのエネミーイベント情報
            /// </summary>
            public struct MapEnemyKarmaInfo
            {
                public EnemyKarmaTiming Timing;
                public string Comment;
                public Karma[] Karmas;
            }

            /// <summary>
            /// エネミーKarmaの発動タイミング
            /// </summary>
            public enum EnemyKarmaTiming : uint
            {
                /// <summary>
                /// プレイヤーに殺されたら発動
                /// </summary>
                KilledByPlayer = 0,

                /// <summary>
                /// バトルが始まったら発動
                /// </summary>
                StartBattle = 1,

                /// <summary>
                /// プレイヤーが殺したら発動
                /// </summary>
                KillPlayer = 2,

                /// <summary>
                /// 常に発動
                /// </summary>
                Always = 3,
            }
        }

        /// <summary>
        /// マップのActor単体
        /// </summary>
        public struct MapActorSingle
        {
            /// <summary>
            /// 不明
            /// </summary>
            public uint Index;

            /// <summary>
            /// 内部ID？
            /// </summary>
            public ushort InternalID;

            /// <summary>
            /// NPCのタイプ
            /// </summary>
            public CType CharType;

            /// <summary>
            /// 方向
            /// </summary>
            public ActorDirect Direct;

            /// <summary>
            /// 湧き時間
            /// </summary>
            public uint PopSpeed;

            /// <summary>
            /// 座標
            /// </summary>
            public Point<uint> Point;

            /// <summary>
            /// 名前
            /// </summary>
            public string Name;

            /// <summary>
            /// イベント
            /// </summary>
            public Event[] Events;

            public ushort Unknown_0;

            public byte[] Unknown_1;

            public byte[] Unknown_2;

            /// <summary>
            /// Player以外のActorのタイプ
            /// </summary>
            public enum CType : ushort
            {
                /// <summary>
                /// プレイヤー
                /// </summary>
                Player = 0,

                /// <summary>
                /// NPC
                /// </summary>
                NPC = 1,

                /// <summary>
                /// モンスター
                /// </summary>
                Monster = 2,

                /// <summary>
                /// 武具商人
                /// </summary>
                Equipment_merchant = 3,

                /// <summary>
                /// 防具商人
                /// </summary>
                ArmorMerchant = 4,

                /// <summary>
                /// 雑貨商人
                /// </summary>
                MiscellaneousGoodsMerchant = 5,

                /// <summary>
                /// 道具商人
                /// </summary>
                ToolMerchant = 6,

                /// <summary>
                /// 取引仲介人
                /// </summary>
                BrokerageHuman = 7,

                /// <summary>
                /// 銀行員
                /// </summary>
                Banker = 8,

                /// <summary>
                /// スキルマスター
                /// </summary>
                SkillMaster = 9,

                /// <summary>
                /// 一般クエスト
                /// </summary>
                GeneralQuest = 10,

                /// <summary>
                /// 称号クエスト
                /// </summary>
                TitleQuest = 11,

                /// <summary>
                /// ギルドクエスト
                /// </summary>
                GuildQuest = 12,

                /// <summary>
                /// メインクエスト
                /// </summary>
                MainQuest = 13,

                /// <summary>
                /// サポーター1
                /// </summary>
                Supporters1 = 14,

                /// <summary>
                /// テレポーター
                /// </summary>
                Teleporters = 15,

                /// <summary>
                /// 治療師
                /// </summary>
                Healers = 16,

                /// <summary>
                /// クエスト案内人
                /// </summary>
                QuestGuidPeople = 17,

                /// <summary>
                /// 鍛冶屋
                /// </summary>
                Blacksmith = 18,

                /// <summary>
                /// サポーター2
                /// </summary>
                Supporters2 = 19,

                /// <summary>
                /// クエスト依頼人
                /// </summary>
                QuestAskedPeople = 20,

                /// <summary>
                /// クエスト関連人
                /// </summary>
                QuestRelatedPerson = 21,

                /// <summary>
                /// クエストモンスター
                /// </summary>
                QuestMonster = 22,

                /// <summary>
                /// 武器商人<br/>(剣士/戦士)
                /// </summary>
                ArmsDealerSwordsmanOrWarrior = 23,

                /// <summary>
                /// 武器商人<br/>(ウィザード/ウルフマン)
                /// </summary>
                ArmsDealerWizardOrWolfman = 24,

                /// <summary>
                /// 武器商人<br/>(ビショップ/追放天使)
                /// </summary>
                ArmsDealerBishopOrexiled_angel = 25,

                /// <summary>
                /// 武器商人<br/>(シーフ/武道家)
                /// </summary>
                ArmsDealerThiefOrmartial_artist = 26,

                /// <summary>
                /// 武器商人<br/>(アーチャー/ランサー)
                /// </summary>
                ArmsDealerArcherOrLancer = 27,

                /// <summary>
                /// 武器商人<br/>(ビーストテイマー/サマナー)
                /// </summary>
                ArmsDealerBisutoteimaOrSummoner = 28,

                /// <summary>
                /// 武器商人<br/>(プリンセス/リトルウィッチ)
                /// </summary>
                ArmsDealerPrincessOrLittlewitch = 29,

                /// <summary>
                /// 武器商人<br/>(ネクロマンサー/悪魔)
                /// </summary>
                ArmsDealerNecromancerOrDevil = 30,

                /// <summary>
                /// 決戦報酬商人
                /// </summary>
                BattleRewardMerchant = 31,

                /// <summary>
                /// 武器商人<br/>(霊術師/闘士)
                /// </summary>
                WeaponNumerologyTeacherMerchantOrWarrior = 32,

                /// <summary>
                /// ギルドホールテレポーター
                /// </summary>
                GuildHallTeleporters = 33,

                /// <summary>
                /// イベント案内人
                /// </summary>
                EventGuidepeople = 34,

                /// <summary>
                /// 冒険家協会
                /// </summary>
                AdventurerAssociation = 35,

                /// <summary>
                /// 武器商人<br/>(光奏師/獣人)
                /// </summary>
                ArmsDealerLightResponseRateNursesOrBeastPeople = 36,

                /// <summary>
                /// 1Dayクエスト
                /// </summary>
                OneDayQuest = 37,

                /// <summary>
                /// 錬成案内人
                /// </summary>
                DrillingGuidePeople = 38,

                /// <summary>
                /// 武器商人<br/>(メイド/黒魔術師)
                /// </summary>
                ArmsDealerMaidOrBlackMagician = 39
            }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="pr"></param>
            public MapActorSingle(PacketReader pr)
            {
                //基本情報
                using (PacketReader baseReader = new PacketReader(
                    pr.EncryptionReads<byte>(0xB0)))
                {
                    Index = baseReader.ReadUInt32();
                    InternalID = baseReader.ReadUInt16();
                    CharType = (CType)baseReader.ReadUInt16();
                    Direct = (ActorDirect)baseReader.ReadUInt16();
                    Unknown_0 = baseReader.ReadUInt16();
                    PopSpeed = baseReader.ReadUInt32();

                    Unknown_1 = baseReader.ReadBytes(0x78);

                    Point = baseReader.ReadStruct<Point<uint>>();
                    Name = baseReader.ReadSjis(0x10);

                    Unknown_2 = baseReader.ReadBytes(0x10);
                }

                //イベント
                Events = new Event[pr.ReadInt16()];
                if (Events.Length > 0)
                {
                    ushort speech_type = pr.ReadUInt16();
                    ushort unknown = pr.ReadUInt16();
                    for (int i = 0; i < Events.Length; i++)
                    {
                        Events[i] = new Event(pr, speech_type);
                    }
                }
            }

            public override string ToString() => Name;
        }

        /// <summary>
        /// アニメーション1 +0x2F0
        /// </summary>
        [Flags]
        public enum ActorAnim1 : uint
        {
            /// <summary>
            /// のけぞる
            /// </summary>
            ToBeUpright = 0x01,

            /// <summary>
            /// シマー
            /// </summary>
            ShimmeringShield = 0x02,

            /// <summary>
            /// 水壁
            /// </summary>
            WaterWall = 0x04,

            /// <summary>
            /// 霧
            /// </summary>
            MysticFog = 0x08,

            /// <summary>
            /// トルネードシールド
            /// </summary>
            TornadoShield = 0x10,

            /// <summary>
            /// ロックバウンディング
            /// </summary>
            LockBounding = 0x20,

            /// <summary>
            /// 空中浮遊
            /// </summary>
            Levitation = 0x40,

            /// <summary>
            /// 石化
            /// </summary>
            Mineralization = 0x80,

            /// <summary>
            /// コールド　遅くなる
            /// </summary>
            Cold = 0x100,

            /// <summary>
            /// エンチャ
            /// </summary>
            FireEnchantment = 0x200,

            /// <summary>
            /// 暗闇
            /// </summary>
            Blindness = 0x400,

            /// <summary>
            /// 狂気
            /// </summary>
            Insanity = 0x800,

            /// <summary>
            /// 睡眠
            /// </summary>
            Sleep = 0x1000,

            /// <summary>
            /// 混乱
            /// </summary>
            Confusion = 0x2000,

            /// <summary>
            /// 魅了
            /// </summary>
            Fascination = 0x4000,

            /// <summary>
            /// スタン
            /// </summary>
            Stan = 0x8000,

            /// <summary>
            /// ブラー
            /// </summary>
            Blur = 0x10000,

            /// <summary>
            /// 不可視
            /// </summary>
            Invisible = 0x20000,

            /// <summary>
            /// フリーズ　止まる
            /// </summary>
            Freeze = 0x40000,

            /// <summary>
            /// ブレッシング
            /// </summary>
            Bless = 0x80000,

            /// <summary>
            /// なし
            /// </summary>
            None_0 = 0x100000,

            /// <summary>
            /// 座る
            /// </summary>
            ShitDown = 0x200000,

            /// <summary>
            /// プロテクティングエビル
            /// </summary>
            ProtectingEvil = 0x400000,

            /// <summary>
            /// 悪夢
            /// </summary>
            Nightmare = 0x800000,

            /// <summary>
            /// 黒いスタン
            /// </summary>
            BlackStan = 0x1000000,

            /// <summary>
            /// MMM? 盾が回る
            /// </summary>
            MMM = 0x2000000,

            /// <summary>
            /// ディスプレイスメント（犬の操作スキル）
            /// </summary>
            Displacement = 0x4000000,

            /// <summary>
            /// サンクチュアリ
            /// </summary>
            Sanctuary = 0x8000000,

            /// <summary>
            /// ホールドパーソン
            /// </summary>
            HoldPerson = 0x10000000,

            /// <summary>
            /// ホールドモンスター
            /// </summary>
            HoldMonster = 0x20000000,

            /// <summary>
            /// エバンジェリズム
            /// </summary>
            Evangelism = 0x40000000,

            /// <summary>
            /// サバイバル
            /// </summary>
            Survival = 0x80000000,
        }

        /// <summary>
        /// 状態？不明 +2F4
        /// </summary>
        [Flags]
        public enum ActorAnim2 : uint
        {
            /// <summary>
            /// 色が青になる
            /// </summary>
            ColdBody = 0x01,

            /// <summary>
            /// 暗闇
            /// </summary>
            Blind = 0x02,

            /// <summary>
            /// スマイル形の暗闇
            /// </summary>
            Blind_Smile = 0x04,

            /// <summary>
            /// E形の暗闇
            /// </summary>
            Blind_E = 0x08,

            /// <summary>
            /// R形の暗闇
            /// </summary>
            Blind_R = 0x10,

            /// <summary>
            /// N形の暗闇
            /// </summary>
            Blind_N = 0x20,

            /// <summary>
            /// 体が黒になる
            /// </summary>
            BlackBody = 0x40,
        }

        /// <summary>
        /// 頭上のアイコンが多い +0x2F8
        /// </summary>
        [Flags]
        public enum ActorAnim3 : uint
        {
            /// <summary>
            /// 攻撃力+
            /// </summary>
            PowerPlus = 0x01,

            /// <summary>
            /// 防御力+
            /// </summary>
            DefencePlus = 0x02,

            /// <summary>
            /// 命中率+
            /// </summary>
            AccuracyRatePlus = 0x04,

            /// <summary>
            /// 回避率+
            /// </summary>
            AvoidanceRatePlus = 0x08,

            /// <summary>
            /// 攻撃速度+
            /// </summary>
            AttackSpeedPlus = 0x10,

            /// <summary>
            /// 魔法抵抗+
            /// </summary>
            MagicResistancePlus = 0x20,

            /// <summary>
            /// 魔法抵抗+?
            /// </summary>
            MagicResistancePlus2 = 0x40,

            /// <summary>
            /// 移動速度+
            /// </summary>
            MoveSpeedPlus = 0x80,

            /// <summary>
            /// 攻撃力-
            /// </summary>
            PowerMinus = 0x100 * PowerPlus,

            /// <summary>
            /// 防御力-
            /// </summary>
            DefenceMinus = 0x100 * DefencePlus,

            /// <summary>
            /// 命中率-
            /// </summary>
            AccuracyRateMinus = 0x100 * AccuracyRatePlus,

            /// <summary>
            /// 回避率-
            /// </summary>
            AvoidanceRateMinus = 0x100 * AvoidanceRatePlus,

            /// <summary>
            /// 攻撃速度-
            /// </summary>
            AttackSpeedMinus = 0x100 * AttackSpeedPlus,

            /// <summary>
            /// 魔法抵抗-
            /// </summary>
            MagicResistanceMinus = 0x100 * MagicResistancePlus,

            /// <summary>
            /// 魔法抵抗-?
            /// </summary>
            MagicResistanceMinus2 = 0x100 * MagicResistancePlus2,

            /// <summary>
            /// 移動速度-
            /// </summary>
            MoveSpeedMinus = 0x100 * MoveSpeedPlus,

            /// <summary>
            /// ミラーカーズ
            /// </summary>
            MirrorCars = 0x10000,

            /// <summary>
            /// 吸血
            /// </summary>
            BloodSucking = 0x20000,

            /// <summary>
            /// ダークネスイリュージョン
            /// </summary>
            DarknessIllusion = 0x40000,

            /// <summary>
            /// 陰謀の影
            /// </summary>
            ShadowOfConspiracy = 0x80000,

            /// <summary>
            /// 鎧破壊
            /// </summary>
            BreakBody = 0x100000,

            /// <summary>
            /// 武器破壊
            /// </summary>
            BreakWeapon = 0x200000,

            /// <summary>
            /// フレームリング（サマナスキル）
            /// </summary>
            FrameRing = 0x400000,

            /// <summary>
            /// インシナレイト（サマナスキル）
            /// </summary>
            Incinerate = 0x800000,

            /// <summary>
            /// バキュームポイント（サマナスキル）
            /// </summary>
            VacuumPoint = 0x1000000,

            /// <summary>
            /// hot_skin.sad
            /// </summary>
            Unk_hot_skin = 0x2000000,

            /// <summary>
            /// 死の香り
            /// </summary>
            DeadSmell = 0x4000000,

            /// <summary>
            /// status_cancer_hall.sad（キャンサーホール状態）
            /// </summary>
            Unk_status_cancer_hall = 0x8000000,

            /// <summary>
            /// 竜巻斬り（霊術スキル）
            /// </summary>
            TornadoSlash = 0x10000000,

            /// <summary>
            /// なし
            /// </summary>
            None_0 = 0x20000000,

            /// <summary>
            /// なし
            /// </summary>
            None_1 = 0x40000000,

            /// <summary>
            /// レベル低下
            /// </summary>
            LevelDown = 0x80000000,
        }

        /// <summary>
        /// リトル・姫スキルあり +0x2FC
        /// </summary>
        [Flags]
        public enum ActorAnim4 : uint
        {
            /// <summary>
            /// 蘇生術（ネクロ，カウントダウンなし）
            /// </summary>
            Resuscitation = 0x01,

            /// <summary>
            /// 蘇生術　同じ
            /// </summary>
            Resuscitation2 = 0x02,

            /// <summary>
            /// 動けない（アニメーションなし）
            /// </summary>
            CantMove = 0x04,

            /// <summary>
            /// 動けない　同じ
            /// </summary>
            CantMove2 = 0x08,

            /// <summary>
            /// なし
            /// </summary>
            None_0 = 0x10,

            /// <summary>
            /// なし
            /// </summary>
            None_1 = 0x20,

            /// <summary>
            /// ラビットラッシュ
            /// </summary>
            RabbitRush = 0x40,

            /// <summary>
            /// レッツダンシング（回る・アニメーションなし）
            /// </summary>
            LetsDancing = 0x80,

            /// <summary>
            /// マジックボックス（動けない）
            /// </summary>
            MagicBox = 0x100,

            /// <summary>
            /// 死んだふり（どの職業も可・アニメーションなし）
            /// </summary>
            PretendToBeDead = 0x200,

            /// <summary>
            /// 兎変身
            /// </summary>
            RabbitTransform = 0x400,

            /// <summary>
            /// バルーンアタック
            /// </summary>
            BalloonAttack = 0x800,

            /// <summary>
            /// かえる変身
            /// </summary>
            FrogTransform = 0x1000,

            /// <summary>
            /// 武器を構えたままアニメーションしなくなる
            /// </summary>
            StopWithWeapon = 0x2000,

            /// <summary>
            /// 消える（動ける）
            /// </summary>
            Disappear = 0x4000,

            /// <summary>
            /// 烈火の怒りの命中時タイマー左側
            /// </summary>
            AngerOfAFieryFireTimer = 0x8000,

            /// <summary>
            /// 烈火の怒りタイマー右側+透明
            /// </summary>
            AngerOfAFieryFireTransparent = 0x10000,

            /// <summary>
            /// status_dark_weapon.sad
            /// </summary>
            Unk_status_dark_weapon = 0x20000,

            /// <summary>
            /// アタックインターセプター（戦士のスキル）
            /// </summary>
            AttackInterceptor = 0x40000,

            /// <summary>
            /// 魔の約定（悪魔のスキル）
            /// </summary>
            EvilCommitment = 0x80000,

            /// <summary>
            /// 血の盟約（悪魔のスキル）
            /// </summary>
            BloodCovenant = 0x100000,

            /// <summary>
            /// 魂の契約（悪魔のスキル）
            /// </summary>
            SoulCovenant = 0x200000,

            /// <summary>
            /// なし
            /// </summary>
            None_2 = 0x400000,

            /// <summary>
            /// デュエリング,タゲ取り,特技命令
            /// </summary>
            TakeTarget = 0x800000,

            /// <summary>
            /// 泡
            /// </summary>
            Awa = 0x1000000,

            /// <summary>
            /// なし
            /// </summary>
            None_3 = 0x2000000,

            /// <summary>
            /// 青い　動ける
            /// </summary>
            Cold = 0x4000000,

            /// <summary>
            /// 半透明
            /// </summary>
            Blur = 0x8000000,

            /// <summary>
            /// 815_hit.sad
            /// </summary>
            Unk_815_hit = 0x10000000,

            /// <summary>
            /// ゾンビメーカー（ドラコリッチ　ゾンビ化・即死マーク）
            /// </summary>
            ZombieMaker = 0x20000000,

            /// <summary>
            /// 腕斬り,武器使用不可
            /// </summary>
            ArmSlashing = 0x40000000,

            /// <summary>
            /// 胴斬り（継続？）
            /// </summary>
            TornDown = 0x80000000,
        }

        /// <summary>
        /// 闘士・霊術死・光奏師・変身など +0x300
        /// </summary>
        [Flags]
        public enum ActorAnim5 : uint
        {
            /// <summary>
            /// アセンブル
            /// </summary>
            Assemble = 0x01,

            /// <summary>
            /// リバレイト（足元の模様）
            /// </summary>
            Rebalate = 0x02,

            /// <summary>
            /// 神隠し（透明になる）
            /// </summary>
            SpiritedAway = 0x04,

            /// <summary>
            /// クルーエルソウル
            /// </summary>
            CruelSeoul = 0x08,

            /// <summary>
            /// ソウルブレイズ
            /// </summary>
            SeoulBlaze = 0x10,

            /// <summary>
            /// なし
            /// </summary>
            None_0 = 0x20,

            /// <summary>
            /// 真空切り（沈黙状態。パッシブスキル不可）
            /// </summary>
            VacuumCutting = 0x40,

            /// <summary>
            /// 混乱
            /// </summary>
            Confuse = 0x80,

            /// <summary>
            /// ポゼッションブル
            /// </summary>
            PossessionBull = 0x100,

            /// <summary>
            /// ポゼッションホーク
            /// </summary>
            PossessionHawk = 0x200,

            /// <summary>
            /// ポゼッションベア
            /// </summary>
            PossessionBear = 0x400,

            /// <summary>
            /// ポゼッションピューマ
            /// </summary>
            PossessionPuma = 0x800,

            /// <summary>
            /// ポゼッションスネーク
            /// </summary>
            PossessionSnake = 0x1000,

            /// <summary>
            /// 815_hit.sad
            /// </summary>
            Unk_815_hit = 0x2000,

            /// <summary>
            /// なし
            /// </summary>
            None_1 = 0x4000,

            /// <summary>
            /// なし
            /// </summary>
            None_2 = 0x8000,

            /// <summary>
            /// 胴斬り（継続？）
            /// </summary>
            TornDown = 0x10000,

            /// <summary>
            /// 大熊拳（闘士）
            /// </summary>
            OkumaKen = 0x20000,

            /// <summary>
            /// 狩人に変身
            /// </summary>
            TransformHunter = 0x40000,

            /// <summary>
            /// 盗賊に変身
            /// </summary>
            TransformThief = 0x80000,

            /// <summary>
            /// メイジに変身
            /// </summary>
            TransformMaze = 0x100000,

            /// <summary>
            /// ファミリアに変身
            /// </summary>
            TransformFamiliar = 0x200000,

            /// <summary>
            /// 歩くと残像が発生
            /// </summary>
            Afterimage = 0x400000,

            /// <summary>
            /// 光のカーテン（継続）
            /// </summary>
            LightCurtain = 0x800000,

            /// <summary>
            /// バイタリゼーション（継続）
            /// </summary>
            Vitalization = 0x1000000,

            /// <summary>
            /// アンデッド化
            /// </summary>
            Undead = 0x2000000,

            /// <summary>
            /// 感電（黄色）
            /// </summary>
            ElectricShock = 0x4000000,

            /// <summary>
            /// なし
            /// </summary>
            None_3 = 0x8000000,

            /// <summary>
            /// なし
            /// </summary>
            None_4 = 0x10000000,

            /// <summary>
            /// なし
            /// </summary>
            None_5 = 0x20000000,

            /// <summary>
            /// なし
            /// </summary>
            None_6 = 0x40000000,

            /// <summary>
            /// なし
            /// </summary>
            None_7 = 0x80000000,
        }

        /// <summary>
        /// プレイヤーのみ可能なAnim +0x23BA
        /// </summary>
        [Flags]
        public enum ActorAnim6 : ushort
        {
            None = 0xFFFF,

            /// <summary>
            /// テレポーテーション
            /// </summary>
            Teleportation = 0x01,

            /// <summary>
            /// ドラツイ
            /// </summary>
            DragonTwister = 0x02,

            /// <summary>
            /// ファイナルチャージング
            /// </summary>
            FinalCharging = 0x04,

            /// <summary>
            /// なし
            /// </summary>
            None_0 = 0x08,

            /// <summary>
            /// サザンクロス
            /// </summary>
            SouthernCross = 0x10,

            /// <summary>
            /// 黄色の爆発？不明
            /// </summary>
            Unk_Yellow_Bomb = 0x20,

            /// <summary>
            /// アイテム使用
            /// </summary>
            UseItem = 0x40,

            /// <summary>
            /// ブーメランシールド
            /// </summary>
            BoomerangShield = 0x80,

            /// <summary>
            /// 白い玉？不明
            /// </summary>
            Unk_White_ball = 0x100,

            /// <summary>
            /// 
            /// </summary>
            A = 0x200,

            /// <summary>
            /// 
            /// </summary>
            B = 0x400,

            /// <summary>
            /// 
            /// </summary>
            C = 0x800,
        }

        /// <summary>
        /// 職業型
        /// </summary>
        public struct JobType
        {
            public int this[Player.JOB index]
            {
                get
                {
                    switch (index)
                    {
                        case Player.JOB.Swordsman:
                            return Swordsman;
                        case Player.JOB.Warrior:
                            return Warrior;
                        case Player.JOB.Wizard:
                            return Wizard;
                        case Player.JOB.Wolfman:
                            return Wolfman;
                        case Player.JOB.Bishop:
                            return Bishop;
                        case Player.JOB.Angel:
                            return Angel;
                        case Player.JOB.Thief:
                            return Thief;
                        case Player.JOB.Monk:
                            return Monk;
                        case Player.JOB.Lancer:
                            return Lancer;
                        case Player.JOB.Archer:
                            return Archer;
                        case Player.JOB.Tamer:
                            return Tamer;
                        case Player.JOB.Summoner:
                            return Summoner;
                        case Player.JOB.Princess:
                            return Princess;
                        case Player.JOB.LittleWitch:
                            return LittleWitch;
                        case Player.JOB.Necro:
                            return Necro;
                        case Player.JOB.Demon:
                            return Demon;
                        case Player.JOB.NumerologyTeacher:
                            return NumerologyTeacher;
                        case Player.JOB.Fighter:
                            return Fighter;
                        case Player.JOB.LightMaster:
                            return LightMaster;
                        default:
                            throw new ArgumentException($"存在しない職業:{index}");
                    }
                }
                set
                {
                    switch (index)
                    {
                        case Player.JOB.Swordsman:
                            Swordsman = value;
                            break;
                        case Player.JOB.Warrior:
                            Warrior = value;
                            break;
                        case Player.JOB.Wizard:
                            Wizard = value;
                            break;
                        case Player.JOB.Wolfman:
                            Wolfman = value;
                            break;
                        case Player.JOB.Bishop:
                            Bishop = value;
                            break;
                        case Player.JOB.Angel:
                            Angel = value;
                            break;
                        case Player.JOB.Thief:
                            Thief = value;
                            break;
                        case Player.JOB.Monk:
                            Monk = value;
                            break;
                        case Player.JOB.Lancer:
                            Lancer = value;
                            break;
                        case Player.JOB.Archer:
                            Archer = value;
                            break;
                        case Player.JOB.Tamer:
                            Tamer = value;
                            break;
                        case Player.JOB.Summoner:
                            Summoner = value;
                            break;
                        case Player.JOB.Princess:
                            Princess = value;
                            break;
                        case Player.JOB.LittleWitch:
                            LittleWitch = value;
                            break;
                        case Player.JOB.Necro:
                            Necro = value;
                            break;
                        case Player.JOB.Demon:
                            Demon = value;
                            break;
                        case Player.JOB.NumerologyTeacher:
                            NumerologyTeacher = value;
                            break;
                        case Player.JOB.Fighter:
                            Fighter = value;
                            break;
                        case Player.JOB.LightMaster:
                            LightMaster = value;
                            break;
                        default:
                            throw new ArgumentException($"存在しない職業:{index}");
                    }
                }
            }

            public int Swordsman { get; set; }
            public int Warrior { get; set; }
            public int Wizard { get; set; }
            public int Wolfman { get; set; }
            public int Bishop { get; set; }
            public int Angel { get; set; }
            public int Thief { get; set; }
            public int Monk { get; set; }
            public int Lancer { get; set; }
            public int Archer { get; set; }
            public int Tamer { get; set; }
            public int Summoner { get; set; }
            public int Princess { get; set; }
            public int LittleWitch { get; set; }
            public int Necro { get; set; }
            public int Demon { get; set; }

            /// <summary>
            /// 霊術師
            /// </summary>
            public int NumerologyTeacher { get; set; }

            /// <summary>
            /// 闘士
            /// </summary>
            public int Fighter { get; set; }

            /// <summary>
            /// 光奏師
            /// </summary>
            public int LightMaster { get; set; }
        }

        /// <summary>
        /// 状態異常
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct StatusAbnormal
        {
            /// <summary>
            /// 暗闇
            /// 目くらまし攻撃
            /// </summary>
            public short Darkness;

            /// <summary>
            /// 毒
            /// </summary>
            public short Poison;

            /// <summary>
            /// 睡眠
            /// 睡眠攻撃
            /// </summary>
            public short Sleep;

            /// <summary>
            /// コールド
            /// コールド攻撃
            /// </summary>
            public short Cold;

            /// <summary>
            /// フリーズ
            /// フリーズ攻撃
            /// </summary>
            public short Freeze;

            /// <summary>
            /// スタン
            /// スタン攻撃
            /// </summary>
            public short Stun;

            /// <summary>
            /// 石化
            /// 石化攻撃
            /// </summary>
            public short Petrification;

            /// <summary>
            /// 混乱
            /// 混乱攻撃
            /// </summary>
            public short Confusion;

            /// <summary>
            /// 魅了
            /// チャーミング攻撃
            /// </summary>
            public short Fascination;

            public override string ToString()
                => $"Da:{Darkness}, P:{Poison}, Sl:{Sleep}, Col:{Cold}, Fr:{Freeze}, St:{Stun}, M:{Petrification}, Con:{Confusion}, Fa:{Fascination}";
        }

        /// <summary>
        /// 状態異常（ジェネリック）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public struct StatusAbnormal<T> where T : IFormattable
        {
            /// <summary>
            /// 暗闇
            /// 目くらまし攻撃
            /// </summary>
            public T Darkness;

            /// <summary>
            /// 毒
            /// </summary>
            public T Poison;

            /// <summary>
            /// 睡眠
            /// 睡眠攻撃
            /// </summary>
            public T Sleep;

            /// <summary>
            /// コールド
            /// コールド攻撃
            /// </summary>
            public T Cold;

            /// <summary>
            /// フリーズ
            /// フリーズ攻撃
            /// </summary>
            public T Freeze;

            /// <summary>
            /// スタン
            /// スタン攻撃
            /// </summary>
            public T Stun;

            /// <summary>
            /// 石化
            /// 石化攻撃
            /// </summary>
            public T Petrification;

            /// <summary>
            /// 混乱
            /// 混乱攻撃
            /// </summary>
            public T Confusion;

            /// <summary>
            /// 魅了
            /// チャーミング攻撃
            /// </summary>
            public T Fascination;

            public override string ToString()
                => $"Da:{Darkness}, P:{Poison}, Sl:{Sleep}, Col:{Cold}, Fr:{Freeze}, St:{Stun}, M:{Petrification}, Con:{Confusion}, Fa:{Fascination}";
        }

        /// <summary>
        /// 低下・上昇するステータス
        /// </summary>
        public struct RateState
        {
            /// <summary>
            /// 攻撃力%
            /// </summary>
            public int Damage;

            /// <summary>
            /// 防御力%
            /// </summary>
            public int Defense;

            /// <summary>
            /// 命中率
            /// </summary>
            public int Accuracy;

            /// <summary>
            /// 回避率
            /// </summary>
            public int Avoidance;

            /// <summary>
            /// 移動速度
            /// </summary>
            public int MovingSpeed;

            /// <summary>
            /// 攻撃速度
            /// </summary>
            public int AttackSpeed;
            
            /// <summary>
            /// 属性抵抗
            /// </summary>
            public Magic<int> AttributeResistance;
            
            /// <summary>
            /// 集中力%
            /// </summary>
            public int Concentration;
        }

        /// <summary>
        /// 呪い系
        /// </summary>
        public struct ActorSpell<T> : IFormattable
            where T :IFormattable
        {
            /// <summary>
            /// 呪い攻撃？　
            /// </summary>
            public T Spell;

            /// <summary>
            /// 武器破壊
            /// </summary>
            public T WeaponDestroy;

            /// <summary>
            /// 鎧破壊
            /// </summary>
            public T ArmorDestroy;

            /// <summary>
            /// 優先ターゲット
            /// </summary>
            public T PreferredTarget;

            /// <summary>
            /// ベルセルク
            /// </summary>
            public T Berserk;

            /// <summary>
            /// AI知能低下
            /// </summary>
            public T IntelligenceDecline;

            /// <summary>
            /// レベル低下
            /// </summary>
            public T Level;

            public override string ToString()
                => $"Spell:{Spell}, WD:{WeaponDestroy}, AD:{ArmorDestroy}, PT:{PreferredTarget}, Berserk:{Berserk}, ID:{IntelligenceDecline}, Level:{Level}";

            public string ToString(string format, IFormatProvider formatProvider)
                => ToString();
        }

        /// <summary>
        /// Actor弱点
        /// </summary>
        public struct PlayerStatusWeakness
        {
            /// <summary>
            /// 即死
            /// </summary>
            public int InstantDeath;

            /// <summary>
            /// ノックバック
            /// </summary>
            public int KnockBack;

            /// <summary>
            /// 致命打
            /// </summary>
            public int FatalHit;

            /// <summary>
            /// 決定打
            /// </summary>
            public int DeterminationHit;
        }

        /// <summary>
        /// 種族型
        /// </summary>
        public struct ActorRaceType
        {
            public int this[int i]
            {
                get
                {
                    switch (i)
                    {
                        case 0:
                            return Undead;
                        case 1:
                            return Devil;
                        case 2:
                            return Human;
                        case 3:
                            return Animal;
                        case 4:
                            return GodAnimal;
                        default:
                            throw new ArgumentOutOfRangeException("定義外のRace");
                    }
                }
            }

            /// <summary>
            /// アンデッド型
            /// </summary>
            public int Undead { get; set; }

            /// <summary>
            /// 悪魔型
            /// </summary>
            public int Devil { get; set; }

            /// <summary>
            /// 人間型
            /// </summary>
            public int Human { get; set; }

            /// <summary>
            /// 動物型
            /// </summary>
            public int Animal { get; set; }

            /// <summary>
            /// 神獣型
            /// </summary>
            public int GodAnimal { get; set; }
        }
    }
}
