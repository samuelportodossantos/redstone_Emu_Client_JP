using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static RedStoneLib.Model.Base.Actor;
using static RedStoneLib.Model.Player;

namespace RedStoneLib.Model.Effect
{
    /// <summary>
    /// プレイヤーに係るエフェクト
    /// </summary>
    public partial class PlayerEffect
    {
        /// <summary>
        /// ステータス+
        /// </summary>
        public ActorStatus<int> Status;

        /// <summary>
        /// ステータス比率上昇
        /// </summary>
        public ActorStatus<double> StatusRaiseRetio;

        /// <summary>
        /// ステータス固定
        /// </summary>
        public ActorStatus<MaxInt32> FixationStatus;

        /// <summary>
        /// 最大・最小ダメージ
        /// </summary>
        public Scale<int> DamageScale;

        /// <summary>
        /// 防御力
        /// </summary>
        public int Defense;

        /// <summary>
        /// 最大HP
        /// </summary>
        public int MaxHP;

        /// <summary>
        /// 最大CP
        /// </summary>
        public int MaxCP;

        /// <summary>
        /// 最大HP%
        /// </summary>
        public int MaxHPRate;

        /// <summary>
        /// 最大CP%
        /// </summary>
        public int MaxCPRate;

        /// <summary>
        /// 能力上昇系（％）
        /// </summary>
        public RateState AbilityRate;

        /// <summary>
        /// 低下系抵抗
        /// </summary>
        public int DeclineResistance;

        /// <summary>
        /// 呪い系抵抗
        /// </summary>
        public int SpellResistance;

        /// <summary>
        /// 減少限界CP
        /// </summary>
        public int DecliningLimitCP;

        /// <summary>
        /// 職業ごとのスキルレベル
        /// </summary>
        public JobType JobSkillLevel;

        /// <summary>
        /// HP回復 +([0]/10秒)
        /// </summary>
        public int HPRecoveryPlusAny10Seconds;

        /// <summary>
        /// 召喚獣のすべてのステータスが[0]増加
        /// </summary>
        public int AllStatusOfSummonedBeastIsIncreased;

        /// <summary>
        /// RED STONE獲得可能レベル [+0]
        /// </summary>
        public int REDSTONEAcceptableLevel;

        /// <summary>
        /// [0]％の確率で火属性ダメージ [0]追加
        /// </summary>
        public int FireAttribute_AdditionalAnyPercent;

        /// <summary>
        /// ブロック率
        /// </summary>
        public int BlockRatio;

        /// <summary>
        /// ブロッキング速度
        /// </summary>
        public int BlockingSpeed;

        /// <summary>
        /// ポーション回復速度
        /// </summary>
        public int PotionRecoverySpeed;

        /// <summary>
        /// CP獲得ボーナス [0]％
        /// </summary>
        public int CPEarnedBonus;

        /// <summary>
        /// 敵逃亡 [0]％
        /// </summary>
        public int EnemyEscape;

        /// <summary>
        /// 魔法攻撃
        /// </summary>
        public Magic<Scale<int>> MagicAttack;

        /// <summary>
        /// 魔法抵抗
        /// </summary>
        public Magic<int> MagicResistance;

        /// <summary>
        /// 属性ダメージ吸収 [0]％
        /// </summary>
        public Magic<int> MagicDamageAbsorption;

        /// <summary>
        /// 呪い攻撃
        /// </summary>
        public ActorSpell<ProbAndTime> SpellAttack;

        /// <summary>
        /// 状態異常攻撃
        /// </summary>
        public StatusAbnormal<ProbAndTime> AbnormalAttack;

        /// <summary>
        /// 状態異常抵抗
        /// </summary>
        public StatusAbnormal<int> AbnormalAttackResistance;

        /// <summary>
        /// 弱点攻撃
        /// </summary>
        public PlayerStatusWeakness WeaknessAttack;

        /// <summary>
        /// 弱点攻撃抵抗
        /// </summary>
        public PlayerStatusWeakness WeaknessAttackResistance;

        /// <summary>
        /// ダメージ返し [0]％
        /// </summary>
        public int DamageReturned;

