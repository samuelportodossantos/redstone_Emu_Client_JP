using RedStoneLib.Model.Base;
using RedStoneLib.Model.Effect;
using RedStoneLib.Packets;
using RedStoneLib.Packets.RSPacket.Action;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RedStoneLib.Model
{
    public class Skill
    {
        /// <summary>
        /// 全てのスキル
        /// </summary>
        public static Dictionary<ushort, Skill> AllSkills { get; private set; }

        /// <summary>
        /// 同じスキルのグループ（同じスキルがプレイヤー用とモンスター用にわけられているものなど）
        /// </summary>
        public static ILookup<ushort, Skill> SameSkillGroups { get; private set; }

        /// <summary>
        /// すべて読み込み
        /// </summary>
        public static void Load()
        {
            using (PacketReader br = new PacketReader(Helper.StreamFromAssembly("skill2.dat")))
            {
                //復号化キーチェック１
                if (br.ReadUInt32() == 0xABCD1234 && br.ReadUInt32() < 1)
                {
                    br.SetDataEncodeTable(-1);
                }

                int skillSize = br.ReadInt32();

                //１でキーがなかった場合の復号化キーチェック
                if (!br.NeedDecrypt)
                {
                    br.SetDataEncodeTable(br.ReadInt32());
                }


                //スキル全て読み込み・最大800だが以下全てvalid
                List<Skill> skills = Enumerable.Range(0, br._Decryption(skillSize)).Select(_ => br.EncryptionReads<byte>(0x82C))
                    .Select(data => new Skill(data)).ToList();

                //全てのスキル
                AllSkills = skills.Where(t => t.Index != ushort.MaxValue).ToDictionary(t => t.Index, t => t);

                var target = AllSkills[713];

                //FamilyIndexの重複を探す
                SameSkillGroups = skills.ToLookup(t => t.FamilyIndex);

                var test = skills.Select(t => t.m_value).OrderBy(t => t.Index).ToList();

                var test_ = test.Where(t => t.Job > Player.JOB.LightMaster).GroupBy(t => t.Name).Where(t => t.Count() > 1).ToList();

                //var test3 = test.Where(t => t.SkillAbilitys.Any(u => u.AbilityIndex == 4)).OrderBy(t=>(ushort)t.Job).ToList();

                var atype = test.GroupBy(t => t.MakePacketType).OrderBy(t => t.Key).Select(t => new { key = t.Key, value = t.ToArray() }).ToArray();

                var unk00 = Enumerable.Range(0, test[0].Unknown_00.Length).Select(n => test.GroupBy(t => t.Unknown_00[n]).OrderBy(t => t.Key).Select(t => new { key = t.Key, value = t.ToArray() }).ToArray()).ToArray();
                var unk01 = Enumerable.Range(0, test[0].Unknown_01.Length).Select(n => test.GroupBy(t => t.Unknown_01[n]).OrderBy(t => t.Key).Select(t => new { key = t.Key, value = t.ToArray() }).ToArray()).ToArray();

                var passive = test.GroupBy(t => t.PassiveType).ToDictionary(t => t.Key, t => t.ToArray());

                //アクションスキル
                var sflag = ((IEnumerable<SkillUsageFlag>)Enum.GetValues(typeof(SkillUsageFlag)))
                    .Select(t => new { flag = t, skills = test.Where(u => u.UsageFlag.HasFlag(t)).ToArray() }).ToList();
                var sflag0 = test.Where(t => t.UsageFlag == 0).ToList();

                //対象知識スキル
                var mflag = ((IEnumerable<SkillDamageFlag>)Enum.GetValues(typeof(SkillDamageFlag)))
                    .Select(t => new { flag = t, skills = test.Where(u => u.DamageFlag.HasFlag(t)).ToArray() }).ToList();

                //対象種族
                var rflag = ((IEnumerable<SkillTargetRace>)Enum.GetValues(typeof(SkillTargetRace)))
                    .Select(t => new { flag = t, skills = test.Where(u => u.TargetRaceFlag.HasFlag(t)).ToArray() }).ToList();

                var test6 = test.GroupBy(t => t.junk_5).OrderBy(t => t.Key).ToDictionary(t => t.Key, t => t.ToList());

                var client_data = SearchAbilityIndex(test);
#if MAKEENUM
                //共用体作成
                var isiage_data = GetAbilityName();
                var s_abilities = client_data.ToDictionary(t => (ushort)t.index, t => isiage_data.TryGetValue(t.index, out var res) ? res.Replace("\\", "") : "INVALID");
                Helper.EnumFieldMaker("AbilityType", s_abilities);
#endif
            }
        }

        /// <summary>
        /// AbilityIndexについての情報を引き出す
        /// </summary>
        /// <param name="skills"></param>
        /// <returns></returns>
        private static List<(int index, List<(string name, uint difficult, Player.JOB job, int opIndex, SkillAbility op, SkillAbility[] opall)>)>
            SearchAbilityIndex(List<SkillInfo> skills)
        {
            int maxval = (int)skills.SelectMany(t => t.SkillAbilitys.Select(u => u.AbilityIndex)).Where(t => t != SkillAbility.AbilityType.None).Max();

            List<(int index, List<(string name, uint difficult, Player.JOB job, int opIndex, SkillAbility op, SkillAbility[] opall)>)> result
                = new List<(int index, List<(string name, uint difficult, Player.JOB job, int opIndex, SkillAbility op, SkillAbility[] opall)>)>();
            Parallel.For(0, maxval, i =>
            {
                //AbilityIndex=iのスキル情報抽出
                IEnumerable< (string name, uint difficult, Player.JOB job, int opIndex, SkillAbility op, SkillAbility[] opall)> getval()
                {
                    foreach(var targetSkill in skills.Where(t => t.SkillAbilitys.Any(u => (int)u.AbilityIndex == i)).OrderBy(t=>(ushort)t.Job))
                    {
                        var targetAbilityOP = targetSkill.SkillAbilitys
                        .Select((v,index)=>(v,index))
                        .Where(t=>(int)t.Item1.AbilityIndex==i)
                        .First();
                        yield return (targetSkill.Name, targetSkill.Difficulty, targetSkill.Job, targetAbilityOP.Item2 + 1, targetAbilityOP.Item1, targetSkill.SkillAbilitys);
                    }
                }

                lock (result)
                {
                    result.Add((i, getval().ToList()));
                }

            });
            return result.OrderBy(t => t.index).ToList();
        }

#if MAKEENUM
        /// <summary>
        /// いしあげからデータ取ってくる
        /// </summary>
        /// <returns></returns>
        private static Dictionary<int, string> GetAbilityName()
        {
            Dictionary<int, List<string>> investigation = new Dictionary<int, List<string>>();
            foreach (var htmlPath in Directory.EnumerateFiles(@"htmls\", "*.html"))
            {
                //player
                HtmlDocument html = new HtmlDocument();

                //html読み込み
                html.LoadHtml(File.ReadAllText(htmlPath, Encoding.GetEncoding("euc-jp")));

                foreach(var node in html.DocumentNode.SelectNodes("//span"))
                {
                    int ID = Convert.ToInt32(Regex.Match(node.Attributes[0].Value, @"\d+").Value);
                    if (!investigation.ContainsKey(ID)) investigation[ID] = new List<string>();
                    investigation[ID].Add(node.InnerText);
                }
            }

            //IDと名前リストの辞書に変換
            investigation = investigation.ToDictionary(t => t.Key, t => t.Value.Select(u => Regex.Match(u, @"[^:]+").Value).ToList());

            //check
            return investigation.ToDictionary(t => t.Key, t =>
            {
                int key = t.Key;
                string represent = investigation[key][0];
                if (!investigation[key].All(u => u == represent))
                {
                    switch (key)
                    {
                        case 200:
                            return investigation[key][0];
                        case 199:
                            return investigation[key][1];
                        default:
                            throw new InvalidOperationException("指定外の重複");
                    }
                }
                return investigation[key][0];
            }).OrderBy(t => t.Key).ToDictionary(t => t.Key, t => t.Value.Replace("#", ""));
        }
#endif

        /// <summary>
        /// skill実態
        /// </summary>
        SkillInfo m_value;

        /// <summary>
        /// 値型から構築
        /// </summary>
        /// <param name="sinfo"></param>
        private Skill(byte[] data)
        {
            m_value = new SkillInfo(data);
        }

        /// <summary>
        /// Index（順番通り）
        /// </summary>
        public ushort Index
            => m_value.Index;

        /// <summary>
        /// 同じスキルのグループのインデックス
        /// </summary>
        public ushort FamilyIndex
            => m_value.FamilyIndex;

        /// <summary>
        /// スキルの種類
        /// </summary>
        public SkillMakePacketType MakePacketType
            => m_value.MakePacketType;

        /// <summary>
        /// 所有する職業
        /// </summary>
        public Player.JOB Job
            => m_value.Job;

        /// <summary>
        /// スキル名
        /// </summary>
        public string Name
            => m_value.Name;

        /// <summary>
        /// スキル難易度
        /// </summary>
        public uint Difficulty
            => m_value.Difficulty;

        /// <summary>
        /// パッシブ・非パッシブの攻撃補助
        /// </summary>
        public SkillPassiveType PassiveType
            => m_value.PassiveType;

        /// <summary>
        /// ダメージのタイプ
        /// </summary>
        public SkillDamageFlag DamageFlag
            => m_value.DamageFlag;

        /// <summary>
        /// スキルの対象種族
        /// </summary>
        public SkillTargetRace TargetRaceFlag
            => m_value.TargetRaceFlag;

        /// <summary>
        /// 使用先フラグ
        /// </summary>
        public SkillUsageFlag UsageFlag
            => m_value.UsageFlag;

        /// <summary>
        /// 消費CP
        /// </summary>
        public SkillExpression LostCP
            => m_value.LostCP;

        /// <summary>
        /// 獲得CP
        /// </summary>
        public SkillExpression GetCP
            => m_value.GetCP;

        /// <summary>
        /// 範囲スキルの範囲
        /// </summary>
        public SkillExpression Range
            => m_value.Range;

        /// <summary>
        /// ダメージ減少値/距離　係数
        /// </summary>
        public SkillExpression RangeDecreaseCoef
            => m_value.RangeDecreaseCoef;

        /// <summary>
        /// 範囲ダメージの最小値・最大値
        /// </summary>
        public Scale<ushort> RangeDamageAmpRateScale
            => m_value.RangeDamageAmpRateScale;

        /// <summary>
        /// クールタイム
        /// </summary>
        public SkillExpression CoolTime
            => m_value.CoolTime;

        /// <summary>
        /// 物理ダメージ+(%)
        /// </summary>
        public SkillExpression DamagePercent
            => m_value.DamagePercent;

        /// <summary>
        /// 防御力+
        /// </summary>
        public SkillExpression Defence
            => m_value.Defence;

        /// <summary>
        /// 最大ブロック率+(%)
        /// </summary>
        public SkillExpression MaxBlockRate
            => m_value.MaxBlockRate;

        /// <summary>
        /// 知識ダメージ
        /// </summary>
        public SkillInteligenceDamage[] SkillInteligenceDamages
            => m_value.SkillInteligenceDamages;

        /// <summary>
        /// アビリティ
        /// </summary>
        public SkillAbility[] SkillAbilitys
            => m_value.SkillAbilitys;

        /// <summary>
        /// 攻撃速度（Frame）
        /// </summary>
        public SkillExpression Frame
            => m_value.Frame;

        /// <summary>
        /// 攻撃速度+(%)
        /// </summary>
        public SkillExpression AttackSpeed
            => m_value.AttackSpeed;

        /// <summary>
        /// 最大射程距離（タゲをとれる最大の距離）
        /// </summary>
        public SkillExpression MaxEnableRange
            => m_value.MaxEnableRange;

        /// <summary>
        /// 効果範囲距離*100[m]
        /// </summary>
        public SkillExpression EffectRange
            => m_value.EffectRange;

        /// <summary>
        /// 命中率(%)
        /// </summary>
        public SkillExpression HitRate
            => m_value.HitRate;

        /// <summary>
        /// 回避率(%)
        /// </summary>
        public SkillExpression AvoidRate
            => m_value.AvoidRate;

        /// <summary>
        /// 致命打率(%)
        /// </summary>
        public SkillExpression FatalAttackRate
            => m_value.FatalAttackRate;

        /// <summary>
        /// 決定打率(%)
        /// </summary>
        public SkillExpression CriticalAttackRate
            => m_value.CriticalAttackRate;

        /// <summary>
        /// 種族決定打率(%)　アンデット，悪魔，...
        /// </summary>
        public SkillExpression[] RaceCriticalAttackRate
            => m_value.RaceCriticalAttackRate;

        /// <summary>
        /// 種族即死確率(%)　アンデット，悪魔，...
        /// </summary>
        public SkillExpression[] RaceInstantDeathRate
            => m_value.RaceInstantDeathRate;

        /// <summary>
        /// ブロック率+(%)
        /// </summary>
        public SkillExpression BlockRate
            => m_value.BlockRate;

        /// <summary>
        /// 麻痺抵抗(%)
        /// </summary>
        public SkillExpression StunResistance
            => m_value.StunResistance;

        /// <summary>
        /// 異常系抵抗(%)
        /// </summary>
        public SkillExpression AbnormalResistance
            => m_value.AbnormalResistance;

        /// <summary>
        /// 敵攻撃限界回数（GPなど）
        /// </summary>
        public SkillExpression EnemyAttackLimit
            => m_value.EnemyAttackLimit;

        /// <summary>
        /// ターゲット数
        /// </summary>
        public SkillExpression TargetNumber
            => m_value.TargetNumber;

        /// <summary>
        /// 攻撃回数（分身人数）
        /// </summary>
        public SkillExpression AttackCount
            => m_value.AttackCount;

        /// <summary>
        /// 持続時間（秒）
        /// </summary>
        public SkillExpression DurationTime
            => m_value.DurationTime;

        /// <summary>
        /// 発動確率(%)
        /// </summary>
        public SkillExpression InvokeProb
            => m_value.InvokeProb;

        /// <summary>
        /// 基礎発動確率(&)
        /// </summary>
        public SkillExpression BasicInvokeProb
            => m_value.BasicInvokeProb;

        /// <summary>
        /// 要求スキル
        /// </summary>
        public RequireSkill[] RequireSkills
            => m_value.RequireSkills;

        /// <summary>
        /// 説明文
        /// </summary>
        public string Description
            => m_value.Description;

        /// <summary>
        /// 上昇形態の説明
        /// </summary>
        public string ElevatedForm
            => m_value.ElevatedForm;
        
        /// <summary>
        /// 回避率取得
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public double GetTargetAvoidance(Actor attacker, Actor target)
        {
            PlayerEffect ate = (attacker as Player)?.Effect ?? null;
            PlayerEffect tae = (target as Player)?.Effect ?? null;

            //敏捷補正値[%]
            double agilityCorrection = target.Status.Agility <= attacker.Status.Agility ?
                (target.Status.Agility - attacker.Status.Agility) / 25.0 :
                24.0 * Math.Log((double)target.Status.Agility / attacker.Status.Agility);

            //Lv差補正値[%]
            int lvlDiff = target.Level - attacker.Level;
            double levelCorrection = Math.Min(0.003 * lvlDiff * lvlDiff + 0.2 * lvlDiff, 80);
            if (lvlDiff < 0) levelCorrection *= -1;

            //回避OP補正[%]
            int opCorrection = tae?.AbilityRate.Avoidance ?? 0;

            //回避補正無視
            bool ignoreTargetAvoidance = ate?.IgnoreTargetAvoidanceRatioCorrectionValue > 0;

            //命中補正無視
            opCorrection -= tae?.IgnoreHitRateCorrectionValueOfAttacker > 0 ? -ate?.AbilityRate.Accuracy ?? 0 : 0;

            //基準回避率[%]
            double standard = (ignoreTargetAvoidance ? 0 : agilityCorrection + opCorrection) + levelCorrection;

            //追加回避補正
            double luckDiff = (target.Status.Luckey - attacker.Status.Luckey) / 50;
            double staRate = standard / 100.0;
            double additional = Math.Exp((0.058* staRate* staRate-0.15* staRate+0.0935) *Math.Abs(luckDiff));
            if (luckDiff < 0) additional = 1 / additional;

            return standard * additional;
        }
        
        /// <summary>
        /// 何かしらの魔法ダメージを含むか
        /// </summary>
        public bool HasSomeMagicDamage
            => ((int)m_value.DamageFlag & 0x7E) != 0;

        public override string ToString() => Name;

        /// <summary>
        /// スキル力=A×SLv+B　となるA,B
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SkillExpression
        {
            /// <summary>
            /// リテラル
            /// </summary>
            public readonly List<short> Literals;

            /// <summary>
            /// 計算式
            /// </summary>
            Func<int, float> Exp;

            /// <summary>
            /// 基本
            /// </summary>
            /// <param name="b"></param>
            /// <param name="a"></param>
            private SkillExpression(short a, short b)
            {
                TypeMismatch = (ushort)a == ushort.MaxValue && (ushort)b == ushort.MaxValue;
                ExpressionUnavailable = a == 0 && b == 0;
                ConstUnavailable = true;
                SLvMaxLimitUnavailable = true;

                if (ExpressionUnavailable)
                {
                    Exp = t => 0;
                }
                else
                {
                    float A = a / (float)100.0;
                    if (b == 0)
                    {
                        Exp = _ => A;
                    }
                    else
                    {
                        float B = b / (float)100.0;
                        Exp = t => A + B * t;
                    }
                }

                Literals = new List<short> { a, b };
            }

            /// <summary>
            /// Value制限付き
            /// </summary>
            /// <param name="b"></param>
            /// <param name="a"></param>
            /// <param name="c"></param>
            private SkillExpression(short a, short b, short c) : this(a, b)
            {
                ConstUnavailable = c == 0;

                if (!ConstUnavailable)
                {
                    if (ExpressionUnavailable)
                    {
                        float C = c / (float)100.0;
                        Exp = _ => C;
                    }
                    else
                    {
                        float C = c / (float)100.0;
                        Func<int, float> oldexp = (Func<int, float>)Exp.Clone();
                        Exp = t => Math.Max(oldexp(t), C);
                    }
                }
                Literals.Add(c);
            }

            /// <summary>
            /// SLv制限付き
            /// </summary>
            /// <param name="b"></param>
            /// <param name="a"></param>
            /// <param name="c"></param>
            /// <param name="d"></param>
            private SkillExpression(short a, short b, short c, ushort d) : this(a, b)
            {
                ConstUnavailable = c == 0;
                SLvMaxLimitUnavailable = d == 0;
                if (!SLvMaxLimitUnavailable && !ExpressionUnavailable)
                {
                    if (ConstUnavailable)
                    {
                        float valueMax = d / (float)100.0;
                        Func<int, float> oldexp = (Func<int, float>)Exp.Clone();
                        Exp = t => Math.Min(oldexp(t), valueMax);
                    }
                    else
                    {
                        int slvMax = 100 / d;
                        Func<int, float> oldexp = (Func<int, float>)Exp.Clone();
                        Exp = t => oldexp(Math.Min(t, slvMax));
                    }
                }
                Literals.Add((short)d);
            }

            /// <summary>
            /// SkillLevelから値を求める
            /// </summary>
            /// <param name="slv"></param>
            /// <returns></returns>
            public float this[int slv]
                => Exp(slv);

            /// <summary>
            /// 式利用不可能フラグ
            /// </summary>
            public readonly bool ExpressionUnavailable;

            /// <summary>
            /// 定数使用不可能フラグ
            /// </summary>
            public readonly bool ConstUnavailable;

            /// <summary>
            /// SLv最大値使用不可能フラグ
            /// </summary>
            public readonly bool SLvMaxLimitUnavailable;

            /// <summary>
            /// 恐らく型が[SkillExpression]ではない
            /// </summary>
            public readonly bool TypeMismatch;

            public override string ToString()
            {
                if (ConstUnavailable && ExpressionUnavailable) return nameof(ExpressionUnavailable);
                if (TypeMismatch) return nameof(TypeMismatch);
                string[] result = new string[10];
                for (int i = 0; i < 10; i++)
                    result[i] = $"{i + 1}:[{this[i + 1]}]";
                return string.Join(", ", result);
            }
        }

        /// <summary>
        /// 知識攻撃
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SkillInteligenceDamage
        {
            /// <summary>
            /// 計算の項
            /// </summary>
            private readonly double MinWeight, MinBias, MaxWeight, MaxBias;

            /// <summary>
            /// 恐らく属性特有効果（風：麻痺など）の効果時間？
            /// </summary>
            public readonly short Unknown;

            /// <summary>
            /// 利用不可能フラグ
            /// </summary>
            public readonly bool Unavailable;

            /// <summary>
            /// 物理ダメージ倍率（％）
            /// </summary>
            public readonly SkillExpression PhysicDamageMagnification;

            /// <summary>
            /// 自動コンストラクタ用
            /// </summary>
            /// <param name="biasAve">バイアス項・平均値</param>
            /// <param name="weightAve">重み・平均値</param>
            /// <param name="biasVer">バイアス項・分散</param>
            /// <param name="weightVer">重み・分散</param>
            private SkillInteligenceDamage(short biasAve, short weightAve, short biasVer, short weightVer, short b, short a, short unk3)
            {
                Unavailable = biasAve == 0 && weightAve == 0 && biasVer == 0 && weightVer == 0 && b == 0 && a == 0;

                MinWeight = (weightAve - weightVer) / 100.0;
                MaxWeight = (weightAve + weightVer) / 100.0;

                MinBias = (biasAve - biasVer) / 100.0;
                MaxBias = (biasAve + biasVer) / 100.0;

                PhysicDamageMagnification = (SkillExpression)typeof(SkillExpression).GetConstructors(BindingFlags.NonPublic|BindingFlags.Instance).First().Invoke(new object[] { b, a });

                Unknown = unk3;
            }

            /// <summary>
            /// 最小ダメージ
            /// </summary>
            /// <param name="slv"></param>
            /// <returns></returns>
            public double Min(int slv) => MinBias + slv * MinWeight;

            /// <summary>
            /// 最大ダメージ
            /// </summary>
            /// <param name="slv"></param>
            /// <returns></returns>
            public double Max(int slv) => MaxBias+ slv * MaxBias;

            /// <summary>
            /// ランダムダメージ
            /// </summary>
            /// <param name="slv"></param>
            /// <returns></returns>
            public double Random(int slv)
                => Min(slv) + Helper.StaticRandom.NextDouble() * Max(slv);

            public override string ToString()
            {
                if (Unavailable) return nameof(Unavailable);
                if (!PhysicDamageMagnification.ExpressionUnavailable) return PhysicDamageMagnification.ToString();
                string[] result = new string[10];
                for (int i = 0; i < 10; i++)
                    result[i] = $"{i + 1}:[{Min(i + 1)}~{Max(i + 1)}]";
                return string.Join(", ", result);
            }
        }

        /// <summary>
        /// 前提スキル
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct RequireSkill
        {
            /// <summary>
            /// インデックス
            /// </summary>
            ushort Index;

            /// <summary>
            /// レベル
            /// </summary>
            ushort Level;

            /// <summary>
            /// 利用不可能フラグ
            /// </summary>
            public bool Unavailable
                => Index == ushort.MaxValue;

            public override string ToString()
                => Unavailable ? nameof(Unavailable) : $"{AllSkills[Index].Name} Lv{Level}";
        }



        /// <summary>
        /// スキルが使用可能な対象のフラグ　0はパッシブ
        /// </summary>
        [Flags]
        public enum SkillUsageFlag : uint
        {
            /// <summary>
            /// 敵に使用可能
            /// </summary>
            CastEnemy = 0x01,

            /// <summary>
            /// プレイヤーに使用可能
            /// </summary>
            CastPlayer = 0x02,

            /// <summary>
            /// 死体に使用可能
            /// </summary>
            CastDead = 0x04,

            /// <summary>
            /// 地面に使用可能
            /// </summary>
            CastGround = 0x08,

            /// <summary>
            /// クイックキャストスキル
            /// </summary>
            QuickCast = 0x10,

            /// <summary>
            /// 状態変化
            /// </summary>
            StateChange = 0x20,

            Unknown = 0x40,

            /// <summary>
            /// パーティーに支援
            /// </summary>
            PartyBuff = 0x80,

            /// <summary>
            /// ペットに対するスキル
            /// </summary>
            ToPet = 0x100,

            /// <summary>
            /// アンテイム系
            /// </summary>
            Untame = 0x200,

            /// <summary>
            /// ペット復活系
            /// </summary>
            PetResurrection = 0x400,

            /// <summary>
            /// 召喚獣パワーアップ系
            /// </summary>
            SummonedBeast = 0x800,

            /// <summary>
            /// ペットに乗る
            /// </summary>
            RidingPet = 0x1000,

            /// <summary>
            /// トラップ解除
            /// </summary>
            DisarmTrap = 0x2000,

            /// <summary>
            /// ロックピック
            /// </summary>
            LockPick = 0x4000,

            /// <summary>
            /// アンロックドア
            /// </summary>
            UnlockDoor = 0x8000,

            /// <summary>
            /// 武器変身
            /// </summary>
            ChangeWeapon = 0x10000,
        }

        /// <summary>
        /// 対象種族
        /// </summary>
        [Flags]
        public enum SkillTargetRace : ushort
        {
            /// <summary>
            /// アンデッド
            /// </summary>
            Undead = 1,

            /// <summary>
            /// 人間
            /// </summary>
            Human = 2,

            /// <summary>
            /// 悪魔
            /// </summary>
            Devil = 4,

            /// <summary>
            /// 動物
            /// </summary>
            Animal = 8,

            /// <summary>
            /// 神獣
            /// </summary>
            DivineBeast = 16,
        }

        /// <summary>
        /// ダメージの種類
        /// </summary>
        [Flags]
        public enum SkillDamageFlag : ushort
        {
            /// <summary>
            /// 物理
            /// </summary>
            Physic = 1,

            /// <summary>
            /// 火
            /// </summary>
            Fire = 2,

            /// <summary>
            /// 水
            /// </summary>
            Water = 4,

            /// <summary>
            /// 風
            /// </summary>
            Wind = 8,

            /// <summary>
            /// 土
            /// </summary>
            Earth = 16,

            /// <summary>
            /// 光
            /// </summary>
            Light = 32,

            /// <summary>
            /// 闇
            /// </summary>
            Dark = 64,
        }

        /// <summary>
        /// パッシブと攻撃補助
        /// </summary>
        public enum SkillPassiveType : ushort
        {
            /// <summary>
            /// パッシブでなく攻撃系
            /// </summary>
            NotPassiveAttack = 0,

            /// <summary>
            /// パッシブで補助系
            /// </summary>
            PassiveSupport = 1,

            /// <summary>
            /// パッシブで攻撃系
            /// </summary>
            PassiveAttack = 2,

            /// <summary>
            /// パッシブでなく補助系
            /// </summary>
            NotPassveSupport = 3,
        }

        /// <summary>
        /// 生成パケット
        /// </summary>
        public enum SkillMakePacketType : ushort
        {
            /// <summary>
            /// 通常攻撃
            /// </summary>
            BasicAttack = 0,

            /// <summary>
            /// 連続打撃攻撃
            /// </summary>
            ContinuousHitAttack = 1,

            /// <summary>
            /// ラッシュ攻撃
            /// </summary>
            Rush = 2,

            /// <summary>
            /// ディレイ攻撃
            /// </summary>
            Delay = 3,

            /// <summary>
            /// 分身攻撃
            /// </summary>
            BunshineAttack = 4,

            /// <summary>
            /// シミター
            /// </summary>
            ScimitarCutting = 5,
            [Comment("タンクラッシュ")]
            TankRush = 6,
            [Comment("ジャンプ攻撃")]
            JumpAttack = 7,
            [Comment("ワイルドスタンプ")]
            WildStamp = 8,
            [Comment("オルターリングヒッター")]
            AlternatorRingHitter = 9,
            [Comment("サイクロンピーク")]
            CyclonePeak = 10,
            [Comment("ワイルドダンス")]
            WildDance = 11,
            [Comment("旋風突き")]
            Whirlwashing = 12,
            [Comment("サプライジングレイド")]
            SupplyJinGlade = 13,
            [Comment("ガーディアンポスト")]
            GuardianPost = 15,
            [Comment("オーサムフォートレス")]
            AutumnFortress = 17,
            [Comment("バイトハンギング")]
            ByteHanging = 18,
            [Comment("バウンシングリニア")]
            BouncingLinear = 19,
            [Comment("ファイアーボール")]
            Fireball = 20,

            /// <summary>
            /// 通常攻撃（遠距離）
            /// </summary>
            BasicFarAttack = 21,
            [Comment("マシーンアロー")]
            MachineArrow = 22,
            [Comment("ウォーターキャノン")]
            WaterCannon = 23,
            [Comment("ピアシングアロー")]
            PiercingArrow = 24,
            [Comment("スプレッドアロー")]
            SpreadArrow = 26,
            [Comment("ダブルスローイング")]
            DoubleThrowing = 27,
            [Comment("ブーメランシールド")]
            BoomerangShield = 28,
            [Comment("ジャベリンテンペスト")]
            JavelinTempest = 29,
            [Comment("ソニックブロー")]
            SonicBlow = 31,
            [Comment("ストレートスパイク")]
            StraightSpike = 32,
            [Comment("ランドマーカー")]
            LandMarker = 33,
            [Comment("インターバルシューター")]
            IntervalShooter = 34,
            [Comment("グライディングファイアー")]
            GlidingFire = 35,
            [Comment("トワーリングプロテクター")]
            TowerRingProtector = 36,
            [Comment("ヘブンリープレシング")]
            HeavenlyPrecision = 37,
            [Comment("ボイドボウ")]
            BoydBow = 38,
            [Comment("テイルチェイサー")]
            TailChaser = 41,
            [Comment("スピンアライジング")]
            SpinAligning = 42,
            [Comment("デュエリング")]
            Dueling = 44,
            [Comment("ダブルターゲット")]
            DoubleTarget = 45,
            [Comment("ローズガーデン")]
            RoseGarden = 46,
            [Comment("花の乙女スペシャル")]
            FlowerMaidSpecial = 47,
            [Comment("丸太変身")]
            TransformLog = 48,
            [Comment("モンスターバレット")]
            MonsterBarrett = 49,
            [Comment("ポーション投げ")]
            PotionThrow = 50,
            [Comment("ライトニングワインダー")]
            LightningWinder = 51,
            [Comment("ミラーイメージ")]
            MirrorImage = 52,
            [Comment("チェーンライトニング")]
            ChainLightning = 53,
            [Comment("ファイアーウォール")]
            Firewall = 54,
            [Comment("ギガライトニング")]
            GigaLightning = 55,
            [Comment("竜巻起こし")]
            ATornadoRaise = 56,
            [Comment("ソウルブレイズ")]
            SeoulBlaze = 57,
            [Comment("ジャンプ")]
            Jump = 59,
            [Comment("ディメンジョンアーマー")]
            DimensionArmor = 60,
            [Comment("スパイクシールディング")]
            SpikeShielding = 61,
            [Comment("アイススタラグマイト")]
            IceStalagmite = 62,
            [Comment("マッスルインフレーション")]
            MuscleInflation = 63,
            [Comment("アースヒール")]
            EarthHeel = 64,
            [Comment("ダンシングブロッカー")]
            DancingBlocker = 65,
            [Comment("防御向上")]
            ImproveDefense = 67,
            [Comment("ムーンクライ")]
            MoonCry = 68,
            [Comment("グレートガッツ")]
            GreatGuts = 69,
            [Comment("回避")]
            Avoidance = 70,
            [Comment("アタックインターセプター")]
            AttackInterceptor = 71,
            [Comment("テレポーテーション")]
            Teleportation = 72,

            /// <summary>
            /// バフ？
            /// </summary>
            Buff = 73,
            [Comment("コンプリートプロテクション")]
            CompleteProtection = 74,
            [Comment("シマーリングシールド")]
            ShimmeringRingShield = 75,
            [Comment("ミラータワー")]
            MillerTower = 76,
            [Comment("攻撃命令")]
            AttackCommand = 78,
            [Comment("マウスフル")]
            MouseFull = 79,
            [Comment("双連破")]
            Shot = 83,
            [Comment("ピューマラッシュ")]
            PumaRush = 84,
            [Comment("エクスプロージョン")]
            Explosion = 85,
            [Comment("リフレクトライト")]
            ReflectLight = 86,
            [Comment("クリムゾン・アイ")]
            CrimsonEye = 88,
        }

        /// <summary>
        /// 元情報
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        private struct SkillInfo
        {
            public SkillInfo(byte[] data)
            {
                this = Helper.BytesToStructFromFieldConstructer<SkillInfo>(data);

                //範囲ダメージのスケール逆
                RangeDamageAmpRateScale.ReverseMinMax();
            }

            /// <summary>
            /// 真のIndex
            /// </summary>
            public ushort Index;

            /// <summary>
            /// 似てるスキルは同じIndex
            /// </summary>
            public ushort FamilyIndex;

            /// <summary>
            /// スキルの種類
            /// </summary>
            public SkillMakePacketType MakePacketType;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x05)]
            public ushort[] Unknown_00;

            /// <summary>
            /// 使用可能なキャラクター
            /// </summary>
            public Player.JOB Job;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x02)]
            public ushort[] Unknown_01;

            /// <summary>
            /// スキル名
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string Name;

            /// <summary>
            /// スキル難易度
            /// </summary>
            public uint Difficulty;

            /// <summary>
            /// パッシブ・非パッシブの攻撃補助
            /// </summary>
            public SkillPassiveType PassiveType; 

            /// <summary>
            /// ダメージのタイプ
            /// </summary>
            public SkillDamageFlag DamageFlag;

            /// <summary>
            /// スキルの対象種族
            /// </summary>
            public SkillTargetRace TargetRaceFlag;

            /// <summary>
            /// 使用先フラグ
            /// </summary>
            public SkillUsageFlag UsageFlag;

            public SkillExpression Unknown_03;
            
            /// <summary>
            /// 消費CP
            /// </summary>
            public SkillExpression LostCP;

            /// <summary>
            /// 獲得CP
            /// </summary>
            public SkillExpression GetCP;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x03)]
            public SkillExpression[] Unknown_2;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x19)]
            public SkillExpression[] Unknown_3;

            public SkillExpression Unknown_4;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x02)]
            public SkillExpression[] Unknown_5;

            public ushort Unknown_6;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x02)]
            public SkillExpression[] Unknown_7;

            /// <summary>
            /// 範囲スキルの範囲
            /// </summary>
            public SkillExpression Range;

            /// <summary>
            /// ダメージ減少値/距離　係数
            /// </summary>
            public SkillExpression RangeDecreaseCoef;

            /// <summary>
            /// 範囲ダメージの最小値・最大値
            /// </summary>
            public Scale<ushort> RangeDamageAmpRateScale;

            /// <summary>
            /// クールタイム
            /// </summary>
            [BytesToStructHelper(constructerIndex: 1)]
            public SkillExpression CoolTime;

            public ushort Unknown_8;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x13)]
            public SkillExpression[] Unknown_10;

            /// <summary>
            /// 物理ダメージ+(%)
            /// </summary>
            public SkillExpression DamagePercent;
            
            /// <summary>
            /// 防御力+
            /// </summary>
            public SkillExpression Defence;

            /// <summary>
            /// 最大ブロック率+(%)
            /// </summary>
            public SkillExpression MaxBlockRate;

            /// <summary>
            /// 知識ダメージ
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x06)]
            public SkillInteligenceDamage[] SkillInteligenceDamages;
            
            /// <summary>
            /// アビリティ
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x0A)]
            public SkillAbility[] SkillAbilitys;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x04)]
            public SkillExpression[] Unknown_12_1;

            /// <summary>
            /// 攻撃速度（Frame）
            /// </summary>
            public SkillExpression Frame;

            public ushort Unknown_13;

            /// <summary>
            /// 攻撃速度+(%)
            /// </summary>
            public SkillExpression AttackSpeed;

            public ushort junk_1;

            /// <summary>
            /// 最大射程距離（タゲをとれる最大の距離）
            /// </summary>
            public SkillExpression MaxEnableRange;
            
            public SkillExpression Unknown_14;

            public ushort junk_2;

            /// <summary>
            /// 効果範囲距離*100[m]
            /// </summary>
            public SkillExpression EffectRange;

            public SkillExpression Unknown_15;

            /// <summary>
            /// 命中率(%)
            /// </summary>
            [BytesToStructHelper(constructerIndex: 1)]
            public SkillExpression HitRate;

            /// <summary>
            /// 回避率(%)
            /// </summary>
            [BytesToStructHelper(constructerIndex: 1)]
            public SkillExpression AvoidRate;

            /// <summary>
            /// 致命打率(%)
            /// </summary>
            [BytesToStructHelper(constructerIndex: 1)]
            public SkillExpression FatalAttackRate;

            /// <summary>
            /// 決定打率(%)
            /// </summary>
            [BytesToStructHelper(constructerIndex: 1)]
            public SkillExpression CriticalAttackRate;

            /// <summary>
            /// 種族決定打率(%)　アンデット，悪魔，...
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x05)]
            public SkillExpression[] RaceCriticalAttackRate;

            ushort junk_3;

            /// <summary>
            /// 種族即死確率(%)　アンデット，悪魔，...
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x05)]
            public SkillExpression[] RaceInstantDeathRate;

            /// <summary>
            /// ブロック率+(%)
            /// </summary>
            public SkillExpression BlockRate;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x02)]
            public SkillExpression[] Unknown_18;
            
            /// <summary>
            /// 麻痺抵抗(%)
            /// </summary>
            public SkillExpression StunResistance;

            /// <summary>
            /// 異常系抵抗(%)
            /// </summary>
            public SkillExpression AbnormalResistance;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public SkillExpression[] Unknown_17_0;

            /// <summary>
            /// 敵攻撃限界回数（GPなど）
            /// </summary>
            [BytesToStructHelper(constructerIndex: 2)]
            public SkillExpression EnemyAttackLimit;

            /// <summary>
            /// ターゲット数
            /// </summary>
            [BytesToStructHelper(constructerIndex: 2)]
            public SkillExpression TargetNumber;

            /// <summary>
            /// 攻撃回数（分身人数）
            /// </summary>
            [BytesToStructHelper(constructerIndex: 2)]
            public SkillExpression AttackCount;

            [BytesToStructHelper(constructerIndex: 1)]
            public SkillExpression Unknown_17;

            /// <summary>
            /// 持続時間（秒）
            /// </summary>
            [BytesToStructHelper(constructerIndex: 2)]
            public SkillExpression DurationTime;

            /// <summary>
            /// 発動確率(%)
            /// </summary>
            public SkillExpression InvokeProb;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
            public SkillExpression[] Unknown_19;
            
            ushort junk_4;

            /// <summary>
            /// 基礎発動確率(&)
            /// </summary>
            [BytesToStructHelper(constructerIndex: 2)]
            public SkillExpression BasicInvokeProb;

            public ushort junk_5;

            /// <summary>
            /// 要求スキル
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x05)]
            public RequireSkill[] RequireSkills;
            
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x6F)]
            public SkillExpression[] Unknown_21;

            /// <summary>
            /// 説明文
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x100)]
            public string Description;

            /// <summary>
            /// 上昇形態の説明
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x40)]
            public string ElevatedForm;

            public override string ToString() => Name;
        }
        
        /// <summary>
        /// スキルの特殊能力関係（ヘイストなど）
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [BytesToStructHelper(0x40, BytesToStructFromFieldConstructer = true)]
        public struct SkillAbility
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public ushort[] Unknown_0;

            /// <summary>
            /// 付加能力のインデックス
            /// </summary>
            public AbilityType AbilityIndex;

            /// <summary>
            /// 発動確率（％）
            /// </summary>
            public SkillExpression Probability;

            /// <summary>
            /// 持続時間
            /// </summary>
            public SkillExpression DurationTime;

            /// <summary>
            /// 能力値
            /// </summary>
            public DynamicExpression Value;

            /// <summary>
            /// 能力オプション
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public DynamicExpression[] AbilityOPs;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public ushort[] Unknown_2;

            /// <summary>
            /// 特殊能力実行
            /// </summary>
            /// <param name="actor"></param>
            /// <param name="slv"></param>
            /// <returns></returns>
            public bool ExecuteToActor(Actor actor, int slv)
            {
                if (!Probability.ExpressionUnavailable&&!Helper.Lottery(Probability[slv]))//発動確率
                {
                    return false;
                }
                switch (AbilityIndex)
                {
                    case AbilityType.Confusion://混乱
                        break;
                    case AbilityType.ArmorBreaking://防具破壊
                        break;
                    case AbilityType.WeaponDestruction://武器破壊
                        break;
                    case AbilityType.Paralysis://麻痺
                        break;
                    case AbilityType.Freezing://氷結
                        break;
                    case AbilityType.Frozen://凍結
                        break;
                    case AbilityType.Darkness://暗闇
                        break;
                    case AbilityType.DeclineInAccuracy://命中率低下
                        break;
                    case AbilityType.ReductionOfAvoidanceRatio://回避率低下
                        break;
                    case AbilityType.IncreaseAttackPowerPercent_ATTACK://攻撃力%上昇(攻命)
                        break;
                    case AbilityType.DefensePowerIncreasePercent_MORTALITY://防御力%上昇(防命)
                        break;
                    case AbilityType.GivePetAndSummonBeastsAnAttackOrder://ペットと召喚獣に攻撃の指令を与える
                        break;
                    case AbilityType.INVALID://INVALID
                        break;
                    case AbilityType.AttackPowerDrop://攻撃力低下
                        break;
                    case AbilityType.HitRateReduction_PHANTOMSPEAR://命中率低下(幻槍)
                        break;
                    case AbilityType.ReductionInAvoidanceRate_PHANTOMSPEAR://回避率低下(幻槍)
                        break;
                    case AbilityType.Darkness_1://暗闇
                        break;
                    case AbilityType.HitLowAvoidanceRatio://命中/回避率低下
                        break;
                    case AbilityType.Curse://呪い
                        break;
                    case AbilityType.DummyGeneration_DUMMY://ダミー生成(ダミー)
                        break;
                    case AbilityType.MovingSpeedDecrease://移動速度低下
                        break;
                    case AbilityType.DecreaseAttackSpeed://攻撃速度低下
                        break;
                    case AbilityType.RiseOfAccuracy://命中率上昇
                        break;
                    case AbilityType.EvasionRateIncrease://回避率上昇
                        break;
                    case AbilityType.RisingSpeed://移動速度上昇
                        actor.Buff.SetLimitedTimeAbility(ActorBuff.BuffType.MovingSpeed, Value[slv], (int)DurationTime[slv]);
                        break;
                    case AbilityType.AttackSpeedIncrease://攻撃速度上昇
                        break;
                    case AbilityType.INVALID_1://INVALID
                        break;
                    case AbilityType.IncreasedAttackPower://攻撃力上昇
                        break;
                    case AbilityType.IncreaseAttackPowerPercent://攻撃力%上昇
                        break;
                    case AbilityType.DefensivePowerReduction_UNFAIRNESS://防御力低下(不公正)
                        break;
                    case AbilityType.DecliningDefense://防御力低下
                        break;
                    case AbilityType.DefensePowerPercentDrop://防御力%低下
                        break;
                    case AbilityType.IncreasedDefensePower://防御力上昇
                        break;
                    case AbilityType.DefensePowerIncreasePercent://防御力%上昇
                        break;
                    case AbilityType.AbnormalSystemAbnormalResistance://異常系異常抵抗
                        break;
                    case AbilityType.DecliningSystemAbnormalResistance://低下系異常抵抗
                        break;
                    case AbilityType.CurseSystemAbnormalResistance://呪い系異常抵抗
                        break;
                    case AbilityType.DecliningTotalAttributeResistance://全属性抵抗低下
                        break;
                    case AbilityType.AllAttributeResistancePercentDecrease://全属性抵抗%低下
                        break;
                    case AbilityType.ReductionOfFireAttributeResistance://火属性抵抗低下
                        break;
                    case AbilityType.WaterResistanceResistanceDrop://水属性抵抗低下
                        break;
                    case AbilityType.WindResistanceResistanceDrop://風属性抵抗低下
                        break;
                    case AbilityType.SoilResistanceResistanceDrop://土属性抵抗低下
                        break;
                    case AbilityType.ReductionOfLightAttributeResistance://光属性抵抗低下
                        break;
                    case AbilityType.DarkResistanceResistanceDrop://闇属性抵抗低下
                        break;
                    case AbilityType.IncreaseOfTotalAttributeResistance://全属性抵抗上昇
                        break;
                    case AbilityType.RisingFireResistanceResistance://火属性抵抗上昇
                        break;
                    case AbilityType.RisingWaterAttributeResistance://水属性抵抗上昇
                        break;
                    case AbilityType.WindResistanceResistanceRise://風属性抵抗上昇
                        break;
                    case AbilityType.RiseOfSoilAttributeResistance://土属性抵抗上昇
                        break;
                    case AbilityType.IncreaseInOpticalAttributeResistance://光属性抵抗上昇
                        break;
                    case AbilityType.RiseResistanceOfDarkAttribute://闇属性抵抗上昇
                        break;
                    case AbilityType.INVALID_5://INVALID
                        break;
                    case AbilityType.ConcentrationRise://集中力上昇
                        break;
                    case AbilityType.ConcentrationRise_STELLA://集中力上昇(ステラ)
                        break;
                    case AbilityType.ConcentrationDecline_NOVA://集中力低下(ノヴァ)
                        break;
                    case AbilityType.PowerRise://力上昇
                        break;
                    case AbilityType.AgileRise://敏捷上昇
                        break;
                    case AbilityType.HealthRise://健康上昇
                        break;
                    case AbilityType.RiseInMajesty://威厳上昇
                        break;
                    case AbilityType.KnowledgeRise://知識上昇
                        break;
                    case AbilityType.RisingWisdom://知恵上昇
                        break;
                    case AbilityType.Luck://運上昇
                        break;
                    case AbilityType.MaximumCPRise://最大CP上昇
                        break;
                    case AbilityType.INVALID_6://INVALID
                        break;
                    case AbilityType.MaximumHPPercentRise://最大HP%上昇
                        break;
                    case AbilityType.RegenerativeEffect://リジェネ効果
                        break;
                    case AbilityType.KnockBack://ノックバック
                        break;
                    case AbilityType.Paralysis_ROCK://麻痺(ロック)
                        break;
                    case AbilityType.Immovable://移動不能 
                        break;
                    case AbilityType.MovingSpeedReduction_GRAD://移動速度低下(グラ)
                        break;
                    case AbilityType.Paralysis_EARTHQUAKE://麻痺(地震)
                        break;
                    case AbilityType.Petrification_STONETOUCH://石化(石触)
                        break;
                    case AbilityType.Poison://毒
                        break;
                    case AbilityType.TreatAPoisonousCondition://毒状態を治療する
                        break;
                    case AbilityType.IncreasedAttackPower_RESONANCE://攻撃力上昇(共鳴)
                        break;
                    case AbilityType.Unknown://不明
                        break;
                    case AbilityType.AddFlameAttribute_ENCHA://炎属性付加(エンチャ)
                        break;
                    case AbilityType.INVALID_7://INVALID
                        break;
                    case AbilityType.INVALID_8://INVALID
                        break;
                    case AbilityType.INVALID_9://INVALID
                        break;
                    case AbilityType.LightAttributeAddition_BLUR://光属性付加(ブレ)
                        break;
                    case AbilityType.DarkAttributeAddition://闇属性付加
                        break;
                    case AbilityType.INVALID_10://INVALID
                        break;
                    case AbilityType.INVALID_11://INVALID
                        break;
                    case AbilityType.INVALID_12://INVALID
                        break;
                    case AbilityType.DamageDamage://地ダメージ
                        break;
                    case AbilityType.INVALID_13://INVALID
                        break;
                    case AbilityType.DarkDamage://闇ダメージ
                        break;
                    case AbilityType.FireContinuousDamage://火持続ダメージ
                        break;
                    case AbilityType.INVALID_14://INVALID
                        break;
                    case AbilityType.WindSustainedDamage://風持続ダメージ
                        break;
                    case AbilityType.INVALID_15://INVALID
                        break;
                    case AbilityType.LightPersistenceDamage://光持続ダメージ
                        break;
                    case AbilityType.INVALID_16://INVALID
                        break;
                    case AbilityType.EnemyEscape_THREAT://敵逃亡(脅)
                        break;
                    case AbilityType.UndeadFugitive_TU://アンデッド逃亡(TU)
                        break;
                    case AbilityType.UndeadParalysis_TU://アンデッド麻痺(TU)
                        break;
                    case AbilityType.UndeadDeathToll_TU://アンデッド即死(TU)
                        break;
                    case AbilityType.Escape_85_Paralysis_60_LightDamage_30GivingUndeadTheFollowingEffects://アンデッドに対し以下の効果を与える逃亡(85%)、麻痺(60%)、光ダメ(30%)
                        break;
                    case AbilityType.FightEscaping_90_Paralysis_70_LightShot_35_GivingTheFollowingEffectToUndead_Devils://アンデッド・悪魔に対し以下の効果を与える逃亡(90%)、麻痺(70%)、光ダメ(35%)
                        break;
                    case AbilityType.HPAbsorption_M://HP吸収(M)
                        break;
                    case AbilityType.HPRecovery_HORIKURO_LUCK://HP回復(ホリクロ・運気)
                        break;
                    case AbilityType.Sleep://眠り
                        break;
                    case AbilityType.AutomaticallyTreatAbnormalSystemAbnormalities://異常系異常を自動で治療する
                        break;
                    case AbilityType.AutomaticallyTreatACurseSystemAbnormality://呪い系異常を自動で治療する
                        break;
                    case AbilityType.AutomaticallyTreatLoweringSystemAbnormalities://低下系異常を自動で治療する
                        break;
                    case AbilityType.Possession://憑依
                        break;
                    case AbilityType.SearchingEnemies_DETECTFOOTSEARCH://索敵(ディテク・足探)
                        break;
                    case AbilityType.DeviceDetection://装置探知
                        break;
                    case AbilityType.AuxiliaryMagicClear_MD://補助魔法解除(MD)
                        break;
                    case AbilityType.CPDecrease_MD_GALPARA://CP減少(MD・ガルパラ)
                        break;
                    case AbilityType.Unknown_1://不明
                        break;
                    case AbilityType.Temptation://誘惑
                        break;
                    case AbilityType.MovingSpeedVariation_THUNK://移動速度変動(サンク)
                        break;
                    case AbilityType.CityMovement_EVA://街移動(エバ)
                        break;
                    case AbilityType.Calling_CALL://コーリング(コール)
                        break;
                    case AbilityType.TownPortal_TAUPO://タウンポータル(タウポ)
                        break;
                    case AbilityType.LvDrop://Lv低下
                        break;
                    case AbilityType.LvRise://Lv上昇
                        break;
                    case AbilityType.INVALID_17://INVALID
                        break;
                    case AbilityType.INVALID_18://INVALID
                        break;
                    case AbilityType.PetTimes://ペットテイム
                        break;
                    case AbilityType.TagetEvasion://タゲ回避
                        break;
                    case AbilityType.INVALID_19://INVALID
                        break;
                    case AbilityType.FirstAid://応急処置
                        break;
                    case AbilityType.BreedingRecordLimitLv://飼育記録限界Lv
                        break;
                    case AbilityType.ExperienceRise_SCOLD://経験値上昇(叱)
                        break;
                    case AbilityType.RedPeppers://唐辛子
                        break;
                    case AbilityType.INVALID_20://INVALID
                        break;
                    case AbilityType.FlameAttackIncreasesAttack://炎属性攻撃上昇
                        break;
                    case AbilityType.RisingWaterAttributeAttack://水属性攻撃上昇
                        break;
                    case AbilityType.WindAttributeAttackElevation://風属性攻撃上昇
                        break;
                    case AbilityType.EarthAttackAttributeRise://土属性攻撃上昇
                        break;
                    case AbilityType.IncreaseOfLightAttributeAttack://光属性攻撃上昇
                        break;
                    case AbilityType.DarknessAttributeRiseAscent://闇属性攻撃上昇
                        break;
                    case AbilityType.PureAttributeAttackRise_ELE://純粋属性攻撃上昇(エレ)
                        break;
                    case AbilityType.TemptationDecrease://誘惑低下
                        break;
                    case AbilityType.EnemyAssembly://敵集合
                        break;
                    case AbilityType.PetTaking://ペット奪取
                        break;
                    case AbilityType.Summons://召喚
                        break;
                    case AbilityType.SummonPowerUp://召喚パワーアップ
                        break;
                    case AbilityType.DogRiding://犬乗り
                        break;
                    case AbilityType.SkillLvRise://スキルLv上昇
                        break;
                    case AbilityType.Trap://罠
                        break;
                    case AbilityType.DamagedReflection://ダメージ反射
                        break;
                    case AbilityType.CriticalRateRise://クリティカル率上昇
                        break;
                    case AbilityType.PhysicalDuck_NOT_REMAININGHP://物理ダメ（残りHP×）
                        break;
                    case AbilityType.ObtainNearbyItems://近くのアイテムを入手する
                        break;
                    case AbilityType.ObtainNearbyItems_1://近くのアイテムを入手する
                        break;
                    case AbilityType.INVALID_21://INVALID
                        break;
                    case AbilityType.BaseAttackPowerReduction_M://基本攻撃力低下(M)
                        break;
                    case AbilityType.IncreaseOfBasicAttackPower://基本攻撃力上昇
                        break;
                    case AbilityType.IncreasedBaseDefensePower://基本防御力上昇
                        break;
                    case AbilityType.TargetFixation://ターゲット固定?
                        break;
                    case AbilityType.GiveAPetAndASummonedBeastACommandOfAttackBySpecialSkillAndFixATagger://ペットと召喚獣に特技による攻撃の指令を与え、タゲを固定させる
                        break;
                    case AbilityType.ParalysisResistance://麻痺抵抗
                        break;
                    case AbilityType.DamageReduction_PROTECTION://ダメージ減少(防御)
                        break;
                    case AbilityType.Invincible_REBELLIOUS://無敵(仰反)
                        break;
                    case AbilityType.INVALID_22://INVALID
                        break;
                    case AbilityType.AbnormalSystemAbnormalTimeDecrease://異常系異常時間減少
                        break;
                    case AbilityType.INVALID_23://INVALID
                        break;
                    case AbilityType.ReducedSystemAbnormalTimeDecrease://低下系異常時間減少
                        break;
                    case AbilityType.Avatar://分身
                        break;
                    case AbilityType.INVALID_24://INVALID
                        break;
                    case AbilityType.INVALID_25://INVALID
                        break;
                    case AbilityType.RightHandRaisesLv://右手Lv上昇
                        break;
                    case AbilityType.LeftHandLvRising://左手Lv上昇
                        break;
                    case AbilityType.RightFootLvUp://右足Lv上昇
                        break;
                    case AbilityType.LeftLegLvUp://左足Lv上昇
                        break;
                    case AbilityType.LowerRiseLvRise://下蹴Lv上昇
                        break;
                    case AbilityType.PoisonDamageReduction_SLOPO://毒ダメージ減少(スロポ)
                        break;
                    case AbilityType.Unknown_SLOPO://不明(スロポ)
                        break;
                    case AbilityType.INVALID_26://INVALID
                        break;
                    case AbilityType.Blur://ブラー
                        break;
                    case AbilityType.InstantDeath_ASSASSINATION://即死(暗殺)
                        break;
                    case AbilityType.TrapCancellation_DITHER://罠解除(ディザ)
                        break;
                    case AbilityType.TrapDetection://罠探知
                        break;
                    case AbilityType.DoorDetection_DOORDETECTION://扉探知(扉探知)
                        break;
                    case AbilityType.TreasureOpening_LOKPI://宝開放(ロクピ)
                        break;
                    case AbilityType.OpeningTheDoor_AMURO://扉開放(アンロ)
                        break;
                    case AbilityType.TakingTheGold_PIKPO://ゴールド奪取(ピクポ)
                        break;
                    case AbilityType.TakingAnItem_ROBBERY://アイテム奪取(強奪)
                        break;
                    case AbilityType.TheProphecyOfDeath://死の予言
                        break;
                    case AbilityType.GoldTaken_M://ゴールド奪取(M)
                        break;
                    case AbilityType.INVALID_27://INVALID
                        break;
                    case AbilityType.UndeadResuscitation://アンデッド蘇生
                        break;
                    case AbilityType.DamageReflectionCurse://ダメージ反射呪
                        break;
                    case AbilityType.SpreadTheSpidersThread://蜘蛛の糸を撒き散らす
                        break;
                    case AbilityType.ImmovableReflex://移動不能反射
                        break;
                    case AbilityType.DarkDamage_1://闇ダメージ
                        break;
                    case AbilityType.UBarrier://Uバリア
                        break;
                    case AbilityType.StorePremium://店割増値
                        break;
                    case AbilityType.Insanity://狂気
                        break;
                    case AbilityType.Dance_LD://踊り(LD)
                        break;
                    case AbilityType.InfectedInABox://箱に幽閉
                        break;
                    case AbilityType.CopyCreation_Magic://コピー作成(魔描)
                        break;
                    case AbilityType.FrogTransform://かえる変身
                        break;
                    case AbilityType.Unknown_2://不明
                        break;
                    case AbilityType.Huge://巨大化
                        break;
                    case AbilityType.MakeAPartyMemberWeapon://パーティーメンバーの武器に変身
                        break;
                    case AbilityType.MakeARabbit://うさぎに変身
                        break;
                    case AbilityType.INVALID_28://INVALID
                        break;
                    case AbilityType.INVALID_29://INVALID
                        break;
                    case AbilityType.TrickKick://トリックキック
                        break;
                    case AbilityType.ChainAttackRange_STUNNING://連鎖攻撃射程(吃驚び)
                        break;
                    case AbilityType.AttributeDegradationOfTotalAbnormalResistance_FLOWERFLOW://属性・全異常抵抗低下(花流)
                        break;
                    case AbilityType.BulletAttributeDamage_BOTTOM://弾丸属性ダメージ(瓶投)
                        break;
                    case AbilityType.CPAcquisition://CP獲得
                        break;
                    case AbilityType.AClown://道化師
                        break;
                    case AbilityType.CapacityChange://能力入替
                        break;
                    case AbilityType.DevilsIllusion://悪魔の幻影
                        break;
                    case AbilityType.DamageCut://ダメージカット
                        break;
                    case AbilityType.InverseScales://逆鱗
                        break;
                    case AbilityType.BloodSucking://吸血
                        break;
                    case AbilityType.RatherOfNeedles://針のむしろ
                        break;
                    case AbilityType.AnAntHill://蟻地獄
                        break;
                    case AbilityType.PI://PI
                        break;
                    case AbilityType.Nightmare://悪夢
                        break;
                    case AbilityType.FieryFire://烈火
                        break;
                    case AbilityType.GiveTheCommandOfAttackToTheDescentEvilUndead://降霊アンデッドに攻撃の指令を与える
                        break;
                    case AbilityType.ShadowOfConspiracy://陰謀の影
                        break;
                    case AbilityType.TheScentOfDeath://死の香り
                        break;
                    case AbilityType.Attract://引き寄せ
                        break;
                    case AbilityType.INVALID_30://INVALID
                        break;
                    case AbilityType.Vibre://バイブレ
                        break;
                    case AbilityType.INVALID_31://INVALID
                        break;
                    case AbilityType.INVALID_32://INVALID
                        break;
                    case AbilityType.INVALID_33://INVALID
                        break;
                    case AbilityType.INVALID_34://INVALID
                        break;
                    case AbilityType.INVALID_35://INVALID
                        break;
                    case AbilityType.ISignAContractWithTheEnemy://敵と契約を結ぶ
                        break;
                    case AbilityType.ReductionInRecoveryRate://回復率低下
                        break;
                    case AbilityType.ResurrectionDisturbance://復活邪魔
                        break;
                    case AbilityType.INVALID_36://INVALID
                        break;
                    case AbilityType.DisableArmorAccordingToACertainLevelAndNumberOfTimes://一定のレベルと回数に応じて防具を使えなくする
                        break;
                    case AbilityType.ASoulPledge://魂の誓約
                        break;
                    case AbilityType.BloodPledge://血の誓約
                        break;
                    case AbilityType.TheEffectOfContractSkillDisappears://契約スキルの効果が消える
                        break;
                    case AbilityType.Unknown_3://不明
                        break;
                    case AbilityType.Unknown_4://不明
                        break;
                    case AbilityType.Unknown_5://不明
                        break;
                    case AbilityType.LastAttackPowerIncreasePercent://最終攻撃力%上昇
                        break;
                    case AbilityType.INVALID_37://INVALID
                        break;
                    case AbilityType.Unknown_6://不明
                        break;
                    case AbilityType.INVALID_38://INVALID
                        break;
                    case AbilityType.INVALID_39://INVALID
                        break;
                    case AbilityType.Unknown_7://不明
                        break;
                    case AbilityType.Unknown_8://不明
                        break;
                    case AbilityType.INVALID_40://INVALID
                        break;
                    case AbilityType.Unknown_9://不明
                        break;
                    case AbilityType.Unknown_10://不明
                        break;
                    case AbilityType.Unknown_11://不明
                        break;
                    case AbilityType.INVALID_41://INVALID
                        break;
                    case AbilityType.Unknown_12://不明
                        break;
                    case AbilityType.INVALID_42://INVALID
                        break;
                    case AbilityType.INVALID_43://INVALID
                        break;
                    case AbilityType.INVALID_44://INVALID
                        break;
                    case AbilityType.INVALID_45://INVALID
                        break;
                    case AbilityType.INVALID_46://INVALID
                        break;
                    case AbilityType.INVALID_47://INVALID
                        break;
                    case AbilityType.INVALID_48://INVALID
                        break;
                    case AbilityType.INVALID_49://INVALID
                        break;
                    case AbilityType.INVALID_50://INVALID
                        break;
                    case AbilityType.NoWeaponEquipped://武器装備不可
                        break;
                    case AbilityType.AttackPowerDecrease_ARMSWORD://攻撃力低下(腕斬)
                        break;
                    case AbilityType.ReSkillCanNotBeActivated://Reスキル発動不可
                        break;
                    case AbilityType.Assemble://アセンブル
                        break;
                    case AbilityType.FullCapacityRise://全能力上昇
                        break;
                    case AbilityType.DefensePowerAvoidanceReduction://防御力・回避率低下
                        break;
                    case AbilityType.MortalBattingAttackPowerIncrease://致命打攻撃力上昇
                        break;
                    case AbilityType.LootedCP://略奪CP
                        break;
                    case AbilityType.BuffReversal://バフ逆転
                        break;
                    case AbilityType.ContinuedFireDamage://継続火ダメージ
                        break;
                    case AbilityType.FireDamage_RAGE://火ダメージ(レイジ)
                        break;
                    case AbilityType.SkillLv1Limit://スキルLv1制限
                        break;
                    case AbilityType.IncreaseOfNextAttack://次の攻撃の増加
                        break;
                    case AbilityType.INVALID_51://INVALID
                        break;
                    case AbilityType.LightningDurationUp://雷撃破持続時間↑
                        break;
                    case AbilityType.INVALID_52://INVALID
                        break;
                    case AbilityType.BlockInvalid://ブロック無効
                        break;
                    case AbilityType.INVALID_53://INVALID
                        break;
                    case AbilityType.Attack3://敵に3倍のダメージを与える攻撃を繰り出す
                        break;
                    case AbilityType.ThrowAway://敵を放り投げる（霊術師）
                        break;
                    case AbilityType.DivisionAttack://分身攻撃回数（霊術師　蛇の目拳）
                        break;
                    case AbilityType.Bit://ビット（光奏師）
                        break;
                    case AbilityType.INVALID_56://INVALID
                        break;
                    case AbilityType.ReceiveAnyRateOfFinalDamageWhenBleeding://出血時、最終ダメージのn％を2秒間隔で追加
                        break;
                    case AbilityType.ElectricShockByAttack://敵は攻撃を受けるごとにダメージのn％に該当する感電ダメージを追加
                        break;
                    case AbilityType.ImmediateRecoveryByHealing://ヒール系呪文をうけた時，治療量のn%を即時回復
                        break;
                    case AbilityType.ChangeUndead://効果を受けたActorの種族をアンデッドに変更
                        break;
                }
                return true;
            }

            public override string ToString()
            {
                string valstr = Value.Unavailable ? "[0]" : Value.ToString();
                var comment = (Comment)typeof(AbilityType).GetField(AbilityIndex.ToString()).GetCustomAttribute(typeof(Comment), false);
                return $"[{comment.Str}] : {valstr}";
            }

            /// <summary>
            /// レベルに応じて係数が変化
            /// </summary>
            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public struct DynamicExpression
            {
                /// <summary>
                /// 式
                /// </summary>
                readonly Func<int, float> Exp;

                /// <summary>
                /// 利用不可能フラグ
                /// </summary>
                public readonly bool Unavailable;

                private DynamicExpression(ushort initA, ushort initB, ushort incA, ushort incB)
                {
                    Unavailable = initA == 0 && initB == 0 && incA == 0 && incB == 0;
                    float InitA = initA / (float)100.0;
                    float InitB = initB / (float)100.0;
                    float IncA = incA / (float)100.0;
                    float IncB = incB / (float)100.0;

                    Exp = t => InitA * t + InitB + (IncA * t + IncB) * t;
                }

                /// <summary>
                /// OPの値
                /// </summary>
                /// <param name="slv"></param>
                /// <returns></returns>
                public double this[int slv] => Exp(slv);

                public override string ToString()
                {
                    if (Unavailable) return nameof(Unavailable);
                    string[] result = new string[10];
                    for (int i = 0; i < 10; i++)
                        result[i] = $"{i + 1}:[{this[i + 1]}]";
                    return string.Join(", ", result);
                }
            }

            /// <summary>
            /// 特殊能力名
            /// </summary>
            public enum AbilityType : ushort
            {
                [Comment("混乱")]
                Confusion = 0,
                [Comment("防具破壊")]
                ArmorBreaking = 1,
                [Comment("武器破壊")]
                WeaponDestruction = 2,
                [Comment("麻痺")]
                Paralysis = 3,
                [Comment("氷結")]
                Freezing = 4,
                [Comment("凍結")]
                Frozen = 5,
                [Comment("暗闇")]
                Darkness = 6,
                [Comment("命中率低下")]
                DeclineInAccuracy = 7,
                [Comment("回避率低下")]
                ReductionOfAvoidanceRatio = 8,
                [Comment("攻撃力%上昇(攻命)")]
                IncreaseAttackPowerPercent_ATTACK = 9,
                [Comment("防御力%上昇(防命)")]
                DefensePowerIncreasePercent_MORTALITY = 10,
                [Comment("ペットと召喚獣に攻撃の指令を与える")]
                GivePetAndSummonBeastsAnAttackOrder = 11,
                [Comment("INVALID")]
                INVALID = 12,
                [Comment("攻撃力低下")]
                AttackPowerDrop = 13,
                [Comment("命中率低下(幻槍)")]
                HitRateReduction_PHANTOMSPEAR = 14,
                [Comment("回避率低下(幻槍)")]
                ReductionInAvoidanceRate_PHANTOMSPEAR = 15,
                [Comment("暗闇")]
                Darkness_1 = 16,
                [Comment("命中/回避率低下")]
                HitLowAvoidanceRatio = 17,
                [Comment("呪い")]
                Curse = 18,
                [Comment("ダミー生成(ダミー)")]
                DummyGeneration_DUMMY = 19,
                [Comment("移動速度低下")]
                MovingSpeedDecrease = 20,
                [Comment("攻撃速度低下")]
                DecreaseAttackSpeed = 21,
                [Comment("命中率上昇")]
                RiseOfAccuracy = 22,
                [Comment("回避率上昇")]
                EvasionRateIncrease = 23,
                [Comment("移動速度上昇")]
                RisingSpeed = 24,
                [Comment("攻撃速度上昇")]
                AttackSpeedIncrease = 25,
                [Comment("INVALID")]
                INVALID_1 = 26,
                [Comment("攻撃力上昇")]
                IncreasedAttackPower = 27,
                [Comment("攻撃力%上昇")]
                IncreaseAttackPowerPercent = 28,
                [Comment("防御力低下(不公正)")]
                DefensivePowerReduction_UNFAIRNESS = 29,
                [Comment("防御力低下")]
                DecliningDefense = 30,
                [Comment("防御力%低下")]
                DefensePowerPercentDrop = 31,
                [Comment("防御力上昇")]
                IncreasedDefensePower = 32,
                [Comment("防御力%上昇")]
                DefensePowerIncreasePercent = 33,
                [Comment("異常系異常抵抗")]
                AbnormalSystemAbnormalResistance = 34,
                [Comment("低下系異常抵抗")]
                DecliningSystemAbnormalResistance = 35,
                [Comment("呪い系異常抵抗")]
                CurseSystemAbnormalResistance = 36,
                [Comment("全属性抵抗低下")]
                DecliningTotalAttributeResistance = 37,
                [Comment("全属性抵抗%低下")]
                AllAttributeResistancePercentDecrease = 38,
                [Comment("火属性抵抗低下")]
                ReductionOfFireAttributeResistance = 39,
                [Comment("水属性抵抗低下")]
                WaterResistanceResistanceDrop = 40,
                [Comment("風属性抵抗低下")]
                WindResistanceResistanceDrop = 41,
                [Comment("土属性抵抗低下")]
                SoilResistanceResistanceDrop = 42,
                [Comment("光属性抵抗低下")]
                ReductionOfLightAttributeResistance = 43,
                [Comment("闇属性抵抗低下")]
                DarkResistanceResistanceDrop = 44,
                [Comment("全属性抵抗上昇")]
                IncreaseOfTotalAttributeResistance = 45,
                [Comment("火属性抵抗上昇")]
                RisingFireResistanceResistance = 46,
                [Comment("水属性抵抗上昇")]
                RisingWaterAttributeResistance = 47,
                [Comment("風属性抵抗上昇")]
                WindResistanceResistanceRise = 48,
                [Comment("土属性抵抗上昇")]
                RiseOfSoilAttributeResistance = 49,
                [Comment("光属性抵抗上昇")]
                IncreaseInOpticalAttributeResistance = 50,
                [Comment("闇属性抵抗上昇")]
                RiseResistanceOfDarkAttribute = 51,
                [Comment("INVALID")]
                INVALID_5 = 52,
                [Comment("集中力上昇")]
                ConcentrationRise = 53,
                [Comment("集中力上昇(ステラ)")]
                ConcentrationRise_STELLA = 54,
                [Comment("集中力低下(ノヴァ)")]
                ConcentrationDecline_NOVA = 55,
                [Comment("力上昇")]
                PowerRise = 56,
                [Comment("敏捷上昇")]
                AgileRise = 57,
                [Comment("健康上昇")]
                HealthRise = 58,
                [Comment("威厳上昇")]
                RiseInMajesty = 59,
                [Comment("知識上昇")]
                KnowledgeRise = 60,
                [Comment("知恵上昇")]
                RisingWisdom = 61,
                [Comment("運上昇")]
                Luck = 62,
                [Comment("最大CP上昇")]
                MaximumCPRise = 63,
                [Comment("INVALID")]
                INVALID_6 = 64,
                [Comment("最大HP%上昇")]
                MaximumHPPercentRise = 65,
                [Comment("リジェネ効果")]
                RegenerativeEffect = 66,
                [Comment("ノックバック")]
                KnockBack = 67,
                [Comment("麻痺(ロック)")]
                Paralysis_ROCK = 68,
                [Comment("移動不能 ")]
                Immovable = 69,
                [Comment("移動速度低下(グラ)")]
                MovingSpeedReduction_GRAD = 70,
                [Comment("麻痺(地震)")]
                Paralysis_EARTHQUAKE = 71,
                [Comment("石化(石触)")]
                Petrification_STONETOUCH = 72,
                [Comment("毒")]
                Poison = 73,
                [Comment("毒状態を治療する")]
                TreatAPoisonousCondition = 74,
                [Comment("攻撃力上昇(共鳴)")]
                IncreasedAttackPower_RESONANCE = 75,
                [Comment("不明")]
                Unknown = 76,
                [Comment("炎属性付加(エンチャ)")]
                AddFlameAttribute_ENCHA = 77,
                [Comment("INVALID")]
                INVALID_7 = 78,
                [Comment("INVALID")]
                INVALID_8 = 79,
                [Comment("INVALID")]
                INVALID_9 = 80,
                [Comment("光属性付加(ブレ)")]
                LightAttributeAddition_BLUR = 81,
                [Comment("闇属性付加")]
                DarkAttributeAddition = 82,
                [Comment("INVALID")]
                INVALID_10 = 83,
                [Comment("INVALID")]
                INVALID_11 = 84,
                [Comment("INVALID")]
                INVALID_12 = 85,
                [Comment("地ダメージ")]
                DamageDamage = 86,
                [Comment("INVALID")]
                INVALID_13 = 87,
                [Comment("闇ダメージ")]
                DarkDamage = 88,
                [Comment("火持続ダメージ")]
                FireContinuousDamage = 89,
                [Comment("INVALID")]
                INVALID_14 = 90,
                [Comment("風持続ダメージ")]
                WindSustainedDamage = 91,
                [Comment("INVALID")]
                INVALID_15 = 92,
                [Comment("光持続ダメージ")]
                LightPersistenceDamage = 93,
                [Comment("INVALID")]
                INVALID_16 = 94,
                [Comment("敵逃亡(脅)")]
                EnemyEscape_THREAT = 95,
                [Comment("アンデッド逃亡(TU)")]
                UndeadFugitive_TU = 96,
                [Comment("アンデッド麻痺(TU)")]
                UndeadParalysis_TU = 97,
                [Comment("アンデッド即死(TU)")]
                UndeadDeathToll_TU = 98,
                [Comment("アンデッドに対し以下の効果を与える逃亡(85%)、麻痺(60%)、光ダメ(30%)")]
                Escape_85_Paralysis_60_LightDamage_30GivingUndeadTheFollowingEffects = 99,
                [Comment("アンデッド・悪魔に対し以下の効果を与える逃亡(90%)、麻痺(70%)、光ダメ(35%)")]
                FightEscaping_90_Paralysis_70_LightShot_35_GivingTheFollowingEffectToUndead_Devils = 100,
                [Comment("HP吸収(M)")]
                HPAbsorption_M = 101,
                [Comment("HP回復(ホリクロ・運気)")]
                HPRecovery_HORIKURO_LUCK = 102,
                [Comment("眠り")]
                Sleep = 103,
                [Comment("異常系異常を自動で治療する")]
                AutomaticallyTreatAbnormalSystemAbnormalities = 104,
                [Comment("呪い系異常を自動で治療する")]
                AutomaticallyTreatACurseSystemAbnormality = 105,
                [Comment("低下系異常を自動で治療する")]
                AutomaticallyTreatLoweringSystemAbnormalities = 106,
                [Comment("憑依")]
                Possession = 107,
                [Comment("索敵(ディテク・足探)")]
                SearchingEnemies_DETECTFOOTSEARCH = 108,
                [Comment("装置探知")]
                DeviceDetection = 109,
                [Comment("補助魔法解除(MD)")]
                AuxiliaryMagicClear_MD = 110,
                [Comment("CP減少(MD・ガルパラ)")]
                CPDecrease_MD_GALPARA = 111,
                [Comment("不明")]
                Unknown_1 = 112,
                [Comment("誘惑")]
                Temptation = 113,
                [Comment("移動速度変動(サンク)")]
                MovingSpeedVariation_THUNK = 114,
                [Comment("街移動(エバ)")]
                CityMovement_EVA = 115,
                [Comment("コーリング(コール)")]
                Calling_CALL = 116,
                [Comment("タウンポータル(タウポ)")]
                TownPortal_TAUPO = 117,
                [Comment("Lv低下")]
                LvDrop = 118,
                [Comment("Lv上昇")]
                LvRise = 119,
                [Comment("INVALID")]
                INVALID_17 = 120,
                [Comment("INVALID")]
                INVALID_18 = 121,
                [Comment("ペットテイム")]
                PetTimes = 122,
                [Comment("タゲ回避")]
                TagetEvasion = 123,
                [Comment("INVALID")]
                INVALID_19 = 124,
                [Comment("応急処置")]
                FirstAid = 125,
                [Comment("飼育記録限界Lv")]
                BreedingRecordLimitLv = 126,
                [Comment("経験値上昇(叱)")]
                ExperienceRise_SCOLD = 127,
                [Comment("唐辛子")]
                RedPeppers = 128,
                [Comment("INVALID")]
                INVALID_20 = 129,
                [Comment("炎属性攻撃上昇")]
                FlameAttackIncreasesAttack = 130,
                [Comment("水属性攻撃上昇")]
                RisingWaterAttributeAttack = 131,
                [Comment("風属性攻撃上昇")]
                WindAttributeAttackElevation = 132,
                [Comment("土属性攻撃上昇")]
                EarthAttackAttributeRise = 133,
                [Comment("光属性攻撃上昇")]
                IncreaseOfLightAttributeAttack = 134,
                [Comment("闇属性攻撃上昇")]
                DarknessAttributeRiseAscent = 135,
                [Comment("純粋属性攻撃上昇(エレ)")]
                PureAttributeAttackRise_ELE = 136,
                [Comment("誘惑低下")]
                TemptationDecrease = 137,
                [Comment("敵集合")]
                EnemyAssembly = 138,
                [Comment("ペット奪取")]
                PetTaking = 139,
                [Comment("召喚")]
                Summons = 140,
                [Comment("召喚パワーアップ")]
                SummonPowerUp = 141,
                [Comment("犬乗り")]
                DogRiding = 142,
                [Comment("スキルLv上昇")]
                SkillLvRise = 143,
                [Comment("罠")]
                Trap = 144,
                [Comment("ダメージ反射")]
                DamagedReflection = 145,
                [Comment("クリティカル率上昇")]
                CriticalRateRise = 146,
                [Comment("物理ダメ（残りHP×）")]
                PhysicalDuck_NOT_REMAININGHP = 147,
                [Comment("近くのアイテムを入手する")]
                ObtainNearbyItems = 148,
                [Comment("近くのアイテムを入手する")]
                ObtainNearbyItems_1 = 149,
                [Comment("INVALID")]
                INVALID_21 = 150,
                [Comment("基本攻撃力低下(M)")]
                BaseAttackPowerReduction_M = 151,
                [Comment("基本攻撃力上昇")]
                IncreaseOfBasicAttackPower = 152,
                [Comment("基本防御力上昇")]
                IncreasedBaseDefensePower = 153,
                [Comment("ターゲット固定?")]
                TargetFixation = 154,
                [Comment("ペットと召喚獣に特技による攻撃の指令を与え、タゲを固定させる")]
                GiveAPetAndASummonedBeastACommandOfAttackBySpecialSkillAndFixATagger = 155,
                [Comment("麻痺抵抗")]
                ParalysisResistance = 156,
                [Comment("ダメージ減少(防御)")]
                DamageReduction_PROTECTION = 157,
                [Comment("無敵(仰反)")]
                Invincible_REBELLIOUS = 158,
                [Comment("INVALID")]
                INVALID_22 = 159,
                [Comment("異常系異常時間減少")]
                AbnormalSystemAbnormalTimeDecrease = 160,
                [Comment("INVALID")]
                INVALID_23 = 161,
                [Comment("低下系異常時間減少")]
                ReducedSystemAbnormalTimeDecrease = 162,
                [Comment("分身")]
                Avatar = 163,
                [Comment("INVALID")]
                INVALID_24 = 164,
                [Comment("INVALID")]
                INVALID_25 = 165,
                [Comment("右手Lv上昇")]
                RightHandRaisesLv = 166,
                [Comment("左手Lv上昇")]
                LeftHandLvRising = 167,
                [Comment("右足Lv上昇")]
                RightFootLvUp = 168,
                [Comment("左足Lv上昇")]
                LeftLegLvUp = 169,
                [Comment("下蹴Lv上昇")]
                LowerRiseLvRise = 170,
                [Comment("毒ダメージ減少(スロポ)")]
                PoisonDamageReduction_SLOPO = 171,
                [Comment("不明(スロポ)")]
                Unknown_SLOPO = 172,
                [Comment("INVALID")]
                INVALID_26 = 173,
                [Comment("ブラー")]
                Blur = 174,
                [Comment("即死(暗殺)")]
                InstantDeath_ASSASSINATION = 175,
                [Comment("罠解除(ディザ)")]
                TrapCancellation_DITHER = 176,
                [Comment("罠探知")]
                TrapDetection = 177,
                [Comment("扉探知(扉探知)")]
                DoorDetection_DOORDETECTION = 178,
                [Comment("宝開放(ロクピ)")]
                TreasureOpening_LOKPI = 179,
                [Comment("扉開放(アンロ)")]
                OpeningTheDoor_AMURO = 180,
                [Comment("ゴールド奪取(ピクポ)")]
                TakingTheGold_PIKPO = 181,
                [Comment("アイテム奪取(強奪)")]
                TakingAnItem_ROBBERY = 182,
                [Comment("死の予言")]
                TheProphecyOfDeath = 183,
                [Comment("ゴールド奪取(M)")]
                GoldTaken_M = 184,
                [Comment("INVALID")]
                INVALID_27 = 185,
                [Comment("アンデッド蘇生")]
                UndeadResuscitation = 186,
                [Comment("ダメージ反射呪")]
                DamageReflectionCurse = 187,
                [Comment("蜘蛛の糸を撒き散らす")]
                SpreadTheSpidersThread = 188,
                [Comment("移動不能反射")]
                ImmovableReflex = 189,
                [Comment("闇ダメージ")]
                DarkDamage_1 = 190,
                [Comment("Uバリア")]
                UBarrier = 191,
                [Comment("店割増値")]
                StorePremium = 192,
                [Comment("狂気")]
                Insanity = 193,
                [Comment("踊り(LD)")]
                Dance_LD = 194,
                [Comment("箱に幽閉")]
                InfectedInABox = 195,
                [Comment("コピー作成(魔描)")]
                CopyCreation_Magic = 196,
                [Comment("かえる変身")]
                FrogTransform = 197,
                [Comment("不明")]
                Unknown_2 = 198,
                [Comment("巨大化")]
                Huge = 199,
                [Comment("パーティーメンバーの武器に変身")]
                MakeAPartyMemberWeapon = 200,
                [Comment("うさぎに変身")]
                MakeARabbit = 201,
                [Comment("INVALID")]
                INVALID_28 = 202,
                [Comment("INVALID")]
                INVALID_29 = 203,
                [Comment("トリックキック")]
                TrickKick = 204,
                [Comment("連鎖攻撃射程(吃驚び)")]
                ChainAttackRange_STUNNING = 205,
                [Comment("属性・全異常抵抗低下(花流)")]
                AttributeDegradationOfTotalAbnormalResistance_FLOWERFLOW = 206,
                [Comment("弾丸属性ダメージ(瓶投)")]
                BulletAttributeDamage_BOTTOM = 207,
                [Comment("CP獲得")]
                CPAcquisition = 208,
                [Comment("道化師")]
                AClown = 209,
                [Comment("能力入替")]
                CapacityChange = 210,
                [Comment("悪魔の幻影")]
                DevilsIllusion = 211,
                [Comment("ダメージカット")]
                DamageCut = 212,
                [Comment("逆鱗")]
                InverseScales = 213,
                [Comment("吸血")]
                BloodSucking = 214,
                [Comment("針のむしろ")]
                RatherOfNeedles = 215,
                [Comment("蟻地獄")]
                AnAntHill = 216,
                [Comment("PI")]
                PI = 217,
                [Comment("悪夢")]
                Nightmare = 218,
                [Comment("烈火")]
                FieryFire = 219,
                [Comment("降霊アンデッドに攻撃の指令を与える")]
                GiveTheCommandOfAttackToTheDescentEvilUndead = 220,
                [Comment("陰謀の影")]
                ShadowOfConspiracy = 221,
                [Comment("死の香り")]
                TheScentOfDeath = 222,
                [Comment("引き寄せ")]
                Attract = 223,
                [Comment("INVALID")]
                INVALID_30 = 224,
                [Comment("バイブレ")]
                Vibre = 225,
                [Comment("INVALID")]
                INVALID_31 = 226,
                [Comment("INVALID")]
                INVALID_32 = 227,
                [Comment("INVALID")]
                INVALID_33 = 228,
                [Comment("INVALID")]
                INVALID_34 = 229,
                [Comment("INVALID")]
                INVALID_35 = 230,
                [Comment("敵と契約を結ぶ")]
                ISignAContractWithTheEnemy = 231,
                [Comment("回復率低下")]
                ReductionInRecoveryRate = 232,
                [Comment("復活邪魔")]
                ResurrectionDisturbance = 233,
                [Comment("INVALID")]
                INVALID_36 = 234,
                [Comment("一定のレベルと回数に応じて防具を使えなくする")]
                DisableArmorAccordingToACertainLevelAndNumberOfTimes = 235,
                [Comment("魂の誓約")]
                ASoulPledge = 236,
                [Comment("血の誓約")]
                BloodPledge = 237,
                [Comment("契約スキルの効果が消える")]
                TheEffectOfContractSkillDisappears = 238,
                [Comment("不明")]
                Unknown_3 = 239,
                [Comment("不明")]
                Unknown_4 = 240,
                [Comment("不明")]
                Unknown_5 = 241,
                [Comment("最終攻撃力%上昇")]
                LastAttackPowerIncreasePercent = 242,
                [Comment("INVALID")]
                INVALID_37 = 243,
                [Comment("不明")]
                Unknown_6 = 244,
                [Comment("INVALID")]
                INVALID_38 = 245,
                [Comment("INVALID")]
                INVALID_39 = 246,
                [Comment("不明")]
                Unknown_7 = 247,
                [Comment("不明")]
                Unknown_8 = 248,
                [Comment("INVALID")]
                INVALID_40 = 249,
                [Comment("不明")]
                Unknown_9 = 250,
                [Comment("不明")]
                Unknown_10 = 251,
                [Comment("不明")]
                Unknown_11 = 252,
                [Comment("INVALID")]
                INVALID_41 = 253,
                [Comment("不明")]
                Unknown_12 = 254,
                [Comment("INVALID")]
                INVALID_42 = 255,
                [Comment("INVALID")]
                INVALID_43 = 256,
                [Comment("INVALID")]
                INVALID_44 = 257,
                [Comment("INVALID")]
                INVALID_45 = 258,
                [Comment("INVALID")]
                INVALID_46 = 259,
                [Comment("INVALID")]
                INVALID_47 = 260,
                [Comment("INVALID")]
                INVALID_48 = 261,
                [Comment("INVALID")]
                INVALID_49 = 262,
                [Comment("INVALID")]
                INVALID_50 = 263,
                [Comment("武器装備不可")]
                NoWeaponEquipped = 264,
                [Comment("攻撃力低下(腕斬)")]
                AttackPowerDecrease_ARMSWORD = 265,
                [Comment("Reスキル発動不可")]
                ReSkillCanNotBeActivated = 266,
                [Comment("アセンブル")]
                Assemble = 267,
                [Comment("全能力上昇")]
                FullCapacityRise = 268,
                [Comment("防御力・回避率低下")]
                DefensePowerAvoidanceReduction = 269,
                [Comment("致命打攻撃力上昇")]
                MortalBattingAttackPowerIncrease = 270,
                [Comment("略奪CP")]
                LootedCP = 271,
                [Comment("バフ逆転")]
                BuffReversal = 272,
                [Comment("継続火ダメージ")]
                ContinuedFireDamage = 273,
                [Comment("火ダメージ(レイジ)")]
                FireDamage_RAGE = 274,
                [Comment("スキルLv1制限")]
                SkillLv1Limit = 275,
                [Comment("次の攻撃の増加")]
                IncreaseOfNextAttack = 276,
                [Comment("INVALID")]
                INVALID_51 = 277,
                [Comment("雷撃破持続時間↑")]
                LightningDurationUp = 278,
                [Comment("INVALID")]
                INVALID_52 = 279,
                [Comment("ブロック無効")]
                BlockInvalid = 280,
                [Comment("INVALID")]
                INVALID_53 = 281,
                [Comment("敵に3倍のダメージを与える攻撃を繰り出す")]
                Attack3 = 282,
                [Comment("敵を放り投げる（霊術師）")]
                ThrowAway = 283,
                [Comment("分身攻撃回数（霊術師　蛇の目拳）")]
                DivisionAttack = 284,
                [Comment("ビット（光奏師）")]
                Bit = 285,
                [Comment("INVALID")]
                INVALID_56 = 286,
                [Comment("出血時、最終ダメージのn％を2秒間隔で追加")]
                ReceiveAnyRateOfFinalDamageWhenBleeding = 287,
                [Comment("敵は攻撃を受けるごとにダメージのn％に該当する感電ダメージを追加")]
                ElectricShockByAttack = 288,
                [Comment("ヒール系呪文をうけた時，治療量のn%を即時回復")]
                ImmediateRecoveryByHealing = 289,
                [Comment("効果を受けたActorの種族をアンデッドに変更")]
                ChangeUndead = 290,

                [Comment("NONE")]
                None = ushort.MaxValue,
            }
        }
    }
}
