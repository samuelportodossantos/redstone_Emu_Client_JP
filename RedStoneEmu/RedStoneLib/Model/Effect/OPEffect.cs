using RedStoneLib.Model.Effect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Model.Effect
{
    /// <summary>
    /// OPの効果に関する処理
    /// </summary>
    public partial class PlayerEffect
    {
        /// <summary>
        /// objの全値をvalueにセット
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        private static void SetValueInAllFields<T>(ref T obj, object value)
        {
            foreach (var field in typeof(T).GetFields())
            {
                field.SetValueDirect(__makeref(obj), value);
            }
        }

        /// <summary>
        /// アビリティの取得
        /// </summary>
        /// <param name="level"></param>
        /// <param name="job"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public void SetAbility(OPEffect m_value, int term, params ushort[] v)
        {
            switch (m_value)
            {
                /// <summary>
                /// 力 [+0]
                /// </summary>
                case OPEffect.StrengthPlusAny:
                    Status.Strength += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.Status, this, (int?)ActorStatusType.Strength);
                    break;
                /// <summary>
                /// 敏捷 [+0]
                /// </summary>
                case OPEffect.AgilityPlusAny:
                    Status.Agility += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.Status, this, (int?)ActorStatusType.Agility);
                    break;
                /// <summary>
                /// 健康 [+0]
                /// </summary>
                case OPEffect.ConditionPlusAny:
                    Status.Condition += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.Status, this, (int?)ActorStatusType.Condition);
                    break;
                /// <summary>
                /// 知恵 [+0]
                /// </summary>
                case OPEffect.WisdomPlusAny:
                    Status.Wisdom += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.Status, this, (int?)ActorStatusType.Wisdom);
                    break;
                /// <summary>
                /// 知識 [+0]
                /// </summary>
                case OPEffect.InteligencePlusAny:
                    Status.Inteligence += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.Status, this, (int?)ActorStatusType.Inteligence);
                    break;
                /// <summary>
                /// カリスマ [+0]
                /// </summary>
                case OPEffect.CharismaPlusAny:
                    Status.Charisma += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.Status, this, (int?)ActorStatusType.Charisma);
                    break;
                /// <summary>
                /// 運 [+0]
                /// </summary>
                case OPEffect.LuckeyPlusAny:
                    Status.Luckey += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.Status, this, (int?)ActorStatusType.Luckey);
                    break;
                /// <summary>
                /// 力固定 [0]
                /// </summary>
                case OPEffect.StrengthFixationAny:
                    FixationStatus.Strength += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.Status, this, (int?)ActorStatusType.Strength);
                    break;
                /// <summary>
                /// 敏捷固定 [0]
                /// </summary>
                case OPEffect.AgilityFixationAny:
                    FixationStatus.Agility += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.Status, this, (int?)ActorStatusType.Agility);
                    break;
                /// <summary>
                /// 健康固定 [0]
                /// </summary>
                case OPEffect.ConditionFixationAny:
                    FixationStatus.Condition += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.Status, this, (int?)ActorStatusType.Condition);
                    break;
                /// <summary>
                /// 知恵固定 [0]
                /// </summary>
                case OPEffect.WisdomFixationAny:
                    FixationStatus.Wisdom += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.Status, this, (int?)ActorStatusType.Wisdom);
                    break;
                /// <summary>
                /// 知識固定 [0]
                /// </summary>
                case OPEffect.InteligenceFixationAny:
                    FixationStatus.Inteligence += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.Status, this, (int?)ActorStatusType.Inteligence);
                    break;
                /// <summary>
                /// カリスマ固定 [0]
                /// </summary>
                case OPEffect.CharismaFixationAny:
                    FixationStatus.Charisma += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.Status, this, (int?)ActorStatusType.Charisma);
                    break;
                /// <summary>
                /// 運固定 [0]
                /// </summary>
                case OPEffect.LuckeyFixationAny:
                    FixationStatus.Luckey += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.Status, this, (int?)ActorStatusType.Luckey);
                    break;
                /// <summary>
                /// 力 [+1]/レベル [0]
                /// </summary>
                case OPEffect.StrengthPlusAnyLevelAny:
                    StatusRaiseRetio.Strength += v[1] / v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.Status, this, (int?)ActorStatusType.Strength);
                    break;
                /// <summary>
                /// 敏捷 [+1]/レベル [0]
                /// </summary>
                case OPEffect.AgilityPlusAnyLevelAny:
                    StatusRaiseRetio.Agility += v[1] / v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.Status, this, (int?)ActorStatusType.Agility);
                    break;
                /// <summary>
                /// 健康 [+1]/レベル [0]
                /// </summary>
                case OPEffect.ConditionPlusAnyLevelAny:
                    StatusRaiseRetio.Condition += v[1] / v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.Status, this, (int?)ActorStatusType.Condition);
                    break;
                /// <summary>
                /// 知恵 [+1]/レベル [0]
                /// </summary>
                case OPEffect.WisdomPlusAnyLevelAny:
                    StatusRaiseRetio.Wisdom += v[1] / v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.Status, this, (int?)ActorStatusType.Wisdom);
                    break;
                /// <summary>
                /// 知識 [+1]/レベル [0]
                /// </summary>
                case OPEffect.InteligencePlusAnyLevelAny:
                    StatusRaiseRetio.Inteligence += v[1] / v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.Status, this, (int?)ActorStatusType.Inteligence);
                    break;
                /// <summary>
                /// カリスマ [+1]/レベル [0]
                /// </summary>
                case OPEffect.CharismaPlusAnyLevelAny:
                    StatusRaiseRetio.Charisma += v[1] / v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.Status, this, (int?)ActorStatusType.Charisma);
                    break;
                /// <summary>
                /// 運 [+1]/レベル [0]
                /// </summary>
                case OPEffect.LuckeyPlusAnyLevelAny:
                    StatusRaiseRetio.Luckey += v[1] / v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.Status, this, (int?)ActorStatusType.Luckey);
                    break;
                /// <summary>
                /// ダメージ [+0]％
                /// </summary>
                case OPEffect.DamagePlusAnyPercent:
                    AbilityRate.Damage += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.AttackPower, this);
                    break;
                /// <summary>
                /// 最小ダメージ [+0]
                /// </summary>
                case OPEffect.MinimumDamagePlusAny:
                    DamageScale.Min += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.AttackPower, this);
                    break;
                /// <summary>
                /// 最大ダメージ [+0]
                /// </summary>
                case OPEffect.MaximumDamagePlusAny:
                    DamageScale.Max += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.AttackPower, this);
                    break;
                /// <summary>
                /// 防御力 [+0]％
                /// </summary>
                case OPEffect.DefensePlusAnyPercent:
                    AbilityRate.Defense += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.Defence, this);
                    break;
                /// <summary>
                /// 防御力 [+0]
                /// </summary>
                case OPEffect.DefensePlusAny:
                    Defense += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.Defence, this);
                    break;
                /// <summary>
                /// 最大HP [+0]％
                /// </summary>
                case OPEffect.MaxHPPlusAnyPercent:
                    MaxHPRate += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.HP, this);
                    break;
                /// <summary>
                /// 最大HP [+0]
                /// </summary>
                case OPEffect.MaxHPPlusAny:
                    MaxHP += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.HP, this);
                    break;
                /// <summary>
                /// 最大CP [+0]％
                /// </summary>
                case OPEffect.MaxCPPlusAnyPercent:
                    MaxCPRate += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.CP, this);
                    break;
                /// <summary>
                /// 最大CP [+0]
                /// </summary>
                case OPEffect.MaxCPPlusAny:
                    MaxCP += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.CP, this);
                    break;
                /// <summary>
                /// 減少限界CP [+0]
                /// </summary>
                case OPEffect.DecliningLimitCPPlusAny:
                    DecliningLimitCP += v[0] * term;
                    break;
                /// <summary>
                /// 命中率 [+0]％
                /// </summary>
                case OPEffect.AccuracyRatePlusAnyPercent:
                    AbilityRate.Accuracy += v[0] * term;
                    break;
                /// <summary>
                /// 回避率 [+0]％
                /// </summary>
                case OPEffect.AvoidanceRatePlusAnyPercent:
                    AbilityRate.Avoidance += v[0] * term;
                    break;
                /// <summary>
                /// ブロック率 [+0]％
                /// </summary>
                case OPEffect.BlockRatioPlusAnyPercent:
                    BlockRatio += v[0] * term;
                    break;
                /// <summary>
                /// 致命打発動確率 [+0]％
                /// </summary>
                case OPEffect.TheFatalHitProbabilityPlusAnyPercent:
                    WeaknessAttack.FatalHit += v[0] * term;
                    break;
                /// <summary>
                /// 決定打発動確率 [+0]％
                /// </summary>
                case OPEffect.DeterminationHittingProbabilityPlusAnyPercent:
                    WeaknessAttack.DeterminationHit += v[0] * term;
                    break;
                /// <summary>
                /// ターゲットの回避率補正値 無視
                /// </summary>
                case OPEffect.IgnoreTargetAvoidanceRatioCorrectionValue:
                    IgnoreTargetAvoidanceRatioCorrectionValue += 1 * term;
                    break;
                /// <summary>
                /// 攻撃者の命中率補正値 無視 
                /// </summary>
                case OPEffect.IgnoreHitRateCorrectionValueOfAttacker:
                    IgnoreHitRateCorrectionValueOfAttacker += 1 * term;
                    break;
                /// <summary>
                /// 命中率 100％
                /// </summary>
                case OPEffect.AccuracyRate100Percent:
                    AbilityRate.Accuracy += 100 * term;
                    break;
                /// <summary>
                /// ブロッキング速度 [+0]％
                /// </summary>
                case OPEffect.BlockingRatePlusAnyPercent:
                    BlockingSpeed += v[0] * term;
                    break;
                /// <summary>
                /// 移動速度 [+0]％
                /// </summary>
                case OPEffect.MovingSpeedPlusAnyPercent:
                    AbilityRate.MovingSpeed += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.MoveSpeed, this);
                    break;
                /// <summary>
                /// 攻撃速度 [+0]％
                /// </summary>
                case OPEffect.AttackSpeedPlusAnyPercent:
                    AbilityRate.AttackSpeed += v[0] * term;
                    break;
                /// <summary>
                /// ポーション回復速度 [+0]％増加
                /// </summary>
                case OPEffect.PotionRecoverySpeedPlusAnyPercentIncrease:
                    PotionRecoverySpeed += v[0] * term;
                    break;
                /// <summary>
                /// 集中力 [+0]％
                /// </summary>
                case OPEffect.ConcentrationPlusAnyPercent:
                    AbilityRate.Concentration += v[0] * term;
                    break;
                /// <summary>
                /// 火ダメージ [0]～[1]
                /// </summary>
                case OPEffect.FireDamageAnyToAny:
                    MagicAttack.Fire += new Scale<int>(v[0] * term,  v[1] * term);
                    break;
                /// <summary>
                /// 水ダメージ [0]～[1]- コールド [2]Frame
                /// </summary>
                case OPEffect.WaterDamageAnyToAnyAndCold:
                    MagicAttack.Water += new Scale<int>(v[0] * term,  v[1] * term);
                    AbnormalAttack.Cold += (100 * term, v[2] / (float)16.0 * term);
                    break;
                /// <summary>
                /// 風ダメージ [0]～[1]- スタン [2]Frame
                /// </summary>
                case OPEffect.WindDamageAnyToAnyAndStan:
                    MagicAttack.Wind += new Scale<int>(v[0] * term,  v[1] * term);
                    AbnormalAttack.Stun += (100 * term, v[2] / (float)16.0 * term);
                    break;
                /// <summary>
                /// [2]秒 [0]～[1] 毒ダメージ
                /// </summary>
                case OPEffect.EarthDamageAnyToAnyAndPoison:
                    MagicAttack.Earth += new Scale<int>(v[0] * term,  v[1] * term);
                    AbnormalAttack.Poison += (100 * term,  v[2] * term);
                    break;
                /// <summary>
                /// 光ダメージ [0]～[1]- [2]秒の間 命中, 回避低下
                /// </summary>
                case OPEffect.LightDamageAnyToAnyAndHitAvoidDecrease:
                    MagicAttack.Light += new Scale<int>(v[0] * term,  v[1] * term);
                    AbnormalAttack.Darkness += (100 * term,  v[2] * term);
                    break;
                /// <summary>
                /// 闇ダメージ [0]～[1]- 呪い [2]秒
                /// </summary>
                case OPEffect.DarkDamageAnyToAnyAndCurse:
                    MagicAttack.Dark += new Scale<int>(v[0] * term,  v[1] * term);
                    SpellAttack.Spell += (100 * term,  v[2] * term);
                    break;
                /// <summary>
                /// 敵に与えたダメージ [0]％を 体力吸収
                /// </summary>
                case OPEffect.AbsorbanceAnyAmountOfDamageGivenToEnemies:
                    AbsorbanceAnyAmountOfDamageGivenToEnemies.prob += 100 * term;
                    AbsorbanceAnyAmountOfDamageGivenToEnemies.rate += v[0] * term;
                    break;
                /// <summary>
                /// CP獲得ボーナス [0]％
                /// </summary>
                case OPEffect.CPEarnedBonusAnyPercent:
                    CPEarnedBonus += v[0] * term;
                    break;
                /// <summary>
                /// 敵逃亡 [0]％
                /// </summary>
                case OPEffect.EnemyEscapeAnyPercent:
                    EnemyEscape += v[0] * term;
                    break;
                /// <summary>
                /// ノックアウト攻撃 [+0]％
                /// </summary>
                case OPEffect.KnockoutAttackPlusAnyPercent:
                    WeaknessAttack.KnockBack += v[0] * term;
                    break;
                /// <summary>
                /// 即死攻撃 [+0]％
                /// </summary>
                case OPEffect.InstantAttackPlusAnyPercent:
                    WeaknessAttack.InstantDeath += v[0] * term;
                    break;
                /// <summary>
                /// 武器破壊攻撃 [+0]％([1]秒)
                /// </summary>
                case OPEffect.WeaponDestructionAttackPlusAnyPercent_anySeconds_:
                    SpellAttack.WeaponDestroy += (v[0] * term,  v[1] * term);
                    break;
                /// <summary>
                /// 鎧破壊攻撃 [+0]％([1]秒)
                /// </summary>
                case OPEffect.ArmorDestroyAttackPlusAnyPercent_anySeconds_:
                    SpellAttack.ArmorDestroy += (v[0] * term,  v[1] * term);
                    break;
                /// <summary>
                /// コールド攻撃+ [0]％([1]秒)
                /// </summary>
                case OPEffect.ColdAttackPlusAnyPercent_anySeconds_:
                    AbnormalAttack.Cold += (v[0] * term,  v[1] * term);
                    break;
                /// <summary>
                /// フリーズ攻撃 [+0]％([1]秒)
                /// </summary>
                case OPEffect.FreezeAttackPlusAnyPercent_anySeconds_:
                    AbnormalAttack.Freeze += (v[0] * term,  v[1] * term);
                    break;
                /// <summary>
                /// 目くらまし攻撃 [+0]％([1]秒)
                /// </summary>
                case OPEffect.BlurredAttackPlusAnyPercent_anySeconds_:
                    AbnormalAttack.Darkness += (v[0] * term,  v[1] * term);
                    break;
                /// <summary>
                /// スタン攻撃 [+0]％([1]秒)
                /// </summary>
                case OPEffect.StunAttackPlusAnyPercent_anySeconds_:
                    AbnormalAttack.Stun += (v[0] * term,  v[1] * term);
                    break;
                /// <summary>
                /// 石化攻撃 [+0]％([1]秒)
                /// </summary>
                case OPEffect.PetrificationAttackPlusAnyPercent_anySeconds_:
                    AbnormalAttack.Petrification += (v[0] * term,  v[1] * term);
                    break;
                /// <summary>
                /// 混乱攻撃 [+0]％([1]秒)
                /// </summary>
                case OPEffect.ConfusionAttackPlusAnyPercent_anySeconds_:
                    AbnormalAttack.Confusion += (v[0] * term,  v[1] * term);
                    break;
                /// <summary>
                /// 睡眠攻撃 [+0]％([1]秒)
                /// </summary>
                case OPEffect.SleepAttackPlusAnyPercent_anySeconds_:
                    AbnormalAttack.Sleep += (v[0] * term,  v[1] * term);
                    break;
                /// <summary>
                /// チャーミング攻撃 [+0]％([1]秒)
                /// </summary>
                case OPEffect.CharmingAttackPlusAnyPercent_anySeconds_:
                    AbnormalAttack.Fascination += (v[0] * term,  v[1] * term);
                    break;
                /// <summary>
                /// 優先ターゲット攻撃 [+0]％([1]秒)
                /// </summary>
                case OPEffect.PreferredTargetAttackPlusAnyPercent_anySeconds_:
                    SpellAttack.PreferredTarget += (v[0] * term,  v[1] * term);
                    break;
                /// <summary>
                /// AI低下攻撃 [+0]％([1]秒)
                /// </summary>
                case OPEffect.AIDecreaseAttackPlusAnyPercent_anySeconds_:
                    SpellAttack.IntelligenceDecline += (v[0] * term,  v[1] * term);
                    break;
                /// <summary>
                /// ベルセルク攻撃 [+0]％([1]秒)
                /// </summary>
                case OPEffect.BerserkAttackPlusAnyPercent_anySeconds_:
                    SpellAttack.Berserk += (v[0] * term,  v[1] * term);
                    break;
                /// <summary>
                /// 火属性抵抗 [+0]％
                /// </summary>
                case OPEffect.FireAttributeResistancePlusAnyPercent:
                    MagicResistance.Fire += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.MagicResistance, this);
                    break;
                /// <summary>
                /// 水属性抵抗 [+0]％
                /// </summary>
                case OPEffect.WaterAttributeResistancePlusAnyPercent:
                    MagicResistance.Water += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.MagicResistance, this);
                    break;
                /// <summary>
                /// 風属性抵抗 [+0]％
                /// </summary>
                case OPEffect.WindAttributeResistancePlusAnyPercent:
                    MagicResistance.Wind += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.MagicResistance, this);
                    break;
                /// <summary>
                /// 大地属性抵抗 [+0]％
                /// </summary>
                case OPEffect.EarthAttributeResistancePlusAnyPercent:
                    MagicResistance.Earth += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.MagicResistance, this);
                    break;
                /// <summary>
                /// 光属性抵抗 [+0]％
                /// </summary>
                case OPEffect.LightAttributeResistancePlusAnyPercent:
                    MagicResistance.Light += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.MagicResistance, this);
                    break;
                /// <summary>
                /// 闇属性抵抗 [+0]％
                /// </summary>
                case OPEffect.DarkAttributeResistancePlusAnyPercent:
                    MagicResistance.Dark += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.MagicResistance, this);
                    break;
                /// <summary>
                /// 魔法抵抗 [+0]％
                /// </summary>
                case OPEffect.MagicResistancePlusAnyPercent:
                    MagicResistance.Fire += v[0] * term;
                    MagicResistance.Water += v[0] * term;
                    MagicResistance.Wind += v[0] * term;
                    MagicResistance.Earth += v[0] * term;
                    MagicResistance.Light += v[0] * term;
                    MagicResistance.Dark += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.MagicResistance, this);
                    break;
                /// <summary>
                /// 火属性ダメージ吸収 [0]％
                /// </summary>
                case OPEffect.FireAttributeDamageAbsorptionAnyPercent:
                    MagicDamageAbsorption.Fire += v[0] * term;
                    break;
                /// <summary>
                /// 水属性ダメージ吸収 [0]％
                /// </summary>
                case OPEffect.WaterAttributeDamageAbsorptionAnyPercent:
                    MagicDamageAbsorption.Water += v[0] * term;
                    break;
                /// <summary>
                /// 風属性ダメージ吸収 [0]％
                /// </summary>
                case OPEffect.WindAttributeDamageAbsorptionAnyPercent:
                    MagicDamageAbsorption.Wind += v[0] * term;
                    break;
                /// <summary>
                /// 大地属性ダメージ吸収 [0]％
                /// </summary>
                case OPEffect.EarthAttributeDamageAbsorptionAnyPercent:
                    MagicDamageAbsorption.Earth += v[0] * term;
                    break;
                /// <summary>
                /// 光属性ダメージ吸収 [0]％
                /// </summary>
                case OPEffect.LightAttributeDamageAbsorptionAnyPercent:
                    MagicDamageAbsorption.Light += v[0] * term;
                    break;
                /// <summary>
                /// 闇属性ダメージ吸収 [0]％
                /// </summary>
                case OPEffect.DarkAttackDamageAbsorptionAnyPercent:
                    MagicDamageAbsorption.Dark += v[0] * term;
                    break;
                /// <summary>
                /// 魔法属性ダメージ吸収 [0]％
                /// </summary>
                case OPEffect.MagicalAttributeDamageAbsorptionAnyPercent:
                    MagicDamageAbsorption.Fire += v[0] * term;
                    MagicDamageAbsorption.Water += v[0] * term;
                    MagicDamageAbsorption.Wind += v[0] * term;
                    MagicDamageAbsorption.Earth += v[0] * term;
                    MagicDamageAbsorption.Light += v[0] * term;
                    MagicDamageAbsorption.Dark += v[0] * term;
                    break;
                /// <summary>
                /// フリーズ抵抗 [+0]％
                /// </summary>
                case OPEffect.FreezeResistancePlusAnyPercent:
                    AbnormalAttackResistance.Freeze += v[0] * term;
                    break;
                /// <summary>
                /// コールド抵抗 [+0]％
                /// </summary>
                case OPEffect.ColdResistancePlusAnyPercent:
                    AbnormalAttackResistance.Cold += v[0] * term;
                    break;
                /// <summary>
                /// スタン抵抗 [+0]％
                /// </summary>
                case OPEffect.StunResistancePlusAnyPercent:
                    AbnormalAttackResistance.Stun += v[0] * term;
                    break;
                /// <summary>
                /// 混乱抵抗 [+0]％
                /// </summary>
                case OPEffect.ConfusionResistancePlusAnyPercent:
                    AbnormalAttackResistance.Confusion += v[0] * term;
                    break;
                /// <summary>
                /// チャーミング抵抗 [+0]％
                /// </summary>
                case OPEffect.CharmingResistancePlusAnyPercent:
                    AbnormalAttackResistance.Fascination += v[0] * term;
                    break;
                /// <summary>
                /// 石化抵抗 [+0]％
                /// </summary>
                case OPEffect.PetrochemicalResistancePlusAnyPercent:
                    AbnormalAttackResistance.Petrification += v[0] * term;
                    break;
                /// <summary>
                /// 即死抵抗 [+0]％
                /// </summary>
                case OPEffect.InstantDeathResistancePlusAnyPercent:
                    WeaknessAttackResistance.InstantDeath += v[0] * term;
                    break;
                /// <summary>
                /// 毒抵抗 [+0]％
                /// </summary>
                case OPEffect.PoisonResistancePlusAnyPercent:
                    AbnormalAttackResistance.Poison += v[0] * term;
                    break;
                /// <summary>
                /// 睡眠抵抗 [+0]％
                /// </summary>
                case OPEffect.SleepResistancePlusAnyPercent:
                    AbnormalAttackResistance.Sleep += v[0] * term;
                    break;
                /// <summary>
                /// 状態異常抵抗 [+0]％
                /// </summary>
                case OPEffect.StateAbnormalResistancePlusAnyPercent:
                    SetValueInAllFields(ref AbnormalAttackResistance, v[0] * term);
                    break;
                /// <summary>
                /// 低下系抵抗 [+0]％
                /// </summary>
                case OPEffect.DecliningSystemResistancePlusAnyPercent:
                    DeclineResistance += v[0] * term;
                    break;
                /// <summary>
                /// 呪い系抵抗 [+0]％
                /// </summary>
                case OPEffect.CurseBasedResistancePlusAnyPercent:
                    SpellResistance += v[0] * term;
                    break;
                /// <summary>
                /// すべての異常系抵抗 [+0]％
                /// </summary>
                case OPEffect.AllAbnormalSystemResistancePlusAnyPercent:
                    SetValueInAllFields(ref AbnormalAttackResistance, v[0] * term);
                    DeclineResistance += v[0] * term;
                    SpellResistance += v[0] * term;
                    break;
                /// <summary>
                /// ノックバック抵抗 [+0]％
                /// </summary>
                case OPEffect.KnockBackResistancePlusAnyPercent:
                    WeaknessAttackResistance.KnockBack += v[0] * term;
                    break;
                /// <summary>
                /// 致命打抵抗 [+0]％
                /// </summary>
                case OPEffect.LifeResistanceResistancePlusAnyPercent:
                    WeaknessAttackResistance.FatalHit += v[0] * term;
                    break;
                /// <summary>
                /// 決定打抵抗 [+0]％
                /// </summary>
                case OPEffect.DeterminationStrikingResistancePlusAnyPercent:
                    WeaknessAttackResistance.DeterminationHit += v[0] * term;
                    break;
                /// <summary>
                /// ダメージ返し [0]％
                /// </summary>
                case OPEffect.DamageReturnedAnyPercent:
                    DamageReturned += v[0] * term;
                    break;
                /// <summary>
                /// ダメージをCPに転換 [0]％
                /// </summary>
                case OPEffect.DamageConvertedToCPAnyPercent:
                    DamageConvertedToCP += v[0] * term;
                    break;
                /// <summary>
                /// カウンターフリーズ [0]％([1]秒)
                /// </summary>
                case OPEffect.CounterFreezeAnyPercent_anySeconds_:
                    CounterFreeze += (v[0] * term,  v[1] * term);
                    break;
                /// <summary>
                /// カウンターコールド [0]％([1]秒)
                /// </summary>
                case OPEffect.CounterColdAnyPercent_anySeconds_:
                    CounterCold += (v[0] * term,  v[1] * term);
                    break;
                /// <summary>
                /// スキルレベル [+0]([1]系列 職業)
                /// </summary>
                case OPEffect.SkillLevelPlusAny_anySeriesOccupation_:
                    JobSkillLevel[(Player.JOB)v[1]] += v[0] * term;
                    break;
                /// <summary>
                /// スキルレベル [+0]
                /// </summary>
                case OPEffect.SkillLevelPlusAny:
                    foreach(Player.JOB job in Enum.GetValues(typeof(Player.JOB)))
                    {
                        JobSkillLevel[job] += v[0] * term;
                    }
                    break;
                /// <summary>
                /// 攻撃を受けると 10％の確率でダメージの[0]％を応急処置
                /// </summary>
                case OPEffect.IfYouReceiveAnAttack_AnyPercentageOfDamageAtAChanceOf10PercentWillBeTreatedFirst:
                    IfYouReceiveAnAttack_DamageAtAChanceOf10PercentWillBeTreatedFirst += v[0] * term;
                    break;
                /// <summary>
                /// 武器交換速度 [+0]％
                /// </summary>
                case OPEffect.WeaponReplacementSpeedPlusAnyPercent:
                    WeaponReplacementSpeed += v[0] * term;
                    break;
                /// <summary>
                /// HP回復 +([0]/10秒)
                /// </summary>
                case OPEffect.HPRecoveryPlus_any10Seconds_:
                    HPRecoveryPlusAny10Seconds += v[0] * term;
                    break;
                /// <summary>
                /// 復活 [0]％
                /// </summary>
                case OPEffect.ResurrectionAnyPercent:
                    Resurrection += v[0] * term;
                    break;
                /// <summary>
                /// ブラー
                /// </summary>
                case OPEffect.Blur:
                    Blur += 1 * term;
                    break;
                /// <summary>
                /// 透明
                /// </summary>
                case OPEffect.Transparent:
                    Transparent += 1 * term;
                    break;
                /// <summary>
                /// 空中浮遊
                /// </summary>
                case OPEffect.FloatingInTheAir:
                    FloatingInTheAir += 1 * term;
                    break;
                /// <summary>
                /// 先攻されない
                /// </summary>
                case OPEffect.IWillNotBeForerunning:
                    AvoidanceTarget += 1 * term;
                    break;
                /// <summary>
                /// 魔法アイテムドロップ確率 [+0]％
                /// </summary>
                case OPEffect.MagicItemDropProbabilityPlusAnyPercent:
                    MagicItemDropProbability += v[0] * term;
                    break;
                /// <summary>
                /// ユニークアイテムドロップ確率 [+0]％
                /// </summary>
                case OPEffect.UniqueItemDropProbabilityPlusAnyPercent:
                    UniqueItemDropProbability += v[0] * term;
                    break;
                /// <summary>
                /// アイテムリロードタイム -[0]％
                /// </summary>
                case OPEffect.ItemReloadTimeMinusAnyPercent:
                    ItemReloadTime -= v[0] * term;
                    break;
                /// <summary>
                /// アイテム自動リロード
                /// </summary>
                case OPEffect.ItemAutomaticReloading:
                    ItemAutomaticReloading += 1 * term;
                    break;
                /// <summary>
                /// スキル難易度[0]以下のスキルレベルが[1]増加する。
                /// </summary>
                case OPEffect.SkillLevelLessThanAnySkillDifficultyLevelIncreasesAny:
                    SkillLevelLessThanAnySkillDifficultyLevelIncrease[v[0]] += v[1] * term;
                    break;
                /// <summary>
                /// 召喚獣のすべてのステータスが[0]増加
                /// </summary>
                case OPEffect.AllStatusOfSummonedBeastIsIncreasedAny:
                    AllStatusOfSummonedBeastIsIncreased += v[0] * term;
                    break;
                /// <summary>
                /// [0]％のペット経験値ボーナス
                /// </summary>
                case OPEffect.AnyPercentPetExperienceBonusBonus:
                    PetExperienceBonusBonus += v[0] * term;
                    break;
                /// <summary>
                /// ターゲットの火抵抗を[0]％弱化させる。
                /// </summary>
                case OPEffect.WeakenTheTargetFireResistanceByAnyPercentage:
                    WeakenMagicResistance.Fire += v[0] * term;
                    break;
                /// <summary>
                /// ターゲットの水抵抗を[0]％弱化させる。
                /// </summary>
                case OPEffect.WeakenTheTargetsWaterResistanceByAnyPercentage:
                    WeakenMagicResistance.Water += v[0] * term;
                    break;
                /// <summary>
                /// ターゲットの風抵抗を[0]％弱化させる。
                /// </summary>
                case OPEffect.WeakenTheTargetWindResistanceByAnyPercentage:
                    WeakenMagicResistance.Wind += v[0] * term;
                    break;
                /// <summary>
                /// ターゲットの大地抵抗を[0]％弱化させる。
                /// </summary>
                case OPEffect.WeakenTheTargetsEarthResistanceByAnyPercentage:
                    WeakenMagicResistance.Earth += v[0] * term;
                    break;
                /// <summary>
                /// ターゲットの光抵抗を[0]％弱化させる。
                /// </summary>
                case OPEffect.WeakenTheLightResistanceOfTheTargetByAnyPercentage:
                    WeakenMagicResistance.Light += v[0] * term;
                    break;
                /// <summary>
                /// ターゲットの闇抵抗を[0]％弱化させる。
                /// </summary>
                case OPEffect.WeakenTheTargetsDarkResistanceByAnyPercentage:
                    WeakenMagicResistance.Dark += v[0] * term;
                    break;
                /// <summary>
                /// ターゲットの魔法抵抗を[0]％弱化させる。
                /// </summary>
                case OPEffect.WeakenTheTargetsMagicResistanceByAnyPercentage:
                    WeakenMagicResistance.Fire += v[0] * term;
                    WeakenMagicResistance.Water += v[0] * term;
                    WeakenMagicResistance.Wind += v[0] * term;
                    WeakenMagicResistance.Earth += v[0] * term;
                    WeakenMagicResistance.Light += v[0] * term;
                    WeakenMagicResistance.Dark += v[0] * term;
                    break;
                /// <summary>
                /// 火属性攻撃力を[0]％強化させる。
                /// </summary>
                case OPEffect.IncreaseFireAttributeAttackPowerByAnyPercentage:
                    IncreaseMagicAttackPower.Fire += v[0] * term;
                    break;
                /// <summary>
                /// 水属性攻撃力を[0]％強化させる。
                /// </summary>
                case OPEffect.ImproveWaterAttributeAttackPowerByAnyPercentage:
                    IncreaseMagicAttackPower.Water += v[0] * term;
                    break;
                /// <summary>
                /// 風属性攻撃力を[0]％強化させる。
                /// </summary>
                case OPEffect.IncreaseTheWindAttackPowerByAnyPercent:
                    IncreaseMagicAttackPower.Wind += v[0] * term;
                    break;
                /// <summary>
                /// 大地属性攻撃力を[0]％強化させる。
                /// </summary>
                case OPEffect.IncreaseTheEarthAttributeAttackPowerByAnyPercentage:
                    IncreaseMagicAttackPower.Earth += v[0] * term;
                    break;
                /// <summary>
                /// 光属性攻撃力を[0]％強化させる。
                /// </summary>
                case OPEffect.IncreaseTheLightAttributeAttackPowerByAnyPercentage:
                    IncreaseMagicAttackPower.Light += v[0] * term;
                    break;
                /// <summary>
                /// 闇属性攻撃力を[0]％強化させる。
                /// </summary>
                case OPEffect.ImproveTheDarkAttributeAttackPowerByAnyPercentage:
                    IncreaseMagicAttackPower.Dark += v[0] * term;
                    break;
                /// <summary>
                /// 魔法攻撃力を[0]％強化させる。
                /// </summary>
                case OPEffect.EnhanceMagicAttackPowerByAnyPercentage:
                    IncreaseMagicAttackPower.Fire += v[0] * term;
                    IncreaseMagicAttackPower.Water += v[0] * term;
                    IncreaseMagicAttackPower.Wind += v[0] * term;
                    IncreaseMagicAttackPower.Earth += v[0] * term;
                    IncreaseMagicAttackPower.Light += v[0] * term;
                    IncreaseMagicAttackPower.Dark += v[0] * term;
                    break;
                /// <summary>
                /// 力[-0]
                /// </summary>
                case OPEffect.StrengthMinusAny:
                    Status.Strength -= v[0] * term;
                    break;
                /// <summary>
                /// 敏捷性[-0]
                /// </summary>
                case OPEffect.AgilityMinusAny:
                    Status.Agility -= v[0] * term;
                    break;
                /// <summary>
                /// 健康[-0]
                /// </summary>
                case OPEffect.ConditionMinusAny:
                    Status.Condition -= v[0] * term;
                    break;
                /// <summary>
                /// 知恵[-0]
                /// </summary>
                case OPEffect.WisdomMinusAny:
                    Status.Wisdom -= v[0] * term;
                    break;
                /// <summary>
                /// 知識[-0]
                /// </summary>
                case OPEffect.InteligenceMinusAny:
                    Status.Inteligence -= v[0] * term;
                    break;
                /// <summary>
                /// カリスマ[-0]
                /// </summary>
                case OPEffect.CharismaMinusAny:
                    Status.Charisma -= v[0] * term;
                    break;
                /// <summary>
                /// 運[-0]
                /// </summary>
                case OPEffect.LuckyMinusAny:
                    Status.Luckey -= v[0] * term;
                    break;
                /// <summary>
                /// 攻撃速度[-0]％
                /// </summary>
                case OPEffect.AttackSpeedMinusAnyPercent:
                    AbilityRate.AttackSpeed -= v[0] * term;
                    break;
                /// <summary>
                /// 移動速度[-0]％
                /// </summary>
                case OPEffect.MovingSpeedMinusAnyPercent:
                    AbilityRate.MovingSpeed -= v[0] * term;
                    break;
                /// <summary>
                /// アンデッド型キャラクターに追加で[0]％のダメージを与える。
                /// </summary>
                case OPEffect.DealsAnyAmountOfDamageToTheUndeadTypeCharacter:
                    DamageOfRace.Undead += v[0] * term;
                    break;
                /// <summary>
                /// 悪魔型キャラクターに追加で[0]％のダメージを与える。
                /// </summary>
                case OPEffect.DealsAnyAmountOfDamageByAddingItToTheDemonicCharacter:
                    DamageOfRace.Devil += v[0] * term;
                    break;
                /// <summary>
                /// 人間型キャラクターに追加で[0]％のダメージを与える。
                /// </summary>
                case OPEffect.DealsAnyPercentageOfDamageToAHumanoidCharacter:
                    DamageOfRace.Human += v[0] * term;
                    break;
                /// <summary>
                /// 動物型キャラクターに追加で[0]％のダメージを与える。
                /// </summary>
                case OPEffect.DealsAnyTypeOfDamageToTheAnimalTypeCharacterInAddition:
                    DamageOfRace.Animal += v[0] * term;
                    break;
                /// <summary>
                /// 神獣型キャラクターに追加で[0]％のダメージを与える。
                /// </summary>
                case OPEffect.DeleteAnyPercentageOfDamageToTheBeastTypeCharacter:
                    DamageOfRace.GodAnimal += v[0] * term;
                    break;
                /// <summary>
                /// ポーション回復速度[0]％増加
                /// </summary>
                case OPEffect.PotionRecoverySpeedAnyPercentageIncrease:
                    PotionRecoverySpeed += v[0] * term;
                    break;
                /// <summary>
                /// 防御力[-0]％
                /// </summary>
                case OPEffect.DefenseMinusAnyPercent:
                    Defense -= v[0] * term;
                    break;
                /// <summary>
                /// 防御力[-0]
                /// </summary>
                case OPEffect.DefenseMinusAny:
                    Defense -= v[0] * term;
                    break;
                /// <summary>
                /// 命中率[-0]％
                /// </summary>
                case OPEffect.AccuracyRateMinusAnyPercent:
                    AbilityRate.Accuracy -= v[0] * term;
                    break;
                /// <summary>
                /// 回避率[-0]％
                /// </summary>
                case OPEffect.AvoidanceRateMinusAnyPercent:
                    AbilityRate.Avoidance -= v[0] * term;
                    break;
                /// <summary>
                /// 火ダメージ [0]～[1]
                /// </summary>
                case OPEffect.FireDamageAnyToAny2:
                    MagicAttack.Fire += new Scale<int>(v[0] * term,  v[1] * term);
                    break;
                /// <summary>
                /// 水ダメージ [0]～[1]- コールド [2]Frame
                /// </summary>
                case OPEffect.WaterDamageAnyToAnyAndCold_2_Frame2:
                    MagicAttack.Water += new Scale<int>(v[0] * term,  v[1] * term);
                    AbnormalAttack.Cold += (100 * term, v[2] / (float)16.0 * term);
                    break;
                /// <summary>
                /// 風ダメージ [0]～[1]- スタン [2]Frame
                /// </summary>
                case OPEffect.WindDamageAnyToAnyAndStan_2_Frame2:
                    MagicAttack.Wind += new Scale<int>(v[0] * term,  v[1] * term);
                    AbnormalAttack.Stun += (100 * term, v[2] / (float)16.0 * term);
                    break;
                /// <summary>
                /// [2]秒 [0]～[1] 毒ダメージ
                /// </summary>
                case OPEffect._2_SecondsAnyToAnyPoisonDamage2:
                    MagicAttack.Earth += new Scale<int>(v[0] * term,  v[1] * term);
                    AbnormalAttack.Poison += (100 * term,  v[2] * term);
                    break;
                /// <summary>
                /// 光ダメージ [0]～[1]- [2]秒の間 命中, 回避低下
                /// </summary>
                case OPEffect.AvoidAvoidanceForLightDamageAnyToAnyAnd_2_Seconds2:
                    MagicAttack.Light += new Scale<int>(v[0] * term,  v[1] * term);
                    AbnormalAttack.Darkness += (100 * term,  v[2] * term);
                    break;
                /// <summary>
                /// 闇ダメージ [0]～[1]- 呪い [2]秒
                /// </summary>
                case OPEffect.DarknessAnyToAnyAndCurse_2_Sec2:
                    MagicAttack.Dark += new Scale<int>(v[0] * term,  v[1] * term);
                    SpellAttack.Spell += (100 * term,  v[2] * term);
                    break;
                /// <summary>
                /// すべての能力値 [+0]
                /// </summary>
                case OPEffect.AllAbilityValuesplusAny:
                    Status.Strength += v[0] * term;
                    Status.Agility += v[0] * term;
                    Status.Condition += v[0] * term;
                    Status.Wisdom += v[0] * term;
                    Status.Inteligence += v[0] * term;
                    Status.Charisma += v[0] * term;
                    Status.Luckey += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.Status, this, (int?)(
                        ActorStatusType.Strength | ActorStatusType.Agility |
                        ActorStatusType.Condition | ActorStatusType.Wisdom |
                        ActorStatusType.Inteligence | ActorStatusType.Charisma |
                        ActorStatusType.Luckey));
                    break;
                /// <summary>
                /// 強打[+0]％
                /// </summary>
                case OPEffect.BangPlusAnyPercent:
                    Bang += v[0] * term;
                    break;
            }
        }
        
        /// <summary>
        /// OP効果
        /// </summary>
        public enum OPEffect : ushort
        {
            /// <summary>
            /// EMPTY
            /// </summary>
            [Comment("EMPTY")]
            EMPTY = ushort.MaxValue,

            /// <summary>
            /// 力 [+0]
            /// </summary>
            [Comment("力 [+0]")]
            StrengthPlusAny = 0,

            /// <summary>
            /// 敏捷 [+0]
            /// </summary>
            [Comment("敏捷 [+0]")]
            AgilityPlusAny = 1,

            /// <summary>
            /// 健康 [+0]
            /// </summary>
            [Comment("健康 [+0]")]
            ConditionPlusAny = 2,

            /// <summary>
            /// 知恵 [+0]
            /// </summary>
            [Comment("知恵 [+0]")]
            WisdomPlusAny = 3,

            /// <summary>
            /// 知識 [+0]
            /// </summary>
            [Comment("知識 [+0]")]
            InteligencePlusAny = 4,

            /// <summary>
            /// カリスマ [+0]
            /// </summary>
            [Comment("カリスマ [+0]")]
            CharismaPlusAny = 5,

            /// <summary>
            /// 運 [+0]
            /// </summary>
            [Comment("運 [+0]")]
            LuckeyPlusAny = 6,

            /// <summary>
            /// 力固定 [0]
            /// </summary>
            [Comment("力固定 [0]")]
            StrengthFixationAny = 7,

            /// <summary>
            /// 敏捷固定 [0]
            /// </summary>
            [Comment("敏捷固定 [0]")]
            AgilityFixationAny = 8,

            /// <summary>
            /// 健康固定 [0]
            /// </summary>
            [Comment("健康固定 [0]")]
            ConditionFixationAny = 9,

            /// <summary>
            /// 知恵固定 [0]
            /// </summary>
            [Comment("知恵固定 [0]")]
            WisdomFixationAny = 10,

            /// <summary>
            /// 知識固定 [0]
            /// </summary>
            [Comment("知識固定 [0]")]
            InteligenceFixationAny = 11,

            /// <summary>
            /// カリスマ固定 [0]
            /// </summary>
            [Comment("カリスマ固定 [0]")]
            CharismaFixationAny = 12,

            /// <summary>
            /// 運固定 [0]
            /// </summary>
            [Comment("運固定 [0]")]
            LuckeyFixationAny = 13,

            /// <summary>
            /// 力 [+1]/レベル [0]
            /// </summary>
            [Comment("力 [+1]/レベル [0]")]
            StrengthPlusAnyLevelAny = 14,

            /// <summary>
            /// 敏捷 [+1]/レベル [0]
            /// </summary>
            [Comment("敏捷 [+1]/レベル [0]")]
            AgilityPlusAnyLevelAny = 15,

            /// <summary>
            /// 健康 [+1]/レベル [0]
            /// </summary>
            [Comment("健康 [+1]/レベル [0]")]
            ConditionPlusAnyLevelAny = 16,

            /// <summary>
            /// 知恵 [+1]/レベル [0]
            /// </summary>
            [Comment("知恵 [+1]/レベル [0]")]
            WisdomPlusAnyLevelAny = 17,

            /// <summary>
            /// 知識 [+1]/レベル [0]
            /// </summary>
            [Comment("知識 [+1]/レベル [0]")]
            InteligencePlusAnyLevelAny = 18,

            /// <summary>
            /// カリスマ [+1]/レベル [0]
            /// </summary>
            [Comment("カリスマ [+1]/レベル [0]")]
            CharismaPlusAnyLevelAny = 19,

            /// <summary>
            /// 運 [+1]/レベル [0]
            /// </summary>
            [Comment("運 [+1]/レベル [0]")]
            LuckeyPlusAnyLevelAny = 20,

            /// <summary>
            /// ダメージ [+0]％
            /// </summary>
            [Comment("ダメージ [+0]％")]
            DamagePlusAnyPercent = 21,

            /// <summary>
            /// 最小ダメージ [+0]
            /// </summary>
            [Comment("最小ダメージ [+0]")]
            MinimumDamagePlusAny = 22,

            /// <summary>
            /// 最大ダメージ [+0]
            /// </summary>
            [Comment("最大ダメージ [+0]")]
            MaximumDamagePlusAny = 23,

            /// <summary>
            /// 防御力 [+0]％
            /// </summary>
            [Comment("防御力 [+0]％")]
            DefensePlusAnyPercent = 24,

            /// <summary>
            /// 防御力 [+0]
            /// </summary>
            [Comment("防御力 [+0]")]
            DefensePlusAny = 25,

            /// <summary>
            /// 最大HP [+0]％
            /// </summary>
            [Comment("最大HP [+0]％")]
            MaxHPPlusAnyPercent = 26,

            /// <summary>
            /// 最大HP [+0]
            /// </summary>
            [Comment("最大HP [+0]")]
            MaxHPPlusAny = 27,

            /// <summary>
            /// 最大CP [+0]％
            /// </summary>
            [Comment("最大CP [+0]％")]
            MaxCPPlusAnyPercent = 28,

            /// <summary>
            /// 最大CP [+0]
            /// </summary>
            [Comment("最大CP [+0]")]
            MaxCPPlusAny = 29,

            /// <summary>
            /// 減少限界CP [+0]
            /// </summary>
            [Comment("減少限界CP [+0]")]
            DecliningLimitCPPlusAny = 30,

            /// <summary>
            /// 命中率 [+0]％
            /// </summary>
            [Comment("命中率 [+0]％")]
            AccuracyRatePlusAnyPercent = 31,

            /// <summary>
            /// 回避率 [+0]％
            /// </summary>
            [Comment("回避率 [+0]％")]
            AvoidanceRatePlusAnyPercent = 32,

            /// <summary>
            /// ブロック率 [+0]％
            /// </summary>
            [Comment("ブロック率 [+0]％")]
            BlockRatioPlusAnyPercent = 33,

            /// <summary>
            /// 致命打発動確率 [+0]％
            /// </summary>
            [Comment("致命打発動確率 [+0]％")]
            TheFatalHitProbabilityPlusAnyPercent = 34,

            /// <summary>
            /// 決定打発動確率 [+0]％
            /// </summary>
            [Comment("決定打発動確率 [+0]％")]
            DeterminationHittingProbabilityPlusAnyPercent = 35,

            /// <summary>
            /// ターゲットの回避率補正値 無視
            /// </summary>
            [Comment("ターゲットの回避率補正値 無視")]
            IgnoreTargetAvoidanceRatioCorrectionValue = 36,

            /// <summary>
            /// 攻撃者の命中率補正値 無視 
            /// </summary>
            [Comment("攻撃者の命中率補正値 無視 ")]
            IgnoreHitRateCorrectionValueOfAttacker = 37,

            /// <summary>
            /// 命中率 100％
            /// </summary>
            [Comment("命中率 100％")]
            AccuracyRate100Percent = 38,

            /// <summary>
            /// ブロッキング速度 [+0]％
            /// </summary>
            [Comment("ブロッキング速度 [+0]％")]
            BlockingRatePlusAnyPercent = 39,

            /// <summary>
            /// 移動速度 [+0]％
            /// </summary>
            [Comment("移動速度 [+0]％")]
            MovingSpeedPlusAnyPercent = 40,

            /// <summary>
            /// 攻撃速度 [+0]％
            /// </summary>
            [Comment("攻撃速度 [+0]％")]
            AttackSpeedPlusAnyPercent = 41,

            /// <summary>
            /// ポーション回復速度 [+0]％増加
            /// </summary>
            [Comment("ポーション回復速度 [+0]％増加")]
            PotionRecoverySpeedPlusAnyPercentIncrease = 42,

            /// <summary>
            /// 集中力 [+0]％
            /// </summary>
            [Comment("集中力 [+0]％")]
            ConcentrationPlusAnyPercent = 43,

            /// <summary>
            /// 火ダメージ [0]～[1]
            /// </summary>
            [Comment("火ダメージ [0]～[1]")]
            FireDamageAnyToAny = 44,

            /// <summary>
            /// 水ダメージ [0]～[1]- コールド [2]Frame
            /// </summary>
            [Comment("水ダメージ [0]～[1]- コールド [2]Frame")]
            WaterDamageAnyToAnyAndCold = 45,

            /// <summary>
            /// 風ダメージ [0]～[1]- スタン [2]Frame
            /// </summary>
            [Comment("風ダメージ [0]～[1]- スタン [2]Frame")]
            WindDamageAnyToAnyAndStan = 46,

            /// <summary>
            /// [2]秒 [0]～[1] 毒ダメージ
            /// </summary>
            [Comment("[2]秒 [0]～[1] 毒ダメージ")]
            EarthDamageAnyToAnyAndPoison = 47,

            /// <summary>
            /// 光ダメージ [0]～[1]- [2]秒の間 命中, 回避低下
            /// </summary>
            [Comment("光ダメージ [0]～[1]- [2]秒の間 命中, 回避低下")]
            LightDamageAnyToAnyAndHitAvoidDecrease = 48,

            /// <summary>
            /// 闇ダメージ [0]～[1]- 呪い [2]秒
            /// </summary>
            [Comment("闇ダメージ [0]～[1]- 呪い [2]秒")]
            DarkDamageAnyToAnyAndCurse = 49,

            /// <summary>
            /// 敵に与えたダメージ [0]％を 体力吸収
            /// </summary>
            [Comment("敵に与えたダメージ [0]％を 体力吸収")]
            AbsorbanceAnyAmountOfDamageGivenToEnemies = 50,

            /// <summary>
            /// CP獲得ボーナス [0]％
            /// </summary>
            [Comment("CP獲得ボーナス [0]％")]
            CPEarnedBonusAnyPercent = 51,

            /// <summary>
            /// 敵逃亡 [0]％
            /// </summary>
            [Comment("敵逃亡 [0]％")]
            EnemyEscapeAnyPercent = 52,

            /// <summary>
            /// ノックアウト攻撃 [+0]％
            /// </summary>
            [Comment("ノックアウト攻撃 [+0]％")]
            KnockoutAttackPlusAnyPercent = 53,

            /// <summary>
            /// 即死攻撃 [+0]％
            /// </summary>
            [Comment("即死攻撃 [+0]％")]
            InstantAttackPlusAnyPercent = 54,

            /// <summary>
            /// 武器破壊攻撃 [+0]％([1]秒)
            /// </summary>
            [Comment("武器破壊攻撃 [+0]％([1]秒)")]
            WeaponDestructionAttackPlusAnyPercent_anySeconds_ = 55,

            /// <summary>
            /// 鎧破壊攻撃 [+0]％([1]秒)
            /// </summary>
            [Comment("鎧破壊攻撃 [+0]％([1]秒)")]
            ArmorDestroyAttackPlusAnyPercent_anySeconds_ = 56,

            /// <summary>
            /// コールド攻撃+ [0]％([1]秒)
            /// </summary>
            [Comment("コールド攻撃+ [0]％([1]秒)")]
            ColdAttackPlusAnyPercent_anySeconds_ = 57,

            /// <summary>
            /// フリーズ攻撃 [+0]％([1]秒)
            /// </summary>
            [Comment("フリーズ攻撃 [+0]％([1]秒)")]
            FreezeAttackPlusAnyPercent_anySeconds_ = 58,

            /// <summary>
            /// 目くらまし攻撃 [+0]％([1]秒)
            /// </summary>
            [Comment("目くらまし攻撃 [+0]％([1]秒)")]
            BlurredAttackPlusAnyPercent_anySeconds_ = 59,

            /// <summary>
            /// スタン攻撃 [+0]％([1]秒)
            /// </summary>
            [Comment("スタン攻撃 [+0]％([1]秒)")]
            StunAttackPlusAnyPercent_anySeconds_ = 60,

            /// <summary>
            /// 石化攻撃 [+0]％([1]秒)
            /// </summary>
            [Comment("石化攻撃 [+0]％([1]秒)")]
            PetrificationAttackPlusAnyPercent_anySeconds_ = 61,

            /// <summary>
            /// 混乱攻撃 [+0]％([1]秒)
            /// </summary>
            [Comment("混乱攻撃 [+0]％([1]秒)")]
            ConfusionAttackPlusAnyPercent_anySeconds_ = 62,

            /// <summary>
            /// 睡眠攻撃 [+0]％([1]秒)
            /// </summary>
            [Comment("睡眠攻撃 [+0]％([1]秒)")]
            SleepAttackPlusAnyPercent_anySeconds_ = 63,

            /// <summary>
            /// チャーミング攻撃 [+0]％([1]秒)
            /// </summary>
            [Comment("チャーミング攻撃 [+0]％([1]秒)")]
            CharmingAttackPlusAnyPercent_anySeconds_ = 64,

            /// <summary>
            /// 優先ターゲット攻撃 [+0]％([1]秒)
            /// </summary>
            [Comment("優先ターゲット攻撃 [+0]％([1]秒)")]
            PreferredTargetAttackPlusAnyPercent_anySeconds_ = 65,

            /// <summary>
            /// AI低下攻撃 [+0]％([1]秒)
            /// </summary>
            [Comment("AI低下攻撃 [+0]％([1]秒)")]
            AIDecreaseAttackPlusAnyPercent_anySeconds_ = 66,

            /// <summary>
            /// ベルセルク攻撃 [+0]％([1]秒)
            /// </summary>
            [Comment("ベルセルク攻撃 [+0]％([1]秒)")]
            BerserkAttackPlusAnyPercent_anySeconds_ = 67,

            /// <summary>
            /// 火属性抵抗 [+0]％
            /// </summary>
            [Comment("火属性抵抗 [+0]％")]
            FireAttributeResistancePlusAnyPercent = 68,

            /// <summary>
            /// 水属性抵抗 [+0]％
            /// </summary>
            [Comment("水属性抵抗 [+0]％")]
            WaterAttributeResistancePlusAnyPercent = 69,

            /// <summary>
            /// 風属性抵抗 [+0]％
            /// </summary>
            [Comment("風属性抵抗 [+0]％")]
            WindAttributeResistancePlusAnyPercent = 70,

            /// <summary>
            /// 大地属性抵抗 [+0]％
            /// </summary>
            [Comment("大地属性抵抗 [+0]％")]
            EarthAttributeResistancePlusAnyPercent = 71,

            /// <summary>
            /// 光属性抵抗 [+0]％
            /// </summary>
            [Comment("光属性抵抗 [+0]％")]
            LightAttributeResistancePlusAnyPercent = 72,

            /// <summary>
            /// 闇属性抵抗 [+0]％
            /// </summary>
            [Comment("闇属性抵抗 [+0]％")]
            DarkAttributeResistancePlusAnyPercent = 73,

            /// <summary>
            /// 魔法抵抗 [+0]％
            /// </summary>
            [Comment("魔法抵抗 [+0]％")]
            MagicResistancePlusAnyPercent = 74,

            /// <summary>
            /// 火属性ダメージ吸収 [0]％
            /// </summary>
            [Comment("火属性ダメージ吸収 [0]％")]
            FireAttributeDamageAbsorptionAnyPercent = 75,

            /// <summary>
            /// 水属性ダメージ吸収 [0]％
            /// </summary>
            [Comment("水属性ダメージ吸収 [0]％")]
            WaterAttributeDamageAbsorptionAnyPercent = 76,

            /// <summary>
            /// 風属性ダメージ吸収 [0]％
            /// </summary>
            [Comment("風属性ダメージ吸収 [0]％")]
            WindAttributeDamageAbsorptionAnyPercent = 77,

            /// <summary>
            /// 大地属性ダメージ吸収 [0]％
            /// </summary>
            [Comment("大地属性ダメージ吸収 [0]％")]
            EarthAttributeDamageAbsorptionAnyPercent = 78,

            /// <summary>
            /// 光属性ダメージ吸収 [0]％
            /// </summary>
            [Comment("光属性ダメージ吸収 [0]％")]
            LightAttributeDamageAbsorptionAnyPercent = 79,

            /// <summary>
            /// 闇属性ダメージ吸収 [0]％
            /// </summary>
            [Comment("闇属性ダメージ吸収 [0]％")]
            DarkAttackDamageAbsorptionAnyPercent = 80,

            /// <summary>
            /// 魔法属性ダメージ吸収 [0]％
            /// </summary>
            [Comment("魔法属性ダメージ吸収 [0]％")]
            MagicalAttributeDamageAbsorptionAnyPercent = 81,

            /// <summary>
            /// フリーズ抵抗 [+0]％
            /// </summary>
            [Comment("フリーズ抵抗 [+0]％")]
            FreezeResistancePlusAnyPercent = 82,

            /// <summary>
            /// コールド抵抗 [+0]％
            /// </summary>
            [Comment("コールド抵抗 [+0]％")]
            ColdResistancePlusAnyPercent = 83,

            /// <summary>
            /// スタン抵抗 [+0]％
            /// </summary>
            [Comment("スタン抵抗 [+0]％")]
            StunResistancePlusAnyPercent = 84,

            /// <summary>
            /// 混乱抵抗 [+0]％
            /// </summary>
            [Comment("混乱抵抗 [+0]％")]
            ConfusionResistancePlusAnyPercent = 85,

            /// <summary>
            /// チャーミング抵抗 [+0]％
            /// </summary>
            [Comment("チャーミング抵抗 [+0]％")]
            CharmingResistancePlusAnyPercent = 86,

            /// <summary>
            /// 石化抵抗 [+0]％
            /// </summary>
            [Comment("石化抵抗 [+0]％")]
            PetrochemicalResistancePlusAnyPercent = 87,

            /// <summary>
            /// 即死抵抗 [+0]％
            /// </summary>
            [Comment("即死抵抗 [+0]％")]
            InstantDeathResistancePlusAnyPercent = 88,

            /// <summary>
            /// 毒抵抗 [+0]％
            /// </summary>
            [Comment("毒抵抗 [+0]％")]
            PoisonResistancePlusAnyPercent = 89,

            /// <summary>
            /// 睡眠抵抗 [+0]％
            /// </summary>
            [Comment("睡眠抵抗 [+0]％")]
            SleepResistancePlusAnyPercent = 90,

            /// <summary>
            /// 状態異常抵抗 [+0]％
            /// </summary>
            [Comment("状態異常抵抗 [+0]％")]
            StateAbnormalResistancePlusAnyPercent = 91,

            /// <summary>
            /// 低下系抵抗 [+0]％
            /// </summary>
            [Comment("低下系抵抗 [+0]％")]
            DecliningSystemResistancePlusAnyPercent = 92,

            /// <summary>
            /// 呪い系抵抗 [+0]％
            /// </summary>
            [Comment("呪い系抵抗 [+0]％")]
            CurseBasedResistancePlusAnyPercent = 93,

            /// <summary>
            /// すべての異常系抵抗 [+0]％
            /// </summary>
            [Comment("すべての異常系抵抗 [+0]％")]
            AllAbnormalSystemResistancePlusAnyPercent = 94,

            /// <summary>
            /// ノックバック抵抗 [+0]％
            /// </summary>
            [Comment("ノックバック抵抗 [+0]％")]
            KnockBackResistancePlusAnyPercent = 95,

            /// <summary>
            /// 致命打抵抗 [+0]％
            /// </summary>
            [Comment("致命打抵抗 [+0]％")]
            LifeResistanceResistancePlusAnyPercent = 96,

            /// <summary>
            /// 決定打抵抗 [+0]％
            /// </summary>
            [Comment("決定打抵抗 [+0]％")]
            DeterminationStrikingResistancePlusAnyPercent = 97,

            /// <summary>
            /// ダメージ返し [0]％
            /// </summary>
            [Comment("ダメージ返し [0]％")]
            DamageReturnedAnyPercent = 98,

            /// <summary>
            /// ダメージをCPに転換 [0]％
            /// </summary>
            [Comment("ダメージをCPに転換 [0]％")]
            DamageConvertedToCPAnyPercent = 99,

            /// <summary>
            /// カウンターフリーズ [0]％([1]秒)
            /// </summary>
            [Comment("カウンターフリーズ [0]％([1]秒)")]
            CounterFreezeAnyPercent_anySeconds_ = 100,

            /// <summary>
            /// カウンターコールド [0]％([1]秒)
            /// </summary>
            [Comment("カウンターコールド [0]％([1]秒)")]
            CounterColdAnyPercent_anySeconds_ = 101,

            /// <summary>
            ///  [不明]
            /// </summary>
            [Comment("[不明]")]
            Invalid = 102,

            /// <summary>
            /// スキルレベル [+0]([1]系列 職業)
            /// </summary>
            [Comment("スキルレベル [+0]([1]系列 職業)")]
            SkillLevelPlusAny_anySeriesOccupation_ = 103,

            /// <summary>
            /// スキルレベル [+0]
            /// </summary>
            [Comment("スキルレベル [+0]")]
            SkillLevelPlusAny = 104,

            /// <summary>
            /// 攻撃を受けると 10％の確率でダメージの[0]％を応急処置
            /// </summary>
            [Comment("攻撃を受けると 10％の確率でダメージの[0]％を応急処置")]
            IfYouReceiveAnAttack_AnyPercentageOfDamageAtAChanceOf10PercentWillBeTreatedFirst = 105,

            /// <summary>
            /// 武器交換速度 [+0]％
            /// </summary>
            [Comment("武器交換速度 [+0]％")]
            WeaponReplacementSpeedPlusAnyPercent = 106,

            /// <summary>
            /// HP回復 +([0]/10秒)
            /// </summary>
            [Comment("HP回復 +([0]/10秒)")]
            HPRecoveryPlus_any10Seconds_ = 107,

            /// <summary>
            /// 復活 [0]％
            /// </summary>
            [Comment("復活 [0]％")]
            ResurrectionAnyPercent = 108,

            /// <summary>
            /// ブラー
            /// </summary>
            [Comment("ブラー")]
            Blur = 109,

            /// <summary>
            /// 透明
            /// </summary>
            [Comment("透明")]
            Transparent = 110,

            /// <summary>
            /// 空中浮遊
            /// </summary>
            [Comment("空中浮遊")]
            FloatingInTheAir = 111,

            /// <summary>
            /// 魔法弾丸
            /// </summary>
            [Comment("魔法弾丸")]
            MagicalBullet = 112,

            /// <summary>
            /// 無限弾丸
            /// </summary>
            [Comment("無限弾丸")]
            InfiniteBullet = 113,

            /// <summary>
            /// 先攻されない
            /// </summary>
            [Comment("先攻されない")]
            IWillNotBeForerunning = 114,

            /// <summary>
            /// 魔法アイテムドロップ確率 [+0]％
            /// </summary>
            [Comment("魔法アイテムドロップ確率 [+0]％")]
            MagicItemDropProbabilityPlusAnyPercent = 115,

            /// <summary>
            /// ユニークアイテムドロップ確率 [+0]％
            /// </summary>
            [Comment("ユニークアイテムドロップ確率 [+0]％")]
            UniqueItemDropProbabilityPlusAnyPercent = 116,

            /// <summary>
            /// アイテムリロードタイム -[0]％
            /// </summary>
            [Comment("アイテムリロードタイム -[0]％")]
            ItemReloadTimeMinusAnyPercent = 117,

            /// <summary>
            /// アイテム自動リロード
            /// </summary>
            [Comment("アイテム自動リロード")]
            ItemAutomaticReloading = 118,

            /// <summary>
            /// アイテム使用不可
            /// </summary>
            [Comment("アイテム使用不可")]
            ItemUnavailable = 119,

            /// <summary>
            /// 装備解除不可
            /// </summary>
            [Comment("装備解除不可")]
            Unenviable = 120,

            /// <summary>
            /// 追加エンチャントが不可能
            /// </summary>
            [Comment("追加エンチャントが不可能")]
            ImpossibleToAddEnchantment = 121,

            /// <summary>
            /// 火攻撃称号 1段階
            /// </summary>
            [Comment("火攻撃称号 1段階")]
            FlameAttackTitle1StepPlus = 122,

            /// <summary>
            /// 水攻撃称号 1段階
            /// </summary>
            [Comment("水攻撃称号 1段階")]
            WaterAttackTitle1StepPlus = 123,

            /// <summary>
            /// 風攻撃称号 1段階 
            /// </summary>
            [Comment("風攻撃称号 1段階 ")]
            WindAttackTitle1StepPlus = 124,

            /// <summary>
            /// 大地攻撃称号 1段階 
            /// </summary>
            [Comment("大地攻撃称号 1段階 ")]
            EarthAttackAttentionTitle1StepPlus = 125,

            /// <summary>
            /// 光攻撃称号 1段階 
            /// </summary>
            [Comment("光攻撃称号 1段階 ")]
            LightAttackTitle1StepPlus = 126,

            /// <summary>
            /// 闇攻撃称号 1段階 
            /// </summary>
            [Comment("闇攻撃称号 1段階 ")]
            DarknessAttackTitle1StepPlus = 127,

            /// <summary>
            /// スキル難易度[0]以下のスキルレベルが[1]増加する。
            /// </summary>
            [Comment("スキル難易度[0]以下のスキルレベルが[1]増加する。")]
            SkillLevelLessThanAnySkillDifficultyLevelIncreasesAny = 128,

            /// <summary>
            /// 召喚獣のすべてのステータスが[0]増加
            /// </summary>
            [Comment("召喚獣のすべてのステータスが[0]増加")]
            AllStatusOfSummonedBeastIsIncreasedAny = 129,

            /// <summary>
            /// [0]％のペット経験値ボーナス
            /// </summary>
            [Comment("[0]％のペット経験値ボーナス")]
            AnyPercentPetExperienceBonusBonus = 130,

            /// <summary>
            /// 一定期間貸与するアイテム
            /// </summary>
            [Comment("一定期間貸与するアイテム")]
            ItemsToLendForACertainPeriod = 131,

            /// <summary>
            /// ターゲットの火抵抗を[0]％弱化させる。
            /// </summary>
            [Comment("ターゲットの火抵抗を[0]％弱化させる。")]
            WeakenTheTargetFireResistanceByAnyPercentage = 132,

            /// <summary>
            /// ターゲットの水抵抗を[0]％弱化させる。
            /// </summary>
            [Comment("ターゲットの水抵抗を[0]％弱化させる。")]
            WeakenTheTargetsWaterResistanceByAnyPercentage = 133,

            /// <summary>
            /// ターゲットの風抵抗を[0]％弱化させる。
            /// </summary>
            [Comment("ターゲットの風抵抗を[0]％弱化させる。")]
            WeakenTheTargetWindResistanceByAnyPercentage = 134,

            /// <summary>
            /// ターゲットの大地抵抗を[0]％弱化させる。
            /// </summary>
            [Comment("ターゲットの大地抵抗を[0]％弱化させる。")]
            WeakenTheTargetsEarthResistanceByAnyPercentage = 135,

            /// <summary>
            /// ターゲットの光抵抗を[0]％弱化させる。
            /// </summary>
            [Comment("ターゲットの光抵抗を[0]％弱化させる。")]
            WeakenTheLightResistanceOfTheTargetByAnyPercentage = 136,

            /// <summary>
            /// ターゲットの闇抵抗を[0]％弱化させる。
            /// </summary>
            [Comment("ターゲットの闇抵抗を[0]％弱化させる。")]
            WeakenTheTargetsDarkResistanceByAnyPercentage = 137,

            /// <summary>
            /// ターゲットの魔法抵抗を[0]％弱化させる。
            /// </summary>
            [Comment("ターゲットの魔法抵抗を[0]％弱化させる。")]
            WeakenTheTargetsMagicResistanceByAnyPercentage = 138,

            /// <summary>
            /// 火属性攻撃力を[0]％強化させる。
            /// </summary>
            [Comment("火属性攻撃力を[0]％強化させる。")]
            IncreaseFireAttributeAttackPowerByAnyPercentage = 139,

            /// <summary>
            /// 水属性攻撃力を[0]％強化させる。
            /// </summary>
            [Comment("水属性攻撃力を[0]％強化させる。")]
            ImproveWaterAttributeAttackPowerByAnyPercentage = 140,

            /// <summary>
            /// 風属性攻撃力を[0]％強化させる。
            /// </summary>
            [Comment("風属性攻撃力を[0]％強化させる。")]
            IncreaseTheWindAttackPowerByAnyPercent = 141,

            /// <summary>
            /// 大地属性攻撃力を[0]％強化させる。
            /// </summary>
            [Comment("大地属性攻撃力を[0]％強化させる。")]
            IncreaseTheEarthAttributeAttackPowerByAnyPercentage = 142,

            /// <summary>
            /// 光属性攻撃力を[0]％強化させる。
            /// </summary>
            [Comment("光属性攻撃力を[0]％強化させる。")]
            IncreaseTheLightAttributeAttackPowerByAnyPercentage = 143,

            /// <summary>
            /// 闇属性攻撃力を[0]％強化させる。
            /// </summary>
            [Comment("闇属性攻撃力を[0]％強化させる。")]
            ImproveTheDarkAttributeAttackPowerByAnyPercentage = 144,

            /// <summary>
            /// 魔法攻撃力を[0]％強化させる。
            /// </summary>
            [Comment("魔法攻撃力を[0]％強化させる。")]
            EnhanceMagicAttackPowerByAnyPercentage = 145,

            /// <summary>
            /// 力[-0]
            /// </summary>
            [Comment("力[-0]")]
            StrengthMinusAny = 146,

            /// <summary>
            /// 敏捷性[-0]
            /// </summary>
            [Comment("敏捷性[-0]")]
            AgilityMinusAny = 147,

            /// <summary>
            /// 健康[-0]
            /// </summary>
            [Comment("健康[-0]")]
            ConditionMinusAny = 148,

            /// <summary>
            /// 知恵[-0]
            /// </summary>
            [Comment("知恵[-0]")]
            WisdomMinusAny = 149,

            /// <summary>
            /// 知識[-0]
            /// </summary>
            [Comment("知識[-0]")]
            InteligenceMinusAny = 150,

            /// <summary>
            /// カリスマ[-0]
            /// </summary>
            [Comment("カリスマ[-0]")]
            CharismaMinusAny = 151,

            /// <summary>
            /// 運[-0]
            /// </summary>
            [Comment("運[-0]")]
            LuckyMinusAny = 152,

            /// <summary>
            /// 移動速度[-0]％
            /// </summary>
            [Comment("移動速度[-0]％")]
            MovingSpeedMinusAnyPercent = 153,

            /// <summary>
            /// 攻撃速度[-0]％
            /// </summary>
            [Comment("攻撃速度[-0]％")]
            AttackSpeedMinusAnyPercent = 154,

            /// <summary>
            /// アンデッド型キャラクターに追加で[0]％のダメージを与える。
            /// </summary>
            [Comment("アンデッド型キャラクターに追加で[0]％のダメージを与える。")]
            DealsAnyAmountOfDamageToTheUndeadTypeCharacter = 155,

            /// <summary>
            /// 悪魔型キャラクターに追加で[0]％のダメージを与える。
            /// </summary>
            [Comment("悪魔型キャラクターに追加で[0]％のダメージを与える。")]
            DealsAnyAmountOfDamageByAddingItToTheDemonicCharacter = 156,

            /// <summary>
            /// 人間型キャラクターに追加で[0]％のダメージを与える。
            /// </summary>
            [Comment("人間型キャラクターに追加で[0]％のダメージを与える。")]
            DealsAnyPercentageOfDamageToAHumanoidCharacter = 157,

            /// <summary>
            /// 動物型キャラクターに追加で[0]％のダメージを与える。
            /// </summary>
            [Comment("動物型キャラクターに追加で[0]％のダメージを与える。")]
            DealsAnyTypeOfDamageToTheAnimalTypeCharacterInAddition = 158,

            /// <summary>
            /// 神獣型キャラクターに追加で[0]％のダメージを与える。
            /// </summary>
            [Comment("神獣型キャラクターに追加で[0]％のダメージを与える。")]
            DeleteAnyPercentageOfDamageToTheBeastTypeCharacter = 159,

            /// <summary>
            /// ポーション回復速度[0]％増加
            /// </summary>
            [Comment("ポーション回復速度[0]％増加")]
            PotionRecoverySpeedAnyPercentageIncrease = 160,

            /// <summary>
            /// 防御力[-0]％
            /// </summary>
            [Comment("防御力[-0]％")]
            DefenseMinusAnyPercent = 161,

            /// <summary>
            /// 防御力[-0]
            /// </summary>
            [Comment("防御力[-0]")]
            DefenseMinusAny = 162,

            /// <summary>
            /// 命中率[-0]％
            /// </summary>
            [Comment("命中率[-0]％")]
            AccuracyRateMinusAnyPercent = 163,

            /// <summary>
            /// 回避率[-0]％
            /// </summary>
            [Comment("回避率[-0]％")]
            AvoidanceRateMinusAnyPercent = 164,

            /// <summary>
            /// 火ダメージ [0]～[1]
            /// </summary>
            [Comment("火ダメージ [0]～[1]")]
            FireDamageAnyToAny2 = 165,

            /// <summary>
            /// 水ダメージ [0]～[1]- コールド [2]Frame
            /// </summary>
            [Comment("水ダメージ [0]～[1]- コールド [2]Frame")]
            WaterDamageAnyToAnyAndCold_2_Frame2 = 166,

            /// <summary>
            /// 風ダメージ [0]～[1]- スタン [2]Frame
            /// </summary>
            [Comment("風ダメージ [0]～[1]- スタン [2]Frame")]
            WindDamageAnyToAnyAndStan_2_Frame2 = 167,

            /// <summary>
            /// [2]秒 [0]～[1] 毒ダメージ
            /// </summary>
            [Comment("[2]秒 [0]～[1] 毒ダメージ")]
            _2_SecondsAnyToAnyPoisonDamage2 = 168,

            /// <summary>
            /// 光ダメージ [0]～[1]- [2]秒の間 命中, 回避低下
            /// </summary>
            [Comment("光ダメージ [0]～[1]- [2]秒の間 命中, 回避低下")]
            AvoidAvoidanceForLightDamageAnyToAnyAnd_2_Seconds2 = 169,

            /// <summary>
            /// 闇ダメージ [0]～[1]- 呪い [2]秒
            /// </summary>
            [Comment("闇ダメージ [0]～[1]- 呪い [2]秒")]
            DarknessAnyToAnyAndCurse_2_Sec2 = 170,

            /// <summary>
            /// [1]レベルのスキルを使用（持続時間 [3]秒）
            /// </summary>
            [Comment("[1]レベルのスキルを使用（持続時間 [3]秒）")]
            UseAnyLevelOfSkill_duration_3_Seconds_ = 171,

            /// <summary>
            /// すべての能力値 [+0]
            /// </summary>
            [Comment("すべての能力値 [+0]")]
            AllAbilityValuesplusAny = 172,

            /// <summary>
            /// [0]スキルを刻印レベルに使用
            /// </summary>
            [Comment("[0]スキルを刻印レベルに使用")]
            UseAnySkillForEngravingLevel = 173,

            /// <summary>
            /// 強打[+0]％
            /// </summary>
            [Comment("強打[+0]％")]
            BangPlusAnyPercent = 174,
        }
        
        /// <summary>
        /// 使われてない能力
        /// </summary>
        private enum NotUseAbility
        {
            /// <summary>
            /// 魔法弾丸
            /// </summary>
            MagicalBullet,

            /// <summary>
            /// 無限弾丸
            /// </summary>
            InfiniteBullet,

            /// <summary>
            /// アイテム使用不可
            /// </summary>
            ItemUnavailable,

            /// <summary>
            /// 装備解除不可
            /// </summary>
            Unenviable,

            /// <summary>
            /// 追加エンチャントが不可能
            /// </summary>
            ImpossibleToAddEnchantment,

            /// <summary>
            /// 火攻撃称号 1段階
            /// </summary>
            FlameAttackTitle1StepPlus,

            /// <summary>
            /// 水攻撃称号 1段階
            /// </summary>
            WaterAttackTitle1StepPlus,

            /// <summary>
            /// 風攻撃称号 1段階 
            /// </summary>
            WindAttackTitle1StepPlus,

            /// <summary>
            /// 大地攻撃称号 1段階 
            /// </summary>
            EarthAttackAttentionTitle1StepPlus,

            /// <summary>
            /// 光攻撃称号 1段階 
            /// </summary>
            LightAttackTitle1StepPlus,

            /// <summary>
            /// 闇攻撃称号 1段階 
            /// </summary>
            DarknessAttackTitle1StepPlus,

            /// <summary>
            /// 一定期間貸与するアイテム
            /// </summary>
            ItemsToLendForACertainPeriod,

            /// <summary>
            /// [1]レベルのスキルを使用（持続時間 [3]秒）
            /// </summary>
            UseAnyLevelOfSkill_duration_3_Seconds_,

            /// <summary>
            /// [0]スキルを刻印レベルに使用
            /// </summary>
            UseAnySkillForEngravingLevel,
        }
    }
}