        /// <summary>
        /// ダメージをCPに転換 [0]％
        /// </summary>
        public int DamageConvertedToCP;

        /// <summary>
        /// カウンターフリーズ [0]％([1]秒)
        /// </summary>
        public ProbAndTime CounterFreeze;

        /// <summary>
        /// カウンターコールド [0]％([1]秒)
        /// </summary>
        public ProbAndTime CounterCold;

        /// <summary>
        /// public 攻撃を受けると 10％の確率でダメージの[0]％を応急処置
        /// </summary>
        public int IfYouReceiveAnAttack_DamageAtAChanceOf10PercentWillBeTreatedFirst;

        /// <summary>
        /// 武器交換速度
        /// </summary>
        public int WeaponReplacementSpeed;

        /// <summary>
        /// 復活
        /// </summary>
        public int Resurrection;

        /// <summary>
        /// 魔法アイテムドロップ確率
        /// </summary>
        public int MagicItemDropProbability;

        /// <summary>
        /// ユニークアイテムドロップ確率
        /// </summary>
        public int UniqueItemDropProbability;

        /// <summary>
        /// アイテムリロードタイム
        /// </summary>
        public int ItemReloadTime;

        /// <summary>
        /// ペット経験値ボーナス
        /// </summary>
        public int PetExperienceBonusBonus;

        /// <summary>
        /// ターゲットの抵抗を[0]％弱化させる。
        /// </summary>
        public Magic<int> WeakenMagicResistance;

        /// <summary>
        /// 魔法攻撃力を[0]％強化させる。
        /// </summary>
        public Magic<int> IncreaseMagicAttackPower;

        /// <summary>
        /// 種族ダメージ  [+0]％
        /// </summary>
        public ActorRaceType DamageOfRace;

        /// <summary>
        /// 強打[+0]％
        /// </summary>
        public int Bang;

        /// <summary>
        /// [0]％の確率で受けたダメージ [0]％減少
        /// </summary>
        public int DamageReceivedAtAnyPercentProbabilityReceivedAnyPercentDecrease;

        /// <summary>
        /// [0]％確率で物理ダメージの[1]％ を 体力吸収
        /// </summary>
        public (int prob, int rate) AbsorbanceAnyAmountOfDamageGivenToEnemies;

        /// <summary>
        /// スキル難易度[0]以下のスキルレベルが[1]増加する。
        /// </summary>
        public int[] SkillLevelLessThanAnySkillDifficultyLevelIncrease = new int[5];

        /// <summary>
        /// 獲得経験値 [0]％増加
        /// </summary>
        public int IncreaseExp;

        /// <summary>
        /// ターゲットの回避率補正値 無視
        /// </summary>
        public int IgnoreTargetAvoidanceRatioCorrectionValue;

        /// <summary>
        /// 攻撃者の命中率補正値 無視
        /// </summary>
        public int IgnoreHitRateCorrectionValueOfAttacker;

        /// <summary>
        /// ブラー
        /// </summary>
        public int Blur;

        /// <summary>
        /// 透明
        /// </summary>
        public int Transparent;

        /// <summary>
        /// 空中浮遊
        /// </summary>
        public int FloatingInTheAir;

        /// <summary>
        /// 先攻されない
        /// </summary>
        public int AvoidanceTarget;

        /// <summary>
        /// 自動リロード
        /// </summary>
        public int ItemAutomaticReloading;

        /// <summary>
        /// 基本攻撃力
        /// </summary>
        Scale<ushort> _ItemBasicAttack;
        public Scale<ushort> ItemBasicAttack
        {
            get => _ItemBasicAttack;
            set
            {
                _ItemBasicAttack = value;
                OnChangeStatus?.Invoke(ChangeStatusInfo.AttackPower, this);
            }
        }

        /// <summary>
        /// 確率と時間
        /// </summary>
        public struct ProbAndTime : IFormattable
        {
            int Prob;
            float Time;

            public static ProbAndTime operator +(ProbAndTime a, (int, float) b)
            {
                a.Prob += b.Item1;
                a.Time += b.Item2;
                return a;
            }

            public static ProbAndTime operator -(ProbAndTime a, (int, float) b)
            {
                a.Prob -= b.Item1;
                a.Time -= b.Item2;
                return a;
            }

            public override string ToString()
                => $"{Prob}％, {Time}秒";

            public string ToString(string format, IFormatProvider formatProvider)
                => ToString();
        }

        /// <summary>
        /// 変動するステータス
        /// </summary>
        [Flags]
        public enum ChangeStatusInfo : ushort
        {
            Status = 0x01,
            HP = 0x02,
            CP = 0x04,
            MoveSpeed = 0x08,
            AttackPower = 0x10,
            Defence = 0x20,
            MagicResistance = 0x40,
        }

        public delegate void ChangeStatusDelegate(ChangeStatusInfo info, PlayerEffect pe, int? peIndex = null);

        /// <summary>
        /// ステータス変更イベント
        /// </summary>
        public event ChangeStatusDelegate OnChangeStatus;

        /// <summary>
        /// アイテムエフェクト辞書
        /// </summary>
        private Dictionary<uint, (ItemEffect effect, byte[] v)> ItemEffectDic = new Dictionary<uint, (ItemEffect effect, byte[] v)>();

        /// <summary>
        /// OPエフェクト辞書
        /// </summary>
        private Dictionary<uint, (OPEffect effect, ushort[] v)> OPEffectDic = new Dictionary<uint, (OPEffect effect, ushort[] v)>();

        /// <summary>
        /// 重複するエフェクトのカウント
        /// </summary>
        private Dictionary<uint, int> EffectCount = new Dictionary<uint, int>();

        /// <summary>
        /// SetAbilityの係数
        /// </summary>
        private const int plus = 1;
        private const int minus = -1;

        private const uint FNV_OFFSET_BASIS_32 = 2166136261U;
        private const uint FNV_PRIME_32 = 16777619U;

        private static uint GetHash(ushort effect, ushort[] datas)
        {
            uint hash = FNV_OFFSET_BASIS_32;
            hash = (FNV_PRIME_32 * hash) ^ (ushort)effect;
            foreach (var data in datas.Where(t => t != 0))
            {
                hash = (FNV_PRIME_32 * hash) ^ data;
            }
            return hash << 1;
        }

        /// <summary>
        /// ハッシュ取得
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="datas"></param>
        /// <returns></returns>
        private static uint GetHash(ItemEffect effect, byte[] datas)
            => GetHash((ushort)effect, datas.Select(t => (ushort)t).ToArray());

        /// <summary>
        /// ハッシュ取得
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="datas"></param>
        /// <returns></returns>
        private static uint GetHash(OPEffect effect, ushort[] datas)
            => GetHash((ushort)effect, datas) | 1;

        /// <summary>
        /// PlayerEffect同士の加法
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static PlayerEffect operator +(PlayerEffect a, PlayerEffect b)
        {
            if (a == null) return b;
            if (b == null) return a;

            //ItemEffectコピー
            foreach (var hash in b.ItemEffectDic.Keys)
            {
                int cnt = b.EffectCount[hash];
                for (int i = 0; i < cnt; i++)
                {
                    a += b.ItemEffectDic[hash];
                }
            }
            //OPEffectコピー
            foreach (var hash in b.OPEffectDic.Keys)
            {
                int cnt = b.EffectCount[hash];
                for (int i = 0; i < cnt; i++)
                {
                    a += b.OPEffectDic[hash];
                }
            }
            return a;
        }

        /// <summary>
        /// PlayerEffect同士の減法
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static PlayerEffect operator -(PlayerEffect a, PlayerEffect b)
        {
            if (a == null) throw new ArgumentNullException("PlayerEffect:引かれる数がnull");
            if (b == null) throw new ArgumentNullException("PlayerEffect:引く数がnull");

            //ItemEffect除去
            foreach (var hash in b.ItemEffectDic.Keys)
            {
                for (int i = 0; i < b.EffectCount[hash]; i++)
                {
                    a -= b.ItemEffectDic[hash];
                }
            }
            //OPEffect除去
            foreach (var hash in b.OPEffectDic.Keys)
            {
                for (int i = 0; i < b.EffectCount[hash]; i++)
                {
                    a -= b.OPEffectDic[hash];
                }
            }
            return a;
        }

        /// <summary>
        /// ItemEffectの加法
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static PlayerEffect operator +(PlayerEffect a, (ItemEffect effect, byte[] v) b)
        {
            a.SetAbility(b.effect, plus, b.v);

            //内部辞書に書き込む
            var hash = GetHash(b.effect, b.v);
            if (a.ItemEffectDic.ContainsKey(hash))
            {
                a.EffectCount[hash] += 1;
            }
            else
            {
                a.ItemEffectDic[hash] = (b.effect, b.v);
                a.EffectCount[hash] = 1;
            }
            return a;
        }

        /// <summary>
        /// ItemEffectの減法
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static PlayerEffect operator -(PlayerEffect a, (ItemEffect effect, byte[] v) b)
        {
            a.SetAbility(b.effect, minus, b.v);

            //内部辞書から除去する
            var hash = GetHash(b.effect, b.v);
            if (a.EffectCount[hash] == 1)
            {
                a.ItemEffectDic.Remove(hash);
                a.EffectCount.Remove(hash);
            }
            else
            {
                a.EffectCount[hash] -= 1;
            }
            return a;
        }

        /// <summary>
        /// OPEffectの加法
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static PlayerEffect operator +(PlayerEffect a, (OPEffect effect, ushort[] v) b)
        {
            a.SetAbility(b.effect, plus, b.v);

            //内部辞書に書き込む
            var hash = GetHash(b.effect, b.v);
            if (a.OPEffectDic.ContainsKey(hash))
            {
                a.EffectCount[hash] += 1;
            }
            else
            {
                a.OPEffectDic[hash] = (b.effect, b.v);
                a.EffectCount[hash] = 1;
            }
            return a;
        }

        /// <summary>
        /// OPEffectの減法
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static PlayerEffect operator -(PlayerEffect a, (OPEffect effect, ushort[] v) b)
        {
            a.SetAbility(b.effect, minus, b.v);

            //内部辞書から除去する
            var hash = GetHash(b.effect, b.v);
            if (a.EffectCount[hash] == 1)
            {
                a.OPEffectDic.Remove(hash);
                a.EffectCount.Remove(hash);
            }
            else
            {
                a.EffectCount[hash] -= 1;
            }
            return a;
        }


        /// <summary>
        /// 性別が男
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        private static bool IsMen(JOB job)
        {
            switch (job)
            {
                case JOB.Swordsman:
                case JOB.Wizard:
                case JOB.Bishop:
                case JOB.Thief:
                case JOB.Warrior:
                case JOB.Wolfman:
                case JOB.Angel:
                case JOB.Monk:
                case JOB.LightMaster:
                    return true;
                case JOB.Lancer:
                case JOB.Tamer:
                case JOB.Princess:
                case JOB.Necro:
                case JOB.Archer:
                case JOB.Summoner:
                case JOB.LittleWitch:
                case JOB.Demon:
                case JOB.NumerologyTeacher:
                case JOB.Fighter:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException("職業が正しくありません");
            }
        }

        /// <summary>
        /// 近接系列
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        private static bool IsProximitySeriesOccupation(JOB job)
        {
            switch (job)
            {
                case JOB.Swordsman:
                case JOB.Warrior:
                case JOB.Lancer:
                case JOB.Archer:
                case JOB.Bishop:
                case JOB.Tamer:
                case JOB.Monk://武闘家
                case JOB.Wolfman:
                case JOB.Necro:
                case JOB.LittleWitch:
                case JOB.NumerologyTeacher://霊術師
                case JOB.Fighter://闘士
                    return true;
                case JOB.Wizard:
                case JOB.Thief:
                case JOB.Summoner:
                case JOB.Angel:
                case JOB.Princess:
                case JOB.Demon://悪魔
                case JOB.LightMaster://光奏師
                    return false;
                default:
                    throw new ArgumentOutOfRangeException("職業が正しくありません");
            }
        }
    }
}
    
