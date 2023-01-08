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
    /// アイテムの効果を扱うクラス
    /// </summary>
    public partial class PlayerEffect
    {
        /// <summary>
        /// アビリティの取得
        /// </summary>
        /// <param name="pe"></param>
        /// <param name="level"></param>
        /// <param name="job"></param>
        public void SetAbility(ItemEffect m_value, int term, byte[] v)
        {
            switch (m_value)
            {
                /// <summary>
                /// 防御力 [+0]
                /// </summary>
                case ItemEffect.DefensePowerPlusAny:
                    Defense += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.Defence, this);
                    break;
                /// <summary>
                /// ブロック率 [+0]％
                /// </summary>
                case ItemEffect.BlockRatioPlusAnyPercent:
                    BlockRatio += v[0] * term;
                    break;
                /// <summary>
                /// ターゲット即死確率 [+0]％
                /// </summary>
                case ItemEffect.TargetInstantDeathProbabilityPlusAnyPercent:
                    WeaknessAttack.InstantDeath += v[0] * term;
                    break;
                /// <summary>
                /// ベルセルク攻撃[0]％ [1]秒
                /// </summary>
                case ItemEffect.BerserkAttackAnyPercentAnySeconds:
                    SpellAttack.Berserk += (v[0] * term,  v[1] * term);
                    break;
                /// <summary>
                /// 鎧破壊攻撃[0]％ [1]秒
                /// </summary>
                case ItemEffect.ArmorDestroyAttackAnyPercentAnySeconds:
                    SpellAttack.ArmorDestroy += (v[0] * term,  v[1] * term);
                    break;
                /// <summary>
                /// 武器破壊攻撃[0]％ [1]秒
                /// </summary>
                case ItemEffect.WeaponDestructionAttackAnyPercentAnySeconds:
                    SpellAttack.WeaponDestroy += (v[0] * term,  v[1] * term);
                    break;
                /// <summary>
                /// 毒攻撃- 秒当りダメージ [1]([0]秒)
                /// </summary>
                case ItemEffect.PoisonAttackAndDamagePerSecondAny_anySeconds_:
                    AbnormalAttack.Poison += (v[1] * term,  v[0] * term);
                    break;
                /// <summary>
                /// 混乱攻撃[0]％ [1]秒
                /// </summary>
                case ItemEffect.ConfusedAttackAnyPercentAnySeconds:
                    AbnormalAttack.Confusion += (v[0] * term,  v[1] * term);
                    break;
                /// <summary>
                /// 誘惑攻撃[0]％ [1]秒
                /// </summary>
                case ItemEffect.TemptationAttackAnyAnyAnySeconds:
                    AbnormalAttack.Fascination += (v[0] * term,  v[1] * term);
                    break;
                /// <summary>
                /// スタン攻撃[0]％ [1]Frame
                /// </summary>
                case ItemEffect.StunAttackAnyPercentAnyFrame:
                    AbnormalAttack.Stun += (v[0] * term, v[1] / (float)16.0 * term);
                    break;
                /// <summary>
                /// コールド攻撃[0]％ [1]秒
                /// </summary>
                case ItemEffect.ColdAttackAnyAnyAnySeconds:
                    AbnormalAttack.Cold += (v[0] * term,  v[1] * term);
                    break;
                /// <summary>
                /// 火属性 ダメージ [+0]
                /// </summary>
                case ItemEffect.FireAttributeDamagePlusAny:
                    MagicAttack.Fire += new Scale<int>(v[0] * term,  v[0] * term);
                    break;
                /// <summary>
                /// 水属性 ダメージ  [+0]
                /// </summary>
                case ItemEffect.WaterAttributeDamagePlusAny:
                    MagicAttack.Water += new Scale<int>(v[0] * term,  v[0] * term);
                    break;
                /// <summary>
                /// 風属性 ダメージ  [+0]
                /// </summary>
                case ItemEffect.WindAttributeDamagePlusAny:
                    MagicAttack.Wind += new Scale<int>(v[0] * term,  v[0] * term);
                    break;
                /// <summary>
                /// 大地属性 ダメージ  [+0]
                /// </summary>
                case ItemEffect.EarthAttributeDamagePlusAny:
                    MagicAttack.Earth += new Scale<int>(v[0] * term,  v[0] * term);
                    break;
                /// <summary>
                /// 光属性 ダメージ  [+0]
                /// </summary>
                case ItemEffect.LightAttributeDamagePlusAny:
                    MagicAttack.Light += new Scale<int>(v[0] * term,  v[0] * term);
                    break;
                /// <summary>
                /// 闇属性 ダメージ  [+0]
                /// </summary>
                case ItemEffect.DarkAttributeDamagePlusAny:
                    MagicAttack.Dark += new Scale<int>(v[0] * term,  v[0] * term);
                    break;
                /// <summary>
                /// vs アンデッド系キャラクター - ダメージ  [+0]％
                /// </summary>
                case ItemEffect.VsUndeadCharacterAndDamagePlusAnyPercent:
                    DamageOfRace.Undead += v[0] * term;
                    break;
                /// <summary>
                /// vs 悪魔系キャラクター - ダメージ  [+0]％
                /// </summary>
                case ItemEffect.VsDevilSeriesCharacterAndDamagePlusAnyPercent:
                    DamageOfRace.Devil += v[0] * term;
                    break;
                /// <summary>
                /// vs 人間系キャラクター - ダメージ  [+0]％
                /// </summary>
                case ItemEffect.VsHumanCharacterAndDamagePlusAnyPercent:
                    DamageOfRace.Human += v[0] * term;
                    break;
                /// <summary>
                /// vs 動物系キャラクター - ダメージ  [+0]％
                /// </summary>
                case ItemEffect.VsAnimalCharacterAndDamagePlusAnyPercent:
                    DamageOfRace.Animal += v[0] * term;
                    break;
                /// <summary>
                /// vs 神獣系キャラクター - ダメージ  [+0]％
                /// </summary>
                case ItemEffect.VsGodBeastCharacterAndDamagePlusAnyPercent:
                    DamageOfRace.GodAnimal += v[0] * term;
                    break;
                /// <summary>
                /// アイテム リロードタイム -[0]％
                /// </summary>
                case ItemEffect.ItemReloadTimeMinusAnyPercent:
                    ItemReloadTime -= v[0] * term;
                    break;
                /// <summary>
                /// 変身速度 [0]％ 増加
                /// </summary>
                case ItemEffect.TransformationSpeedAnyPercentageIncrease:
                    WeaponReplacementSpeed += v[0] * term;
                    break;
                /// <summary>
                /// ポーション  回復速度 [0]％ 増加
                /// </summary>
                case ItemEffect.PotionRecoverySpeedAnyPercentageIncrease:
                    PotionRecoverySpeed += v[0] * term;
                    break;
                /// <summary>
                /// ダメージ  リターン [0]％
                /// </summary>
                case ItemEffect.DamageReturnAnyPercent:
                    DamageReturned += v[0] * term;
                    break;
                /// <summary>
                /// 近接系列職業 攻撃力 [+0]～[+1]
                /// </summary>
                case ItemEffect.ProximityLineOccupationAttackPowerPlusAnyToPlusAny:
                    DamageScale += new Scale<int>(v[0] * term,  v[1] * term);
                    OnChangeStatus?.Invoke(ChangeStatusInfo.AttackPower, this);
                    break;
                /// <summary>
                /// 力 [+0]
                /// </summary>
                case ItemEffect.StrengthPlusAny:
                    Status.Strength += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.Status, this, (int?)ActorStatusType.Strength);
                    break;
                /// <summary>
                /// 知恵 [+0]
                /// </summary>
                case ItemEffect.WisdomPlusAny:
                    Status.Wisdom += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.Status, this, (int?)ActorStatusType.Wisdom);
                    break;
                /// <summary>
                /// 知識 [+0]
                /// </summary>
                case ItemEffect.InteligencePlusAny:
                    Status.Inteligence += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.Status, this, (int?)ActorStatusType.Inteligence);
                    break;
                /// <summary>
                /// 健康 [+0]
                /// </summary>
                case ItemEffect.ConditionPlusAny:
                    Status.Condition += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.Status, this, (int?)ActorStatusType.Condition);
                    break;
                /// <summary>
                /// 敏捷 [+0]
                /// </summary>
                case ItemEffect.AgilityPlusAny:
                    Status.Agility += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.Status, this, (int?)ActorStatusType.Agility);
                    break;
                /// <summary>
                /// カリスマ [+0]
                /// </summary>
                case ItemEffect.CharismaPlusAny:
                    Status.Charisma += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.Status, this, (int?)ActorStatusType.Charisma);
                    break;
                /// <summary>
                /// 幸運 [+0]
                /// </summary>
                case ItemEffect.LuckeyPlusAny:
                    Status.Luckey += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.Status, this, (int?)ActorStatusType.Luckey);
                    break;
                /// <summary>
                /// クリティカル [+0]％
                /// </summary>
                case ItemEffect.CriticalPlusAnyPercent:
                    WeaknessAttack.FatalHit += v[0] * term;
                    break;
                /// <summary>
                /// 決定打 [+0]％
                /// </summary>
                case ItemEffect.DeterminationHitPlusAnyPercent:
                    WeaknessAttack.DeterminationHit += v[0] * term;
                    break;
                /// <summary>
                /// 命中率 [+0]％
                /// </summary>
                case ItemEffect.AccuracyPlusAnyPercent:
                    AbilityRate.Accuracy += v[0] * term;
                    break;
                /// <summary>
                /// 集中力 [+0]
                /// </summary>
                case ItemEffect.ConcentrationPlusAny:
                    AbilityRate.Concentration += v[0] * term;
                    break;
                /// <summary>
                /// 攻撃速度 [+0]％
                /// </summary>
                case ItemEffect.AttackSpeedPlusAnyPercent:
                    AbilityRate.AttackSpeed += v[0] * term;
                    break;
                /// <summary>
                /// 状態 抵抗 [+0]％
                /// </summary>
                case ItemEffect.StateResistancePlusAnyPercent:
                    SpellResistance += v[0] * term;
                    break;
                /// <summary>
                /// 能力値低下状態 抵抗 [+0]％
                /// </summary>
                case ItemEffect.CapabilityValueReducedStateResistancePlusAnyPercent:
                    DeclineResistance += v[0] * term;
                    break;
                /// <summary>
                /// 異常状態 抵抗 [+0]％
                /// </summary>
                case ItemEffect.AbnormalStateResistancePlusAnyPercent:
                    SetValueInAllFields(ref AbnormalAttackResistance, v[0]);
                    break;
                /// <summary>
                /// すべての状態異常 抵抗 [+0]％
                /// </summary>
                case ItemEffect.AllStateAbnormalResistancePlusAnyPercent:
                    SpellResistance += v[0] * term;
                    DeclineResistance += v[0] * term;
                    SetValueInAllFields(ref AbnormalAttackResistance, v[0]);
                    break;
                /// <summary>
                /// 火 抵抗 [+0]％
                /// </summary>
                case ItemEffect.FireResistancePlusAnyPercent:
                    MagicResistance.Fire += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.MagicResistance, this);
                    break;
                /// <summary>
                /// 大地 抵抗 [+0]％
                /// </summary>
                case ItemEffect.GroundResistancePlusAnyPercent:
                    MagicResistance.Earth += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.MagicResistance, this);
                    break;
                /// <summary>
                /// 風 抵抗 [+0]％
                /// </summary>
                case ItemEffect.WindResistancePlusAnyPercent:
                    MagicResistance.Wind += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.MagicResistance, this);
                    break;
                /// <summary>
                /// 水 抵抗 [+0]％
                /// </summary>
                case ItemEffect.WaterResistancePlusAnyPercent:
                    MagicResistance.Water += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.MagicResistance, this);
                    break;
                /// <summary>
                /// 光 抵抗 [+0]％
                /// </summary>
                case ItemEffect.LightResistancePlusAnyPercent:
                    MagicResistance.Light += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.MagicResistance, this);
                    break;
                /// <summary>
                /// 闇 抵抗 [+0]％
                /// </summary>
                case ItemEffect.DarkResistancePlusAnyPercent:
                    MagicResistance.Dark += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.MagicResistance, this);
                    break;
                /// <summary>
                /// 火, 水, 風, 大地 抵抗 [+0]％
                /// </summary>
                case ItemEffect.Fire_Water_Wind_EarthResistancePlusAnyPercent:
                    MagicResistance.Fire += v[0] * term;
                    MagicResistance.Water += v[0] * term;
                    MagicResistance.Wind += v[0] * term;
                    MagicResistance.Earth += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.MagicResistance, this);
                    break;
                /// <summary>
                /// 魔法 抵抗 [+0]％
                /// </summary>
                case ItemEffect.MagicResistancePlusAnyPercent:
                    MagicResistance.Fire += v[0] * term;
                    MagicResistance.Water += v[0] * term;
                    MagicResistance.Wind += v[0] * term;
                    MagicResistance.Earth += v[0] * term;
                    MagicResistance.Light += v[0] * term;
                    MagicResistance.Dark += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.MagicResistance, this);
                    break;
                /// <summary>
                /// ノックバック 抵抗 [+0]％
                /// </summary>
                case ItemEffect.KnockBackResistancePlusAnyPercent:
                    WeaknessAttackResistance.KnockBack += v[0] * term;
                    break;
                /// <summary>
                /// 致命打 抵抗 [+0]％
                /// </summary>
                case ItemEffect.LifeResistanceResistancePlusAnyPercent:
                    WeaknessAttackResistance.FatalHit += v[0] * term;
                    break;
                /// <summary>
                /// 決定打 抵抗 [+0]％
                /// </summary>
                case ItemEffect.DeterminationStrikingResistancePlusAnyPercent:
                    WeaknessAttackResistance.DeterminationHit += v[0] * term;
                    break;
                /// <summary>
                /// 即死 抵抗 [+0]％
                /// </summary>
                case ItemEffect.InstantDeathResistancePlusAnyPercent:
                    WeaknessAttackResistance.InstantDeath += v[0] * term;
                    break;
                /// <summary>
                /// ランサーのスキルレベル [+0]
                /// </summary>
                case ItemEffect.LancerSkillLevelPlusAny:
                    JobSkillLevel.Lancer += v[0] * term;
                    break;
                /// <summary>
                /// アーチャーのスキルレベル [+0]
                /// </summary>
                case ItemEffect.ArchersSkillLevelPlusAny:
                    JobSkillLevel.Archer += v[0] * term;
                    break;
                /// <summary>
                /// 剣士のスキルレベル [+0]
                /// </summary>
                case ItemEffect.FencerSkillLevelPlusAny:
                    JobSkillLevel.Swordsman += v[0] * term;
                    break;
                /// <summary>
                /// 戦士のスキルレベル [+0]
                /// </summary>
                case ItemEffect.WarriorSkillLevelPlusAny:
                    JobSkillLevel.Warrior += v[0] * term;
                    break;
                /// <summary>
                /// ウィザードのスキルレベル [+0]
                /// </summary>
                case ItemEffect.WizardSkillLevelPlusAny:
                    JobSkillLevel.Wizard += v[0] * term;
                    break;
                /// <summary>
                /// ウルフマンのスキルレベル [+0]
                /// </summary>
                case ItemEffect.WolfmansSkillLevelPlusAny:
                    JobSkillLevel.Wolfman += v[0] * term;
                    break;
                /// <summary>
                /// シーフのスキルレベル [+0]
                /// </summary>
                case ItemEffect.ThiefSkillLevelPlusAny:
                    JobSkillLevel.Thief += v[0] * term;
                    break;
                /// <summary>
                /// 武道家のスキルレベル [+0]
                /// </summary>
                case ItemEffect.MartialArtsSkillLevelPlusAny:
                    JobSkillLevel.Monk += v[0] * term;
                    break;
                /// <summary>
                /// ビーストテイマーのスキルレベル [+0]
                /// </summary>
                case ItemEffect.BeastatersSkillLevelPlusAny:
                    JobSkillLevel.Tamer += v[0] * term;
                    break;
                /// <summary>
                /// サマナーのスキルレベル [+0]
                /// </summary>
                case ItemEffect.SummonerSkillLevelPlusAny:
                    JobSkillLevel.Summoner += v[0] * term;
                    break;
                /// <summary>
                /// プリンセスのスキルレベル [+0]
                /// </summary>
                case ItemEffect.PrincessSkillLevelPlusAny:
                    JobSkillLevel.Princess += v[0] * term;
                    break;
                /// <summary>
                /// リトルウィッチのスキルレベル [+0]
                /// </summary>
                case ItemEffect.LittleWitchSkillLevelPlusAny:
                    JobSkillLevel.LittleWitch += v[0] * term;
                    break;
                /// <summary>
                /// ビショップのスキルレベル [+0]
                /// </summary>
                case ItemEffect.BishopsSkillLevelPlusAny:
                    JobSkillLevel.Bishop += v[0] * term;
                    break;
                /// <summary>
                /// 追放天使のスキルレベル [+0]
                /// </summary>
                case ItemEffect.ExileAngelSkillLevelPlusAny:
                    JobSkillLevel.Angel += v[0] * term;
                    break;
                /// <summary>
                /// ネクロマンサーのスキルレベル [+0]
                /// </summary>
                case ItemEffect.NecromancerSkillLevelPlusAny:
                    JobSkillLevel.Necro += v[0] * term;
                    break;
                /// <summary>
                /// 悪魔のスキルレベル [+0]
                /// </summary>
                case ItemEffect.DevilSkillLevelPlusAny:
                    JobSkillLevel.Demon += v[0] * term;
                    break;
                /// <summary>
                /// 女性キャラクター スキルレベル [+0]
                /// </summary>
                case ItemEffect.FemaleCharacterSkillLevelPlusAny:
                    foreach (var female in ((IEnumerable<Player.JOB>)Enum.GetValues(typeof(Player.JOB))).Where(t => !IsMen(t)))
                    {
                        JobSkillLevel[female] += v[0] * term;
                    }
                    break;
                /// <summary>
                /// 男性キャラクター スキルレベル [+0]
                /// </summary>
                case ItemEffect.MaleCharacterSkillLevelPlusAny:
                    foreach (var male in ((IEnumerable<Player.JOB>)Enum.GetValues(typeof(Player.JOB))).Where(t => IsMen(t)))
                    {
                        JobSkillLevel[male] += v[0] * term;
                    }
                    break;
                /// <summary>
                /// すべてのスキルレベル [+0]
                /// </summary>
                case ItemEffect.AllSkillLevelsPlusAny:
                    foreach (Player.JOB job in Enum.GetValues(typeof(Player.JOB)))
                    {
                        JobSkillLevel[job] += v[0] * term;
                    }
                    break;
                /// <summary>
                /// 回避率 [+0]％
                /// </summary>
                case ItemEffect.AvoidanceRatePlusAnyPercent:
                    AbilityRate.Avoidance += v[0] * term;
                    break;
                /// <summary>
                /// 誘惑攻撃[0]％ 30秒
                /// </summary>
                case ItemEffect.TemptationAttackAnyPercent30Seconds:
                    AbnormalAttack.Fascination += (v[0] * term,  30 * term);
                    break;
                /// <summary>
                /// コールド攻撃100％ [0]秒
                /// </summary>
                case ItemEffect.ColdAttack100PercentAnySeconds:
                    AbnormalAttack.Cold += (100 * term,  v[0] * term);
                    break;
                /// <summary>
                /// ダメージ  リターン 40％
                /// </summary>
                case ItemEffect.DamageReturn40Percent:
                    DamageReturned += 40 * term;
                    break;
                /// <summary>
                /// すべてのスキルレベル +2- 幸運 +200 
                /// </summary>
                case ItemEffect.AllSkillLevelsPlus2AndFortunePlus200:
                    foreach (Player.JOB job in Enum.GetValues(typeof(Player.JOB)))
                    {
                        JobSkillLevel[job] += 2 * term;
                    }
                    Status.Luckey += 200 * term;
                    break;
                /// <summary>
                /// ターゲットの火の抵抗を [0]％ 弱化させる。
                /// </summary>
                case ItemEffect.WeakenTheTargetsFireResistanceByAnyPercentage:
                    WeakenMagicResistance.Fire += v[0] * term;
                    break;
                /// <summary>
                /// ターゲットの水の抵抗を [0]％ 弱化させる。
                /// </summary>
                case ItemEffect.WeakenTheTargetWaterResistanceAnyPercentage:
                    WeakenMagicResistance.Water += v[0] * term;
                    break;
                /// <summary>
                /// ターゲットの風の抵抗を [0]％ 弱化させる。
                /// </summary>
                case ItemEffect.WeakenTheTargetWindResistanceByAnyPercentage:
                    WeakenMagicResistance.Wind += v[0] * term;
                    break;
                /// <summary>
                /// ターゲットの大地の抵抗を [0]％ 弱化させる。
                /// </summary>
                case ItemEffect.WeakenTheResistanceOfTheTargetsEarthAnyAmount:
                    WeakenMagicResistance.Earth += v[0] * term;
                    break;
                /// <summary>
                /// ターゲットの光の抵抗を [0]％ 弱化させる。
                /// </summary>
                case ItemEffect.WeakenTheResistanceOfTheTargetLightByAnyPercentage:
                    WeakenMagicResistance.Light += v[0] * term;
                    break;
                /// <summary>
                /// ターゲットの闇の抵抗を [0]％ 弱化させる。
                /// </summary>
                case ItemEffect.WeakenTheTargetsDarkResistanceByAnyPercentage:
                    WeakenMagicResistance.Dark += v[0] * term;
                    break;
                /// <summary>
                /// ターゲットの魔法の抵抗を [0]％ 弱化させる。
                /// </summary>
                case ItemEffect.WeakenTheTargetsMagicResistanceAnyPercent:
                    WeakenMagicResistance.Fire += v[0] * term;
                    WeakenMagicResistance.Water += v[0] * term;
                    WeakenMagicResistance.Wind += v[0] * term;
                    WeakenMagicResistance.Earth += v[0] * term;
                    WeakenMagicResistance.Light += v[0] * term;
                    WeakenMagicResistance.Dark += v[0] * term;
                    break;
                /// <summary>
                /// 火属性の攻撃力を [0]％ 強化させる。
                /// </summary>
                case ItemEffect.ImproveFirePerformanceByAnyPercentage:
                    IncreaseMagicAttackPower.Fire += v[0] * term;
                    break;
                /// <summary>
                /// 水属性の攻撃力を [0]％ 強化させる。
                /// </summary>
                case ItemEffect.ImproveTheWaterAttackStrengthByAnyPercent:
                    IncreaseMagicAttackPower.Water += v[0] * term;
                    break;
                /// <summary>
                /// 風属性の攻撃力を [0]％ 強化させる。
                /// </summary>
                case ItemEffect.ImproveTheWindAttackStrengthByAnyPercent:
                    IncreaseMagicAttackPower.Wind += v[0] * term;
                    break;
                /// <summary>
                /// 大地属性の攻撃力を [0]％ 強化させる。
                /// </summary>
                case ItemEffect.IncreaseTheAttackPowerOfTheEarthAttributeByAnyPercentage:
                    IncreaseMagicAttackPower.Earth += v[0] * term;
                    break;
                /// <summary>
                /// 光属性の攻撃力を [0]％ 強化させる。
                /// </summary>
                case ItemEffect.EnhanceTheAttackPowerOfLightAttributeByAnyPercent:
                    IncreaseMagicAttackPower.Light += v[0] * term;
                    break;
                /// <summary>
                /// 闇属性の攻撃力を [0]％ 強化させる。
                /// </summary>
                case ItemEffect.ImproveTheAttackPowerOfTheDarkAttributeByAnyPercent:
                    IncreaseMagicAttackPower.Dark += v[0] * term;
                    break;
                /// <summary>
                /// 魔法の攻撃力を [0]％ 強化させる。
                /// </summary>
                case ItemEffect.EnhanceMagicalAttackPowerAnyPercentage:
                    IncreaseMagicAttackPower.Fire += v[0] * term;
                    IncreaseMagicAttackPower.Water += v[0] * term;
                    IncreaseMagicAttackPower.Wind += v[0] * term;
                    IncreaseMagicAttackPower.Earth += v[0] * term;
                    IncreaseMagicAttackPower.Light += v[0] * term;
                    IncreaseMagicAttackPower.Dark += v[0] * term;
                    break;
                /// <summary>
                /// HP [+1]
                /// </summary>
                case ItemEffect.HPPlusAny:
                    MaxHP += v[1] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.HP, this);
                    break;
                /// <summary>
                /// CP [+1]
                /// </summary>
                case ItemEffect.CPPlusAny:
                    MaxCP += v[1] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.CP, this);
                    break;
                /// <summary>
                /// 移動速度 [0]％増加
                /// </summary>
                case ItemEffect.MovementSpeedAnyPercentIncrease:
                    AbilityRate.MovingSpeed += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.MoveSpeed, this);
                    break;
                /// <summary>
                /// [0]％確率で物理ダメージの[1]％を 体力吸収
                /// </summary>
                case ItemEffect.AnyPercentProbabilityToAbsorbAnyPercentageOfPhysicalDamage:
                    AbsorbanceAnyAmountOfDamageGivenToEnemies.prob += v[0] * term;
                    AbsorbanceAnyAmountOfDamageGivenToEnemies.rate += v[1] * term;
                    break;
                /// <summary>
                /// 火属性 ダメージ [+0]
                /// </summary>
                case ItemEffect.FireAttributeDamagePlusAny2:
                    MagicAttack.Fire += new Scale<int>(v[0] * term,  v[0] * term);
                    break;
                /// <summary>
                /// 水属性 ダメージ  [+0]
                /// </summary>
                case ItemEffect.WaterAttributeDamagePlusAny2:
                    MagicAttack.Water += new Scale<int>(v[0] * term,  v[0] * term);
                    break;
                /// <summary>
                /// 風属性 ダメージ  [+0]
                /// </summary>
                case ItemEffect.WindAttributeDamagePlusAny2:
                    MagicAttack.Wind += new Scale<int>(v[0] * term,  v[0] * term);
                    break;
                /// <summary>
                /// 大地属性 ダメージ  [+0]
                /// </summary>
                case ItemEffect.EarthDamageDamagePlusAny2:
                    MagicAttack.Earth += new Scale<int>(v[0] * term,  v[0] * term);
                    break;
                /// <summary>
                /// 光属性 ダメージ  [+0]
                /// </summary>
                case ItemEffect.LightAttributeDamagePlusAny2:
                    MagicAttack.Light += new Scale<int>(v[0] * term,  v[0] * term);
                    break;
                /// <summary>
                /// 闇属性 ダメージ  [+0]
                /// </summary>
                case ItemEffect.DarkDamageDamagePlusAny2:
                    MagicAttack.Dark += new Scale<int>(v[0] * term,  v[0] * term);
                    break;
                /// <summary>
                /// 霊術師のスキルレベル [+0]
                /// </summary>
                case ItemEffect.SpiritistSkillLevelPlusAny:
                    JobSkillLevel.NumerologyTeacher += v[0] * term;
                    break;
                /// <summary>
                /// すべての能力値 [+0]
                /// </summary>
                case ItemEffect.AllAbilityValuesPlusAny:
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
                /// [0]％の確率で受けたダメージ [0]％減少
                /// </summary>
                case ItemEffect.DamageReceivedAtAnyPercentProbabilityReceivedAnyPercentDecrease:
                    DamageReceivedAtAnyPercentProbabilityReceivedAnyPercentDecrease += v[0] * term;
                    break;
                /// <summary>
                /// 獲得経験値 [0]％増加
                /// </summary>
                case ItemEffect.AcquiredExperienceValueAnyPercentageIncrease:
                    IncreaseExp += v[0] * term;
                    break;
                /// <summary>
                /// [0]％の確率で火属性ダメージ [0]追加
                /// </summary>
                case ItemEffect.AdditionalProbabilityOfAnyPercentFireAttributeAddedAny:
                    MagicResistance.Fire += v[0] * term;
                    break;
                /// <summary>
                /// すべてのステータス [0]増加
                /// </summary>
                case ItemEffect.AllStatusAnyAnyIncrease:
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
                /// 最大体力 [0]増加
                /// </summary>
                case ItemEffect.MaximumPhysicalStrengthAny:
                    MaxHP += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.HP, this);
                    break;
                /// <summary>
                /// 最大HP [+0]
                /// </summary>
                case ItemEffect.MaxHPPlusAny:
                    MaxHP += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.HP, this);
                    break;
                /// <summary>
                /// 最大CP [+0]
                /// </summary>
                case ItemEffect.MaxCPPlusAny:
                    MaxCP += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.CP, this);
                    break;
                /// <summary>
                /// 防御力 [+0]％
                /// </summary>
                case ItemEffect.DefensePowerPlusAnyPercent:
                    Defense += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.Defence, this);
                    break;
                /// <summary>
                /// 最大HP [+0]％
                /// </summary>
                case ItemEffect.MaxHPPlusAnyPercent:
                    MaxHP += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.HP, this);
                    break;
                /// <summary>
                /// 最大CP [+0]％
                /// </summary>
                case ItemEffect.MaxCPPlusAnyPercent:
                    MaxCP += v[0] * term;
                    OnChangeStatus?.Invoke(ChangeStatusInfo.CP, this);
                    break;
                /// <summary>
                /// RED STONE獲得可能レベル [+0]
                /// </summary>
                case ItemEffect.REDSTONEAcceptableLevelPlusAny:
                    REDSTONEAcceptableLevel += v[0] * term;
                    break;
            }
        }

        public enum ItemEffect : ushort
        {
            /// <summary>
            /// EMPTY
            /// </summary>
            [Comment("EMPTY")]
            EMPTY = ushort.MaxValue,

            /// <summary>
            /// 防御力 [+0]
            /// </summary>
            [Comment("防御力 [+0]")]
            DefensePowerPlusAny = 0,

            /// <summary>
            /// ブロック率 [+0]％
            /// </summary>
            [Comment("ブロック率 [+0]％")]
            BlockRatioPlusAnyPercent = 1,

            /// <summary>
            /// スタックアイテム [0]個
            /// </summary>
            [Comment("スタックアイテム [0]個")]
            StackItemAny = 2,

            /// <summary>
            /// [0]個の制限なしの称号の作成可能
            /// </summary>
            [Comment("[0]個の制限なしの称号の作成可能")]
            CreateAnyNumberOfUnlimitedTitles = 3,

            /// <summary>
            /// HP回復 [0]ポイント
            /// </summary>
            [Comment("HP回復 [0]ポイント")]
            HPRecoveryAnyPoint = 4,

            /// <summary>
            /// HP回復 [0]％
            /// </summary>
            [Comment("HP回復 [0]％")]
            HPRecoveryAnyPercent = 5,

            /// <summary>
            /// CP充填 [0]ポイント
            /// </summary>
            [Comment("CP充填 [0]ポイント")]
            CPFillingAnyPoint = 6,

            /// <summary>
            /// CP充填 [0]％
            /// </summary>
            [Comment("CP充填 [0]％")]
            CPFillAnyPercent = 7,

            /// <summary>
            /// HP回復 [0]ポイント- CP充填 [0]ポイント
            /// </summary>
            [Comment("HP回復 [0]ポイント- CP充填 [0]ポイント")]
            HPRecoveryAnyPointAndCPFillingAnyPoint = 8,

            /// <summary>
            /// HP回復 [0]％- CP充填 [0]％
            /// </summary>
            [Comment("HP回復 [0]％- CP充填 [0]％")]
            HPRecoveryAnyPercentAndCPFillingAnyPercent = 9,

            /// <summary>
            /// 戦闘不能のキャラクター復活- HP回復 [0]％
            /// </summary>
            [Comment("戦闘不能のキャラクター復活- HP回復 [0]％")]
            BattleImpossibleCharacterResurrectionAndHPRecoveryAnyPercent = 10,

            /// <summary>
            /// 戦闘不能状態で復活- HP回復 [0]％
            /// </summary>
            [Comment("戦闘不能状態で復活- HP回復 [0]％")]
            ResurrectionInNonCombatStateAndHPRecoveryAnyPercent = 11,

            /// <summary>
            /// 状態異常の中和 [0]％
            /// </summary>
            [Comment("状態異常の中和 [0]％")]
            NeutralizationOfStateAnomaliesAnyPercent = 12,

            /// <summary>
            /// すべての異常系の状態治療
            /// </summary>
            [Comment("すべての異常系の状態治療")]
            TreatmentOfAllAbnormalSystemConditions = 13,

            /// <summary>
            /// すべての低下系の状態治療
            /// </summary>
            [Comment("すべての低下系の状態治療")]
            ConditionTreatmentOfAllLoweringSystem = 14,

            /// <summary>
            /// すべての呪い系の状態治療 
            /// </summary>
            [Comment("すべての呪い系の状態治療 ")]
            TreatmentOfTheStatusOfAllCurses = 15,

            /// <summary>
            /// すべての状態異常の治療
            /// </summary>
            [Comment("すべての状態異常の治療")]
            TreatmentOfAllConditionAbnormalities = 16,

            /// <summary>
            /// 毒状態の治療
            /// </summary>
            [Comment("毒状態の治療")]
            TreatmentOfPoisonousCondition = 17,

            /// <summary>
            /// 力 [+0] [1]秒
            /// </summary>
            [Comment("力 [+0] [1]秒")]
            StrengthPlusAnyAnySeconds = 18,

            /// <summary>
            /// 敏捷 [+0] [1]秒
            /// </summary>
            [Comment("敏捷 [+0] [1]秒")]
            AgilityPlusAnyAnySeconds = 19,

            /// <summary>
            /// 健康 [+0] [1]秒
            /// </summary>
            [Comment("健康 [+0] [1]秒")]
            ConditionPlusAnyAnySeconds = 20,

            /// <summary>
            /// 知恵 [+0] [1]秒
            /// </summary>
            [Comment("知恵 [+0] [1]秒")]
            WisdomPlusAnyAnySeconds = 21,

            /// <summary>
            /// 知識 [+0] [1]秒
            /// </summary>
            [Comment("知識 [+0] [1]秒")]
            KnowledgePlusAnyAnySeconds = 22,

            /// <summary>
            /// カリスマ [+0] [1]秒
            /// </summary>
            [Comment("カリスマ [+0] [1]秒")]
            CharismaPlusAnyAnySeconds = 23,

            /// <summary>
            /// 幸運 [+0] [1]秒
            /// </summary>
            [Comment("幸運 [+0] [1]秒")]
            LuckeyPlusAnyAnySeconds = 24,

            /// <summary>
            /// 攻撃力 [+1]％ [0]秒
            /// </summary>
            [Comment("攻撃力 [+1]％ [0]秒")]
            AttackPowerPlusAnyPercentAnySeconds = 25,

            /// <summary>
            /// 防御力 [+1]％ [0]秒
            /// </summary>
            [Comment("防御力 [+1]％ [0]秒")]
            DefensePowerPlusAnyPercentAnySeconds = 26,

            /// <summary>
            /// 最大HP [+1]％ [0]秒
            /// </summary>
            [Comment("最大HP [+1]％ [0]秒")]
            MaxHPPlusAnyPercentAnySeconds = 27,

            /// <summary>
            /// 最大CP [+1]％ [0]秒
            /// </summary>
            [Comment("最大CP [+1]％ [0]秒")]
            MaxCPPlusAnyPercentAnySeconds = 28,

            /// <summary>
            /// 武器ダメージを最高に維持 [0]秒
            /// </summary>
            [Comment("武器ダメージを最高に維持 [0]秒")]
            MaintainWeaponDamageAtMaximumMax = 29,

            /// <summary>
            /// CPを最高に維持 [0]秒
            /// </summary>
            [Comment("CPを最高に維持 [0]秒")]
            MaintainCPAtMaximumAnySeconds = 30,

            /// <summary>
            /// 宝箱の鍵 レベル [0]
            /// </summary>
            [Comment("宝箱の鍵 レベル [0]")]
            TreasureBoxKeyLevelAny = 31,

            /// <summary>
            /// 門の鍵 レベル [0]
            /// </summary>
            [Comment("門の鍵 レベル [0]")]
            GateKeyLevelAny = 32,

            /// <summary>
            /// 万能鍵 レベル [0]
            /// </summary>
            [Comment("万能鍵 レベル [0]")]
            UniversalKeyLevelAny = 33,

            /// <summary>
            /// 街へ帰還する
            /// </summary>
            [Comment("街へ帰還する")]
            ReturnToTheCity = 34,

            /// <summary>
            /// 街往復ポータル
            /// </summary>
            [Comment("街往復ポータル")]
            RoundtripStreetPortal = 35,

            /// <summary>
            /// 使用可能回数[1]回(維持時間 [0]分)
            /// </summary>
            [Comment("使用可能回数[1]回(維持時間 [0]分)")]
            AvailableNumberOfTimesAny_maintenanceTimeAny_ = 36,

            /// <summary>
            /// ターゲット即死確率 [+0]％
            /// </summary>
            [Comment("ターゲット即死確率 [+0]％")]
            TargetInstantDeathProbabilityPlusAnyPercent = 37,

            /// <summary>
            /// ベルセルク攻撃[0]％ [1]秒
            /// </summary>
            [Comment("ベルセルク攻撃[0]％ [1]秒")]
            BerserkAttackAnyPercentAnySeconds = 38,

            /// <summary>
            /// 鎧破壊攻撃[0]％ [1]秒
            /// </summary>
            [Comment("鎧破壊攻撃[0]％ [1]秒")]
            ArmorDestroyAttackAnyPercentAnySeconds = 39,

            /// <summary>
            /// 武器破壊攻撃[0]％ [1]秒
            /// </summary>
            [Comment("武器破壊攻撃[0]％ [1]秒")]
            WeaponDestructionAttackAnyPercentAnySeconds = 40,

            /// <summary>
            /// 毒攻撃- 秒当りダメージ [1]([0]秒)
            /// </summary>
            [Comment("毒攻撃- 秒当りダメージ [1]([0]秒)")]
            PoisonAttackAndDamagePerSecondAny_anySeconds_ = 41,

            /// <summary>
            /// 混乱攻撃[0]％ [1]秒
            /// </summary>
            [Comment("混乱攻撃[0]％ [1]秒")]
            ConfusedAttackAnyPercentAnySeconds = 42,

            /// <summary>
            /// 誘惑攻撃[0]％ [1]秒
            /// </summary>
            [Comment("誘惑攻撃[0]％ [1]秒")]
            TemptationAttackAnyAnyAnySeconds = 43,

            /// <summary>
            /// スタン攻撃[0]％ [1]Frame
            /// </summary>
            [Comment("スタン攻撃[0]％ [1]Frame")]
            StunAttackAnyPercentAnyFrame = 44,

            /// <summary>
            /// コールド攻撃[0]％ [1]秒
            /// </summary>
            [Comment("コールド攻撃[0]％ [1]秒")]
            ColdAttackAnyAnyAnySeconds = 45,

            /// <summary>
            /// 火属性 ダメージ [+0]
            /// </summary>
            [Comment("火属性 ダメージ [+0]")]
            FireAttributeDamagePlusAny = 46,

            /// <summary>
            /// 水属性 ダメージ  [+0]
            /// </summary>
            [Comment("水属性 ダメージ  [+0]")]
            WaterAttributeDamagePlusAny = 47,

            /// <summary>
            /// 風属性 ダメージ  [+0]
            /// </summary>
            [Comment("風属性 ダメージ  [+0]")]
            WindAttributeDamagePlusAny = 48,

            /// <summary>
            /// 大地属性 ダメージ  [+0]
            /// </summary>
            [Comment("大地属性 ダメージ  [+0]")]
            EarthAttributeDamagePlusAny = 49,

            /// <summary>
            /// 光属性 ダメージ  [+0]
            /// </summary>
            [Comment("光属性 ダメージ  [+0]")]
            LightAttributeDamagePlusAny = 50,

            /// <summary>
            /// 闇属性 ダメージ  [+0]
            /// </summary>
            [Comment("闇属性 ダメージ  [+0]")]
            DarkAttributeDamagePlusAny = 51,

            /// <summary>
            /// vs アンデッド系キャラクター - ダメージ  [+0]％
            /// </summary>
            [Comment("vs アンデッド系キャラクター - ダメージ  [+0]％")]
            VsUndeadCharacterAndDamagePlusAnyPercent = 52,

            /// <summary>
            /// vs 悪魔系キャラクター - ダメージ  [+0]％
            /// </summary>
            [Comment("vs 悪魔系キャラクター - ダメージ  [+0]％")]
            VsDevilSeriesCharacterAndDamagePlusAnyPercent = 53,

            /// <summary>
            /// vs 人間系キャラクター - ダメージ  [+0]％
            /// </summary>
            [Comment("vs 人間系キャラクター - ダメージ  [+0]％")]
            VsHumanCharacterAndDamagePlusAnyPercent = 54,

            /// <summary>
            /// vs 動物系キャラクター - ダメージ  [+0]％
            /// </summary>
            [Comment("vs 動物系キャラクター - ダメージ  [+0]％")]
            VsAnimalCharacterAndDamagePlusAnyPercent = 55,

            /// <summary>
            /// vs 神獣系キャラクター - ダメージ  [+0]％
            /// </summary>
            [Comment("vs 神獣系キャラクター - ダメージ  [+0]％")]
            VsGodBeastCharacterAndDamagePlusAnyPercent = 56,

            /// <summary>
            /// アイテム リロードタイム -[0]％
            /// </summary>
            [Comment("アイテム リロードタイム -[0]％")]
            ItemReloadTimeMinusAnyPercent = 57,

            /// <summary>
            /// 変身速度 [0]％ 増加
            /// </summary>
            [Comment("変身速度 [0]％ 増加")]
            TransformationSpeedAnyPercentageIncrease = 58,

            /// <summary>
            /// ポーション  回復速度 [0]％ 増加
            /// </summary>
            [Comment("ポーション  回復速度 [0]％ 増加")]
            PotionRecoverySpeedAnyPercentageIncrease = 59,

            /// <summary>
            /// ダメージ  リターン [0]％
            /// </summary>
            [Comment("ダメージ  リターン [0]％")]
            DamageReturnAnyPercent = 60,

            /// <summary>
            /// 無限弾丸
            /// </summary>
            [Comment("無限弾丸")]
            InfiniteBullet = 61,

            /// <summary>
            /// 近接系列職業 攻撃力 [+0]～[+1]
            /// </summary>
            [Comment("近接系列職業 攻撃力 [+0]～[+1]")]
            ProximityLineOccupationAttackPowerPlusAnyToPlusAny = 62,

            /// <summary>
            /// 力 [+0]
            /// </summary>
            [Comment("力 [+0]")]
            StrengthPlusAny = 63,

            /// <summary>
            /// 知恵 [+0]
            /// </summary>
            [Comment("知恵 [+0]")]
            WisdomPlusAny = 64,

            /// <summary>
            /// 知識 [+0]
            /// </summary>
            [Comment("知識 [+0]")]
            InteligencePlusAny = 65,

            /// <summary>
            /// 健康 [+0]
            /// </summary>
            [Comment("健康 [+0]")]
            ConditionPlusAny = 66,

            /// <summary>
            /// 敏捷 [+0]
            /// </summary>
            [Comment("敏捷 [+0]")]
            AgilityPlusAny = 67,

            /// <summary>
            /// カリスマ [+0]
            /// </summary>
            [Comment("カリスマ [+0]")]
            CharismaPlusAny = 68,

            /// <summary>
            /// 幸運 [+0]
            /// </summary>
            [Comment("幸運 [+0]")]
            LuckeyPlusAny = 69,

            /// <summary>
            /// クリティカル [+0]％
            /// </summary>
            [Comment("クリティカル [+0]％")]
            CriticalPlusAnyPercent = 70,

            /// <summary>
            /// 決定打 [+0]％
            /// </summary>
            [Comment("決定打 [+0]％")]
            DeterminationHitPlusAnyPercent = 71,

            /// <summary>
            /// 命中率 [+0]％
            /// </summary>
            [Comment("命中率 [+0]％")]
            AccuracyPlusAnyPercent = 72,

            /// <summary>
            /// 集中力 [+0]
            /// </summary>
            [Comment("集中力 [+0]")]
            ConcentrationPlusAny = 73,

            /// <summary>
            /// 攻撃速度 [+0]％
            /// </summary>
            [Comment("攻撃速度 [+0]％")]
            AttackSpeedPlusAnyPercent = 74,

            /// <summary>
            /// 状態 抵抗 [+0]％
            /// </summary>
            [Comment("状態 抵抗 [+0]％")]
            StateResistancePlusAnyPercent = 75,

            /// <summary>
            /// 能力値低下状態 抵抗 [+0]％
            /// </summary>
            [Comment("能力値低下状態 抵抗 [+0]％")]
            CapabilityValueReducedStateResistancePlusAnyPercent = 76,

            /// <summary>
            /// 異常状態 抵抗 [+0]％
            /// </summary>
            [Comment("異常状態 抵抗 [+0]％")]
            AbnormalStateResistancePlusAnyPercent = 77,

            /// <summary>
            /// すべての状態異常 抵抗 [+0]％
            /// </summary>
            [Comment("すべての状態異常 抵抗 [+0]％")]
            AllStateAbnormalResistancePlusAnyPercent = 78,

            /// <summary>
            /// 火 抵抗 [+0]％
            /// </summary>
            [Comment("火 抵抗 [+0]％")]
            FireResistancePlusAnyPercent = 79,

            /// <summary>
            /// 大地 抵抗 [+0]％
            /// </summary>
            [Comment("大地 抵抗 [+0]％")]
            GroundResistancePlusAnyPercent = 80,

            /// <summary>
            /// 風 抵抗 [+0]％
            /// </summary>
            [Comment("風 抵抗 [+0]％")]
            WindResistancePlusAnyPercent = 81,

            /// <summary>
            /// 水 抵抗 [+0]％
            /// </summary>
            [Comment("水 抵抗 [+0]％")]
            WaterResistancePlusAnyPercent = 82,

            /// <summary>
            /// 光 抵抗 [+0]％
            /// </summary>
            [Comment("光 抵抗 [+0]％")]
            LightResistancePlusAnyPercent = 83,

            /// <summary>
            /// 闇 抵抗 [+0]％
            /// </summary>
            [Comment("闇 抵抗 [+0]％")]
            DarkResistancePlusAnyPercent = 84,

            /// <summary>
            /// 火, 水, 風, 大地 抵抗 [+0]％
            /// </summary>
            [Comment("火, 水, 風, 大地 抵抗 [+0]％")]
            Fire_Water_Wind_EarthResistancePlusAnyPercent = 85,

            /// <summary>
            /// 魔法 抵抗 [+0]％
            /// </summary>
            [Comment("魔法 抵抗 [+0]％")]
            MagicResistancePlusAnyPercent = 86,

            /// <summary>
            /// ノックバック 抵抗 [+0]％
            /// </summary>
            [Comment("ノックバック 抵抗 [+0]％")]
            KnockBackResistancePlusAnyPercent = 87,

            /// <summary>
            /// 致命打 抵抗 [+0]％
            /// </summary>
            [Comment("致命打 抵抗 [+0]％")]
            LifeResistanceResistancePlusAnyPercent = 88,

            /// <summary>
            /// 決定打 抵抗 [+0]％
            /// </summary>
            [Comment("決定打 抵抗 [+0]％")]
            DeterminationStrikingResistancePlusAnyPercent = 89,

            /// <summary>
            /// 即死 抵抗 [+0]％
            /// </summary>
            [Comment("即死 抵抗 [+0]％")]
            InstantDeathResistancePlusAnyPercent = 90,

            /// <summary>
            /// ランサーのスキルレベル [+0]
            /// </summary>
            [Comment("ランサーのスキルレベル [+0]")]
            LancerSkillLevelPlusAny = 91,

            /// <summary>
            /// アーチャーのスキルレベル [+0]
            /// </summary>
            [Comment("アーチャーのスキルレベル [+0]")]
            ArchersSkillLevelPlusAny = 92,

            /// <summary>
            /// 剣士のスキルレベル [+0]
            /// </summary>
            [Comment("剣士のスキルレベル [+0]")]
            FencerSkillLevelPlusAny = 93,

            /// <summary>
            /// 戦士のスキルレベル [+0]
            /// </summary>
            [Comment("戦士のスキルレベル [+0]")]
            WarriorSkillLevelPlusAny = 94,

            /// <summary>
            /// ウィザードのスキルレベル [+0]
            /// </summary>
            [Comment("ウィザードのスキルレベル [+0]")]
            WizardSkillLevelPlusAny = 95,

            /// <summary>
            /// ウルフマンのスキルレベル [+0]
            /// </summary>
            [Comment("ウルフマンのスキルレベル [+0]")]
            WolfmansSkillLevelPlusAny = 96,

            /// <summary>
            /// シーフのスキルレベル [+0]
            /// </summary>
            [Comment("シーフのスキルレベル [+0]")]
            ThiefSkillLevelPlusAny = 97,

            /// <summary>
            /// 武道家のスキルレベル [+0]
            /// </summary>
            [Comment("武道家のスキルレベル [+0]")]
            MartialArtsSkillLevelPlusAny = 98,

            /// <summary>
            /// ビーストテイマーのスキルレベル [+0]
            /// </summary>
            [Comment("ビーストテイマーのスキルレベル [+0]")]
            BeastatersSkillLevelPlusAny = 99,

            /// <summary>
            /// サマナーのスキルレベル [+0]
            /// </summary>
            [Comment("サマナーのスキルレベル [+0]")]
            SummonerSkillLevelPlusAny = 100,

            /// <summary>
            /// プリンセスのスキルレベル [+0]
            /// </summary>
            [Comment("プリンセスのスキルレベル [+0]")]
            PrincessSkillLevelPlusAny = 101,

            /// <summary>
            /// リトルウィッチのスキルレベル [+0]
            /// </summary>
            [Comment("リトルウィッチのスキルレベル [+0]")]
            LittleWitchSkillLevelPlusAny = 102,

            /// <summary>
            /// ビショップのスキルレベル [+0]
            /// </summary>
            [Comment("ビショップのスキルレベル [+0]")]
            BishopsSkillLevelPlusAny = 103,

            /// <summary>
            /// 追放天使のスキルレベル [+0]
            /// </summary>
            [Comment("追放天使のスキルレベル [+0]")]
            ExileAngelSkillLevelPlusAny = 104,

            /// <summary>
            /// ネクロマンサーのスキルレベル [+0]
            /// </summary>
            [Comment("ネクロマンサーのスキルレベル [+0]")]
            NecromancerSkillLevelPlusAny = 105,

            /// <summary>
            /// 悪魔のスキルレベル [+0]
            /// </summary>
            [Comment("悪魔のスキルレベル [+0]")]
            DevilSkillLevelPlusAny = 106,

            /// <summary>
            /// 女性キャラクター スキルレベル [+0]
            /// </summary>
            [Comment("女性キャラクター スキルレベル [+0]")]
            FemaleCharacterSkillLevelPlusAny = 107,

            /// <summary>
            /// 男性キャラクター スキルレベル [+0]
            /// </summary>
            [Comment("男性キャラクター スキルレベル [+0]")]
            MaleCharacterSkillLevelPlusAny = 108,

            /// <summary>
            /// すべてのスキルレベル [+0]
            /// </summary>
            [Comment("すべてのスキルレベル [+0]")]
            AllSkillLevelsPlusAny = 109,

            /// <summary>
            /// 回避率 [+0]％
            /// </summary>
            [Comment("回避率 [+0]％")]
            AvoidanceRatePlusAnyPercent = 110,

            /// <summary>
            /// 誘惑攻撃[0]％ 30秒
            /// </summary>
            [Comment("誘惑攻撃[0]％ 30秒")]
            TemptationAttackAnyPercent30Seconds = 111,

            /// <summary>
            /// コールド攻撃100％ [0]秒
            /// </summary>
            [Comment("コールド攻撃100％ [0]秒")]
            ColdAttack100PercentAnySeconds = 112,

            /// <summary>
            /// 力 [+0] (成功確率 : [1]％)
            /// </summary>
            [Comment("力 [+0] (成功確率 : [1]％)")]
            PowerPlusAny_SuccessProbability_AnyPercent_ = 113,

            /// <summary>
            /// 敏捷 [+0] (成功確率 : [1]％)
            /// </summary>
            [Comment("敏捷 [+0] (成功確率 : [1]％)")]
            AgilityPlusAny_successProbability_AnyPercent_ = 114,

            /// <summary>
            /// 健康 [+0] (成功確率 : [1]％)
            /// </summary>
            [Comment("健康 [+0] (成功確率 : [1]％)")]
            HealthPlusAny_SuccessProbability_AnyPercent_ = 115,

            /// <summary>
            /// 知恵 [+0] (成功確率 : [1]％)
            /// </summary>
            [Comment("知恵 [+0] (成功確率 : [1]％)")]
            WisdomPlusAny_successProbability_AnyPercent_ = 116,

            /// <summary>
            /// 知識 [+0] (成功確率 : [1]％)
            /// </summary>
            [Comment("知識 [+0] (成功確率 : [1]％)")]
            KnowledgePlusAny_probabilityOfSuccess_AnyPercent_ = 117,

            /// <summary>
            /// カリスマ [+0] (成功確率 : [1]％)
            /// </summary>
            [Comment("カリスマ [+0] (成功確率 : [1]％)")]
            CharismaPlusAny_probabilityOfSuccess_AnyPercent_ = 118,

            /// <summary>
            /// 幸運 [+0] (成功確率 : [1]％)
            /// </summary>
            [Comment("幸運 [+0] (成功確率 : [1]％)")]
            GoodLuckPlusAny_SuccessProbability_AnyPercent_ = 119,

            /// <summary>
            /// 最大 HP [+0] (失敗確率 : [0]*[1]％)
            /// </summary>
            [Comment("最大 HP [+0] (失敗確率 : [0]*[1]％)")]
            MaxHPPlusAny_failureProbability_AnyTimesAnyPercent_ = 120,

            /// <summary>
            /// 最大 CP [+0] (失敗確率 : [0]*[1]％)
            /// </summary>
            [Comment("最大 CP [+0] (失敗確率 : [0]*[1]％)")]
            MaxCPPlusAny_failureProbability_AnyTimesAnyPercent_ = 121,

            /// <summary>
            /// 武器 ダメージ  [+0]％ (失敗確率 : [0]+0.65％)
            /// </summary>
            [Comment("武器 ダメージ  [+0]％ (失敗確率 : [0]+0.65％)")]
            WeaponDamagePlusAnyPercent_failureProbability_AnyPlus065Percent_ = 122,

            /// <summary>
            /// 鎧 防御力 [+0] (失敗確率 : [0]+0.65％)
            /// </summary>
            [Comment("鎧 防御力 [+0] (失敗確率 : [0]+0.65％)")]
            ArmorDefensePowerPlusAny_failureProbability_AnyPlus065Percent_ = 123,

            /// <summary>
            /// 称号 [0]個  生成 (成功確率 : [1]％)
            /// </summary>
            [Comment("称号 [0]個  生成 (成功確率 : [1]％)")]
            TitleCreationOfAnyNumber_successProbability_AnyPercent_ = 124,

            /// <summary>
            /// ダメージ  リターン 40％
            /// </summary>
            [Comment("ダメージ  リターン 40％")]
            DamageReturn40Percent = 125,

            /// <summary>
            /// 使用者に [0]レベル フロンティア　タイトルを付与します。
            /// </summary>
            [Comment("使用者に [0]レベル フロンティア　タイトルを付与します。")]
            GrantTheUserAnyLevelFrontierTitle = 126,

            /// <summary>
            /// ドロップレベル [0]のレア　アイテムをランダムに生成する。成功確率 [1]％
            /// </summary>
            [Comment("ドロップレベル [0]のレア　アイテムをランダムに生成する。成功確率 [1]％")]
            RareItemsOfDropLevelAnyAreRandomlyGeneratedSuccessProbabilityAnyPercent = 127,

            /// <summary>
            /// ドロップレベル [0]のユニーク　アイテムをランダムに生成する。成功確率 [1]％
            /// </summary>
            [Comment("ドロップレベル [0]のユニーク　アイテムをランダムに生成する。成功確率 [1]％")]
            RandomlyGenerateUniqueItemsOfDropLevelAnySuccessProbabilityAnyPercent = 128,

            /// <summary>
            /// ドロップレベル [0]のスーパー　ユニークをランダムに生成する。成功確率 [1]％
            /// </summary>
            [Comment("ドロップレベル [0]のスーパー　ユニークをランダムに生成する。成功確率 [1]％")]
            RandomlyGenerateASuperUniqueWithADropLevelOfAnySuccessProbabilityAnyPercent = 129,

            /// <summary>
            /// スキルポイント 再配分
            /// </summary>
            [Comment("スキルポイント 再配分")]
            SkillPointRedistribution = 130,

            /// <summary>
            /// ステータスポイント 再配分
            /// </summary>
            [Comment("ステータスポイント 再配分")]
            RedistributionOfStatusPoints = 131,

            /// <summary>
            /// ダメージ　オプション変換
            /// </summary>
            [Comment("ダメージ　オプション変換")]
            DamageOptionConversion = 132,

            /// <summary>
            /// 危険なダメージ  オプション変換
            /// </summary>
            [Comment("危険なダメージ  オプション変換")]
            DangerousDamageOptionConversion = 133,

            /// <summary>
            /// 経験値 1.5倍- アイテム ドロップ率 1.5倍- 街　無制限の帰還可能- プレミアムゾーン進入- タウン  テレポーター無料使用- 魔法のカーペット無限召喚可能
            /// </summary>
            [Comment("経験値 1.5倍- アイテム ドロップ率 1.5倍- 街　無制限の帰還可能- プレミアムゾーン進入- タウン  テレポーター無料使用- 魔法のカーペット無限召喚可能")]
            ExperienceValue15TimesAndItemDropRate15TimesAndTownUnlimitedReturnableAndPremiumZoneEntryAndTownTeleporterFreeUseAndMagicCarpetInfiniteSummonable = 134,

            /// <summary>
            /// [0]分の間、攻撃力を [1]％ 増加させる。
            /// </summary>
            [Comment("[0]分の間、攻撃力を [1]％ 増加させる。")]
            IncreaseTheAttackPowerAnyPercentDuringAnyMinutes = 135,

            /// <summary>
            /// [0]分の間、防御力を [1]％ 増加させる。
            /// </summary>
            [Comment("[0]分の間、防御力を [1]％ 増加させる。")]
            IncreaseYourDefensePowerAnyAmountDuringAnyMinutes = 136,

            /// <summary>
            /// [0]分の間、HPを [1]％ 増加させる。
            /// </summary>
            [Comment("[0]分の間、HPを [1]％ 増加させる。")]
            IncreaseHPAnyPercentForAnyMinutes = 137,

            /// <summary>
            /// [0]分の間、CPを [1]％ 増加させる。
            /// </summary>
            [Comment("[0]分の間、CPを [1]％ 増加させる。")]
            IncreaseCPAnyPercentForAnyMinutes = 138,

            /// <summary>
            /// [0]分の間、武器ダメージが最高に維持される。
            /// </summary>
            [Comment("[0]分の間、武器ダメージが最高に維持される。")]
            WeaponDamageIsKeptAtItsMaximumDuringAnyMinutes = 139,

            /// <summary>
            /// [0]分の間、CPがいつも最高に維持される。
            /// </summary>
            [Comment("[0]分の間、CPがいつも最高に維持される。")]
            CPIsAlwaysKeptHighestForAnyMinutes = 140,

            /// <summary>
            /// 力を [0]ほど [1]分の間、上昇させる。
            /// </summary>
            [Comment("力を [0]ほど [1]分の間、上昇させる。")]
            IncreaseForceAsMuchAsAnyForAny = 141,

            /// <summary>
            /// 敏捷を [0]ほど [1]分の間、上昇させる。
            /// </summary>
            [Comment("敏捷を [0]ほど [1]分の間、上昇させる。")]
            IncreaseAgilityAsMuchAsAnyForAny = 142,

            /// <summary>
            /// 健康を [0]ほど [1]分の間、上昇させる。
            /// </summary>
            [Comment("健康を [0]ほど [1]分の間、上昇させる。")]
            IncreaseHealthAsMuchAsAnyForAny = 143,

            /// <summary>
            /// 知恵を [0]ほど [1]分の間、上昇させる。
            /// </summary>
            [Comment("知恵を [0]ほど [1]分の間、上昇させる。")]
            RaiseWisdomForAnyAmountAsMuchAsAny = 144,

            /// <summary>
            /// 知識を [0]ほど [1]分の間、上昇させる。
            /// </summary>
            [Comment("知識を [0]ほど [1]分の間、上昇させる。")]
            IncreaseKnowledgeAsMuchAsAnyForAny = 145,

            /// <summary>
            /// カリスマを [0]ほど [1]分の間、上昇させる。
            /// </summary>
            [Comment("カリスマを [0]ほど [1]分の間、上昇させる。")]
            IncreaseTheCharismaAsMuchAsAnyForAny = 146,

            /// <summary>
            /// 運を [0]ほど [1]分の間、上昇させる。
            /// </summary>
            [Comment("運を [0]ほど [1]分の間、上昇させる。")]
            IncreaseLuckForAnyAmountAsMuchAsAny = 147,

            /// <summary>
            /// 自分のレベルより [0]レベル高い制限レベルのアイテム使用可能
            /// </summary>
            [Comment("自分のレベルより [0]レベル高い制限レベルのアイテム使用可能")]
            YouCanUseItemsOfAnyLevelHigherThanYourOwnLevel = 148,

            /// <summary>
            /// 現在の位置を場所スロット[0]に覚えてクリスタルを[1]個生成する。
            /// </summary>
            [Comment("現在の位置を場所スロット[0]に覚えてクリスタルを[1]個生成する。")]
            RememberTheCurrentPositionInPlaceSlotAny_AndGenerateAnyNumberOfCrystals = 149,

            /// <summary>
            /// 場所スロット [0] で覚えている場所にテレポートする。
            /// </summary>
            [Comment("場所スロット [0] で覚えている場所にテレポートする。")]
            TeleportToTheLocationYouRememberInPlaceSlotAny = 150,

            /// <summary>
            /// 死亡ペナルティー時間を90％減少して、[0]秒の間、最大HPを[1]％にする。
            /// </summary>
            [Comment("死亡ペナルティー時間を90％減少して、[0]秒の間、最大HPを[1]％にする。")]
            ReduceTheDeathPenaltyTimeBy90PercentAndSetAnyMaximumPercentForAnySecondsForAnySeconds = 151,

            /// <summary>
            /// 魔法のカーペットを召喚する。
            /// </summary>
            [Comment("魔法のカーペットを召喚する。")]
            SummonAMagicalCarpet = 152,

            /// <summary>
            /// [0]分の間、攻撃力を [1]0％ 増加させる。
            /// </summary>
            [Comment("[0]分の間、攻撃力を [1]0％ 増加させる。")]
            IncreaseTheAttackPowerAny0PercentForAnyMinutes = 153,

            /// <summary>
            /// [0]分の間、防御力 [1]0％ 増加させる。
            /// </summary>
            [Comment("[0]分の間、防御力 [1]0％ 増加させる。")]
            IncreaseYourDefenseAnyAnyPercentageForAnyMinutes = 154,

            /// <summary>
            /// レベル [0]が上昇する。
            /// </summary>
            [Comment("レベル [0]が上昇する。")]
            LevelAnyRises = 155,

            /// <summary>
            /// すべてのスキルレベル +2- 幸運 +200 
            /// </summary>
            [Comment("すべてのスキルレベル +2- 幸運 +200 ")]
            AllSkillLevelsPlus2AndFortunePlus200 = 156,

            /// <summary>
            /// 持っていると魔法のカーペットの模様が変わる。
            /// </summary>
            [Comment("持っていると魔法のカーペットの模様が変わる。")]
            TheMagicCarpetPatternWillChangeWhenYouHaveIt = 157,

            /// <summary>
            /// ジム・モリのアイテム エンチャント
            /// </summary>
            [Comment("ジム・モリのアイテム エンチャント")]
            JimMorisItemEnchantment = 158,

            /// <summary>
            /// 壊れたアイテムを不器用に修理する。
            /// </summary>
            [Comment("壊れたアイテムを不器用に修理する。")]
            RepairBrokenItemsClumsy = 159,

            /// <summary>
            /// アイテムエンチャント
            /// </summary>
            [Comment("アイテムエンチャント")]
            ItemEnchantment = 160,

            /// <summary>
            /// 壊れたアイテムを完璧に修理する。
            /// </summary>
            [Comment("壊れたアイテムを完璧に修理する。")]
            RepairBrokenItemsPerfectly = 161,

            /// <summary>
            /// アイテムにかかっている呪いを解いてくれる。
            /// </summary>
            [Comment("アイテムにかかっている呪いを解いてくれる。")]
            ItWillSolveTheCurseThatIsHangingOnTheItem = 162,

            /// <summary>
            /// アイテム改良制限を解いてくれる。
            /// </summary>
            [Comment("アイテム改良制限を解いてくれる。")]
            ItSolvesTheItemImprovementRestriction = 163,

            /// <summary>
            /// ○× クイズチケット
            /// </summary>
            [Comment("○× クイズチケット")]
            OXQuizTicket = 164,

            /// <summary>
            /// 座っている時、回復速度が [0]％ 増加する。
            /// </summary>
            [Comment("座っている時、回復速度が [0]％ 増加する。")]
            WhenSitting_TheRecoveryRateIncreasesAnyPercent = 165,

            /// <summary>
            /// ギルドを作成してくれる。
            /// </summary>
            [Comment("ギルドを作成してくれる。")]
            ItWillCreateAGuild = 166,

            /// <summary>
            /// ギルドのレベルを [0]にしてくれる。
            /// </summary>
            [Comment("ギルドのレベルを [0]にしてくれる。")]
            ItMakesTheLevelOfTheGuildAny = 167,

            /// <summary>
            /// ギルドマークを選ぶことができる選択の幅を増やしてくれる。
            /// </summary>
            [Comment("ギルドマークを選ぶことができる選択の幅を増やしてくれる。")]
            ItIncreasesTheRangeOfChoicesThatYouCanChooseGuildMarks = 168,

            /// <summary>
            /// ギルドマークを選ぶことができる選択の幅を増やしてくれる。
            /// </summary>
            [Comment("ギルドマークを選ぶことができる選択の幅を増やしてくれる。")]
            ItIncreasesTheRangeOfChoicesThatYouCanChooseGuildMarks2 = 169,

            /// <summary>
            /// ギルドマークを選ぶことができる選択の幅を増やしてくれる。
            /// </summary>
            [Comment("ギルドマークを選ぶことができる選択の幅を増やしてくれる。")]
            ItIncreasesTheRangeOfChoicesThatYouCanChooseGuildMarks3 = 170,

            /// <summary>
            /// ギルドマークを選ぶことができる選択の幅を増やしてくれる。
            /// </summary>
            [Comment("ギルドマークを選ぶことができる選択の幅を増やしてくれる。")]
            ItIncreasesTheRangeOfChoicesThatYouCanChooseGuildMarks4 = 171,

            /// <summary>
            /// ギルドマークを選ぶことができる選択の幅を増やしてくれる。
            /// </summary>
            [Comment("ギルドマークを選ぶことができる選択の幅を増やしてくれる。")]
            ItIncreasesTheRangeOfChoicesThatYouCanChooseGuildMarks5 = 172,

            /// <summary>
            /// ギルドマークを選ぶことができる選択の幅を増やしてくれる。
            /// </summary>
            [Comment("ギルドマークを選ぶことができる選択の幅を増やしてくれる。")]
            ItIncreasesTheRangeOfChoicesThatYouCanChooseGuildMarks6 = 173,

            /// <summary>
            /// ギルドマークを選ぶことができる選択の幅を増やしてくれる。
            /// </summary>
            [Comment("ギルドマークを選ぶことができる選択の幅を増やしてくれる。")]
            ItIncreasesTheRangeOfChoicesThatYouCanChooseGuildMarks7 = 174,

            /// <summary>
            /// ギルドマークを選ぶことができる選択の幅を増やしてくれる。
            /// </summary>
            [Comment("ギルドマークを選ぶことができる選択の幅を増やしてくれる。")]
            ItIncreasesTheRangeOfChoicesThatYouCanChooseGuildMarks8 = 175,

            /// <summary>
            /// 頼もしい立て札
            /// </summary>
            [Comment("頼もしい立て札")]
            ConfidentLetterpress = 176,

            /// <summary>
            /// 露店商の立て札の内容の色を選択できるようにしてくれる。
            /// </summary>
            [Comment("露店商の立て札の内容の色を選択できるようにしてくれる。")]
            ItMakesItPossibleToSelectTheColorOfTheContentOfTheStandingVendorStandingTenders = 177,

            /// <summary>
            /// 露店商の立て札の内容を太い字で書くことができるようにしてくれる。
            /// </summary>
            [Comment("露店商の立て札の内容を太い字で書くことができるようにしてくれる。")]
            ItMakesItPossibleToWriteTheContentOfTheStandingVendorsStandUpTenderInBoldLetters = 178,

            /// <summary>
            /// 露店商の名前をより長く 書くことができるようにしてくれる。
            /// </summary>
            [Comment("露店商の名前をより長く 書くことができるようにしてくれる。")]
            ItMakesItPossibleToWriteTheNameOfTheStallVendorLonger = 179,

            /// <summary>
            /// 露店商の立て札の周りにきらめく効果をくれる。
            /// </summary>
            [Comment("露店商の立て札の周りにきらめく効果をくれる。")]
            ItGivesASparklingEffectAroundStandingVendorsStandingTenders = 180,

            /// <summary>
            /// 露店商の関連機能をすべて可能にしてくれる。(アシスタント除外)
            /// </summary>
            [Comment("露店商の関連機能をすべて可能にしてくれる。(アシスタント除外)")]
            ItAllowsAllRelatedFunctionsOfStallVendors_ExcludeAssistant_ = 181,

            /// <summary>
            /// 派手な立て札
            /// </summary>
            [Comment("派手な立て札")]
            FlashyStandingBug = 182,

            /// <summary>
            /// 不思議な立て札
            /// </summary>
            [Comment("不思議な立て札")]
            MysteriousStandUpBill = 183,

            /// <summary>
            /// きれいな立て札
            /// </summary>
            [Comment("きれいな立て札")]
            ACleanStandUpBadge = 184,

            /// <summary>
            /// 情熱的な立て札
            /// </summary>
            [Comment("情熱的な立て札")]
            PassionateStandUpNotes = 185,

            /// <summary>
            /// 拡声器
            /// </summary>
            [Comment("拡声器")]
            Loudspeaker = 186,

            /// <summary>
            /// ドロップレベル [0]～[1] 間のノーマル　アイテムをランダムに生成する。成功確率 100％
            /// </summary>
            [Comment("ドロップレベル [0]～[1] 間のノーマル　アイテムをランダムに生成する。成功確率 100％")]
            DropLevelGenerateRandomItemsBetweenAnyAndAnyNormalSuccessProbability100Percent = 187,

            /// <summary>
            /// ターゲットの火の抵抗を [0]％ 弱化させる。
            /// </summary>
            [Comment("ターゲットの火の抵抗を [0]％ 弱化させる。")]
            WeakenTheTargetsFireResistanceByAnyPercentage = 188,

            /// <summary>
            /// ターゲットの水の抵抗を [0]％ 弱化させる。
            /// </summary>
            [Comment("ターゲットの水の抵抗を [0]％ 弱化させる。")]
            WeakenTheTargetWaterResistanceAnyPercentage = 189,

            /// <summary>
            /// ターゲットの風の抵抗を [0]％ 弱化させる。
            /// </summary>
            [Comment("ターゲットの風の抵抗を [0]％ 弱化させる。")]
            WeakenTheTargetWindResistanceByAnyPercentage = 190,

            /// <summary>
            /// ターゲットの大地の抵抗を [0]％ 弱化させる。
            /// </summary>
            [Comment("ターゲットの大地の抵抗を [0]％ 弱化させる。")]
            WeakenTheResistanceOfTheTargetsEarthAnyAmount = 191,

            /// <summary>
            /// ターゲットの光の抵抗を [0]％ 弱化させる。
            /// </summary>
            [Comment("ターゲットの光の抵抗を [0]％ 弱化させる。")]
            WeakenTheResistanceOfTheTargetLightByAnyPercentage = 192,

            /// <summary>
            /// ターゲットの闇の抵抗を [0]％ 弱化させる。
            /// </summary>
            [Comment("ターゲットの闇の抵抗を [0]％ 弱化させる。")]
            WeakenTheTargetsDarkResistanceByAnyPercentage = 193,

            /// <summary>
            /// ターゲットの魔法の抵抗を [0]％ 弱化させる。
            /// </summary>
            [Comment("ターゲットの魔法の抵抗を [0]％ 弱化させる。")]
            WeakenTheTargetsMagicResistanceAnyPercent = 194,

            /// <summary>
            /// 火属性の攻撃力を [0]％ 強化させる。
            /// </summary>
            [Comment("火属性の攻撃力を [0]％ 強化させる。")]
            ImproveFirePerformanceByAnyPercentage = 195,

            /// <summary>
            /// 水属性の攻撃力を [0]％ 強化させる。
            /// </summary>
            [Comment("水属性の攻撃力を [0]％ 強化させる。")]
            ImproveTheWaterAttackStrengthByAnyPercent = 196,

            /// <summary>
            /// 風属性の攻撃力を [0]％ 強化させる。
            /// </summary>
            [Comment("風属性の攻撃力を [0]％ 強化させる。")]
            ImproveTheWindAttackStrengthByAnyPercent = 197,

            /// <summary>
            /// 大地属性の攻撃力を [0]％ 強化させる。
            /// </summary>
            [Comment("大地属性の攻撃力を [0]％ 強化させる。")]
            IncreaseTheAttackPowerOfTheEarthAttributeByAnyPercentage = 198,

            /// <summary>
            /// 光属性の攻撃力を [0]％ 強化させる。
            /// </summary>
            [Comment("光属性の攻撃力を [0]％ 強化させる。")]
            EnhanceTheAttackPowerOfLightAttributeByAnyPercent = 199,

            /// <summary>
            /// 闇属性の攻撃力を [0]％ 強化させる。
            /// </summary>
            [Comment("闇属性の攻撃力を [0]％ 強化させる。")]
            ImproveTheAttackPowerOfTheDarkAttributeByAnyPercent = 200,

            /// <summary>
            /// 魔法の攻撃力を [0]％ 強化させる。
            /// </summary>
            [Comment("魔法の攻撃力を [0]％ 強化させる。")]
            EnhanceMagicalAttackPowerAnyPercentage = 201,

            /// <summary>
            /// ペットの飼育記録書である。
            /// </summary>
            [Comment("ペットの飼育記録書である。")]
            ItIsAPetsBreedingRecord = 202,

            /// <summary>
            /// 神秘的なアイテムボックス
            /// </summary>
            [Comment("神秘的なアイテムボックス")]
            MysteriousItemBox = 203,

            /// <summary>
            /// 魔力補充キット
            /// </summary>
            [Comment("魔力補充キット")]
            MagicalSupplementKit = 204,

            /// <summary>
            /// HP回復機能のあるタートル絨毯(カーペット)を召喚する。
            /// </summary>
            [Comment("HP回復機能のあるタートル絨毯(カーペット)を召喚する。")]
            SummonATurtleCarpet_carpet_WithHPRecoveryFunction = 205,

            /// <summary>
            /// 移動してもCPが減少しないリスの絨毯(カーペット)を召喚する。
            /// </summary>
            [Comment("移動してもCPが減少しないリスの絨毯(カーペット)を召喚する。")]
            SummonACarpetOfSquirrelThatDoesNotDecreaseCPEvenIfItMoves = 206,

            /// <summary>
            /// 魔法絨毯(カーペット)よりも移動速度が早いドレイク絨毯(カーペット)を召喚する。
            /// </summary>
            [Comment("魔法絨毯(カーペット)よりも移動速度が早いドレイク絨毯(カーペット)を召喚する。")]
            SummonADrakeCarpet_carpet_WhichMovesFasterThanAMagicCarpet_carpet_ = 207,

            /// <summary>
            /// 乗ったままで攻撃可能なギア絨毯(カーペット)を召喚する。
            /// </summary>
            [Comment("乗ったままで攻撃可能なギア絨毯(カーペット)を召喚する。")]
            SummonAGearCarpet_carpet_ThatCanBeAttackedWhileRiding = 208,

            /// <summary>
            /// 旅行者用のカバンの大きさを[0]箇所拡張させる。
            /// </summary>
            [Comment("旅行者用のカバンの大きさを[0]箇所拡張させる。")]
            ExtendTheSizeOfTheBagForTravelersAnyLocation = 209,

            /// <summary>
            /// デラックスアイテムの耐久度を[0]％上昇させる。
            /// </summary>
            [Comment("デラックスアイテムの耐久度を[0]％上昇させる。")]
            IncreaseTheDurabilityOfDeluxeItemsByAnyPercentage = 210,

            /// <summary>
            /// アイテムのかけら
            /// </summary>
            [Comment("アイテムのかけら")]
            FragmentOfItem = 211,

            /// <summary>
            /// 秘密ダンジョン鍵
            /// </summary>
            [Comment("秘密ダンジョン鍵")]
            SecretDungeonKey = 212,

            /// <summary>
            /// 称号削除
            /// </summary>
            [Comment("称号削除")]
            DeleteTitle = 213,

            /// <summary>
            /// ワールドマップへフィールド移動
            /// </summary>
            [Comment("ワールドマップへフィールド移動")]
            FieldMoveToWorldMap = 214,

            /// <summary>
            /// 垂直バナー設置
            /// </summary>
            [Comment("垂直バナー設置")]
            VerticalBannerInstallation = 215,

            /// <summary>
            /// 垂直バナー設置
            /// </summary>
            [Comment("垂直バナー設置")]
            VerticalBannerInstallation2 = 216,

            /// <summary>
            /// 宝物地図
            /// </summary>
            [Comment("宝物地図")]
            TreasureMap = 217,

            /// <summary>
            /// 幸運[+0]00 [1]分
            /// </summary>
            [Comment("幸運[+0]00 [1]分")]
            FortunePlusAny00AnyMinutes = 218,

            /// <summary>
            /// ギルドスキルポイント再分配(石像スキル除外)
            /// </summary>
            [Comment("ギルドスキルポイント再分配(石像スキル除外)")]
            GuildSkillPointRedistribution_stoneSkillExclusion_ = 219,

            /// <summary>
            /// ウェイポイントスロット[0]に記憶している場所に通じるポータルを開く。
            /// </summary>
            [Comment("ウェイポイントスロット[0]に記憶している場所に通じるポータルを開く。")]
            OpenAPortalThatLeadsToTheLocationStoredInWaypointSlotAny = 220,

            /// <summary>
            /// カップルリング２つを生成する。
            /// </summary>
            [Comment("カップルリング２つを生成する。")]
            CoupleRingsToProduceTwo = 221,

            /// <summary>
            /// カップルリング
            /// </summary>
            [Comment("カップルリング")]
            CoupleRing = 222,

            /// <summary>
            /// 同じ種類のアイテム２つを交ぜる。
            /// </summary>
            [Comment("同じ種類のアイテム２つを交ぜる。")]
            MixTwoItemsOfTheSameType = 223,

            /// <summary>
            /// 花火
            /// </summary>
            [Comment("花火")]
            fireworks = 224,

            /// <summary>
            /// どのような花火が出るか、使う前にはわからない。
            /// </summary>
            [Comment("どのような花火が出るか、使う前にはわからない。")]
            IDoNotKnowWhatKindOfFireworksWillComeOutBeforeUsingIt = 225,

            /// <summary>
            /// 選択ステータス増加
            /// </summary>
            [Comment("選択ステータス増加")]
            IncreaseSelectionStatus = 226,

            /// <summary>
            /// プレゼント用カップルリング
            /// </summary>
            [Comment("プレゼント用カップルリング")]
            CoupleRingForGifts = 227,

            /// <summary>
            /// パートナー無しカップルリング
            /// </summary>
            [Comment("パートナー無しカップルリング")]
            NoPartnerCoupleRing = 228,

            /// <summary>
            /// 本にカテゴリー追加
            /// </summary>
            [Comment("本にカテゴリー追加")]
            AddCategoryToBook = 229,

            /// <summary>
            /// 本にページ追加
            /// </summary>
            [Comment("本にページ追加")]
            AddPageToBook = 230,

            /// <summary>
            /// 本
            /// </summary>
            [Comment("本")]
            Book = 231,

            /// <summary>
            /// プリンセス変身時の武器
            /// </summary>
            [Comment("プリンセス変身時の武器")]
            WeaponsWhenPrincessTransformation = 232,

            /// <summary>
            /// ギルドホール移動アイテム
            /// </summary>
            [Comment("ギルドホール移動アイテム")]
            GuildhallMoveItem = 233,

            /// <summary>
            /// ギルドガーディアン召喚
            /// </summary>
            [Comment("ギルドガーディアン召喚")]
            SummonGuildGuardian = 234,

            /// <summary>
            /// ギルド石像関連アイテム
            /// </summary>
            [Comment("ギルド石像関連アイテム")]
            GuildStoneStatuesRelatedItems = 235,

            /// <summary>
            /// ギルド石像関連アイテム
            /// </summary>
            [Comment("ギルド石像関連アイテム")]
            GuildStoneStatuesRelatedItems2 = 236,

            /// <summary>
            /// 運営者に変身することができる。
            /// </summary>
            [Comment("運営者に変身することができる。")]
            YouCanTransformIntoAnOperator = 237,

            /// <summary>
            /// 最終ダメージが[0]分間[1]0％増加する。
            /// </summary>
            [Comment("最終ダメージが[0]分間[1]0％増加する。")]
            TheFinalDamageIncreasesByAny0PercentAnyMinute = 238,

            /// <summary>
            /// モンスターを倒す時に獲得する経験値が[0]分間[1]0％増加する。
            /// </summary>
            [Comment("モンスターを倒す時に獲得する経験値が[0]分間[1]0％増加する。")]
            TheExperienceValueYouEarnWhenYouDefeatAMonsterIncreasesAnyAnyPercentageAnyMinute = 239,

            /// <summary>
            /// アイテムドロップ確率が[0]分間[1]0％増加する。
            /// </summary>
            [Comment("アイテムドロップ確率が[0]分間[1]0％増加する。")]
            TheItemDropProbabilityIncreasesAnyAnyPercentForAnyMinute = 240,

            /// <summary>
            /// 死亡ペナルティーをすべて回復させる。
            /// </summary>
            [Comment("死亡ペナルティーをすべて回復させる。")]
            RecoverAllDeathPenalty = 241,

            /// <summary>
            /// 移動中も増加する。
            /// </summary>
            [Comment("移動中も増加する。")]
            ItAlsoIncreasesDuringTravel = 242,

            /// <summary>
            /// イベント用風船
            /// </summary>
            [Comment("イベント用風船")]
            EventBalloons = 243,

            /// <summary>
            /// 神秘的なアイテムボックス
            /// </summary>
            [Comment("神秘的なアイテムボックス")]
            MysteriousItemBox2 = 244,

            /// <summary>
            /// 最終ダメージ[+1]％[0]分
            /// </summary>
            [Comment("最終ダメージ[+1]％[0]分")]
            LastDamagePlusAnyPercentAnyMinutes = 245,

            /// <summary>
            /// ミニペット召喚
            /// </summary>
            [Comment("ミニペット召喚")]
            MiniPetSummon = 246,

            /// <summary>
            /// ミニペット期間延長
            /// </summary>
            [Comment("ミニペット期間延長")]
            ExtendedMiniPetTerm = 247,

            /// <summary>
            /// 全ミニペット期間延長
            /// </summary>
            [Comment("全ミニペット期間延長")]
            ExtendedMiniPetPeriod = 248,

            /// <summary>
            /// ミニペット封印
            /// </summary>
            [Comment("ミニペット封印")]
            MiniPetSeal = 249,

            /// <summary>
            /// ミニペット名前変更
            /// </summary>
            [Comment("ミニペット名前変更")]
            MiniPetNameChange = 250,

            /// <summary>
            /// ミニペットが入った袋
            /// </summary>
            [Comment("ミニペットが入った袋")]
            BagContainingMiniPet = 251,

            /// <summary>
            /// ミニペットのエサ
            /// </summary>
            [Comment("ミニペットのエサ")]
            MinipetFood = 252,

            /// <summary>
            /// ミニペットのエサ
            /// </summary>
            [Comment("ミニペットのエサ")]
            MinipetFood2 = 253,

            /// <summary>
            /// ミニペットのエサ
            /// </summary>
            [Comment("ミニペットのエサ")]
            MinipetFood3 = 254,

            /// <summary>
            /// ミニペットを2体同時に活性化できるようになる- 期間 [0]日
            /// </summary>
            [Comment("ミニペットを2体同時に活性化できるようになる- 期間 [0]日")]
            YouWillBeAbleToActivate2MiniPetsAtTheSameTimeAndPeriodAnyDay = 255,

            /// <summary>
            /// 称号レベル上昇
            /// </summary>
            [Comment("称号レベル上昇")]
            TitleLevelRise = 256,

            /// <summary>
            /// 取引不可アイテムを取引可能に変更
            /// </summary>
            [Comment("取引不可アイテムを取引可能に変更")]
            ChangeDealNoDealItemToBeTradable = 257,

            /// <summary>
            /// ギルドスキルポイント再分配(石像スキル除外)
            /// </summary>
            [Comment("ギルドスキルポイント再分配(石像スキル除外)")]
            GuildSkillPointRedistribution_stoneSkillExclusion_2 = 258,

            /// <summary>
            /// HP [+1]
            /// </summary>
            [Comment("HP [+1]")]
            HPPlusAny = 259,

            /// <summary>
            /// CP [+1]
            /// </summary>
            [Comment("CP [+1]")]
            CPPlusAny = 260,

            /// <summary>
            /// 自分のレベルより[0]レベル高いレベル制限アイテムの使用が可能
            /// </summary>
            [Comment("自分のレベルより[0]レベル高いレベル制限アイテムの使用が可能")]
            YouCanUseLevelRestrictedItemsThatAreAnyLevelHigherThanYourOwnLevel = 261,

            /// <summary>
            /// スキル１つ初期化
            /// </summary>
            [Comment("スキル１つ初期化")]
            InitializeOneSkill = 262,

            /// <summary>
            /// ステータス１つ初期化
            /// </summary>
            [Comment("ステータス１つ初期化")]
            InitializeOneStatus = 263,

            /// <summary>
            /// 称号除去
            /// </summary>
            [Comment("称号除去")]
            TitleRemoval = 264,

            /// <summary>
            /// アイテム複製
            /// </summary>
            [Comment("アイテム複製")]
            ItemDuplication = 265,

            /// <summary>
            /// 一般アイテム称号アップグレード
            /// </summary>
            [Comment("一般アイテム称号アップグレード")]
            GeneralItemTitleUpgrade = 266,

            /// <summary>
            /// 一般アイテム性能向上
            /// </summary>
            [Comment("一般アイテム性能向上")]
            GeneralItemPerformanceImprovement = 267,

            /// <summary>
            /// ユニークアイテム称号アップグレード
            /// </summary>
            [Comment("ユニークアイテム称号アップグレード")]
            UniqueItemTitleUpgrade = 268,

            /// <summary>
            /// ユニークアイテム性能向上
            /// </summary>
            [Comment("ユニークアイテム性能向上")]
            UniqueItemPerformanceImprovement = 269,

            /// <summary>
            /// 同じ種類のアイテム２つを交ぜる
            /// </summary>
            [Comment("同じ種類のアイテム２つを交ぜる")]
            MixTwoItemsOfTheSameType2 = 270,

            /// <summary>
            /// 性向数値選択
            /// </summary>
            [Comment("性向数値選択")]
            SelectionOfTendencyNumber = 271,

            /// <summary>
            /// タイムスタンプ
            /// </summary>
            [Comment("タイムスタンプ")]
            TimeStamp = 272,

            /// <summary>
            /// 称号追加
            /// </summary>
            [Comment("称号追加")]
            TitleAdded = 273,

            /// <summary>
            /// 狩りでの経験値ボーナス
            /// </summary>
            [Comment("狩りでの経験値ボーナス")]
            ExperienceBonusOnHunting = 274,

            /// <summary>
            /// バッジに刻印する
            /// </summary>
            [Comment("バッジに刻印する")]
            EngraveOnABadge = 275,

            /// <summary>
            /// 生命石
            /// </summary>
            [Comment("生命石")]
            LifeStone = 276,

            /// <summary>
            /// 魔法絨毯(カーペット)よりも移動速度が早いファイヤードレイク絨毯(カーペット)を召喚する。
            /// </summary>
            [Comment("魔法絨毯(カーペット)よりも移動速度が早いファイヤードレイク絨毯(カーペット)を召喚する。")]
            SummonAFiredrakeCarpet_carpet_ThatMovesFasterThanAMagicCarpet_carpet_ = 277,

            /// <summary>
            /// 火炎の激怒
            /// </summary>
            [Comment("火炎の激怒")]
            FuriousRage = 278,

            /// <summary>
            /// 水の波動
            /// </summary>
            [Comment("水の波動")]
            WaterWave = 279,

            /// <summary>
            /// 風の加護
            /// </summary>
            [Comment("風の加護")]
            WindProtection = 280,

            /// <summary>
            /// 大地の響き
            /// </summary>
            [Comment("大地の響き")]
            EchoesOfTheEarth = 281,

            /// <summary>
            /// ルーンを刻印する
            /// </summary>
            [Comment("ルーンを刻印する")]
            EngraveRune = 282,

            /// <summary>
            /// 覚醒の種
            /// </summary>
            [Comment("覚醒の種")]
            AwakeningSeed = 283,

            /// <summary>
            /// 覚醒の果実
            /// </summary>
            [Comment("覚醒の果実")]
            AwakedFruits = 284,

            /// <summary>
            /// ギルドマーク・シグナルパッケージ
            /// </summary>
            [Comment("ギルドマーク・シグナルパッケージ")]
            GuildMarkSignalPackage = 285,

            /// <summary>
            /// ギルドマーク・ユニーク１パッケージ
            /// </summary>
            [Comment("ギルドマーク・ユニーク１パッケージ")]
            GuildMarkEUnique1Package = 286,

            /// <summary>
            /// ギルドマーク・ユニーク２パッケージ
            /// </summary>
            [Comment("ギルドマーク・ユニーク２パッケージ")]
            GuildMarkEUnique2Package = 287,

            /// <summary>
            /// HPを [0]00ポイント回復
            /// </summary>
            [Comment("HPを [0]00ポイント回復")]
            Any00PointRecoveryHP = 288,

            /// <summary>
            /// CPを [0]00ポイント充填
            /// </summary>
            [Comment("CPを [0]00ポイント充填")]
            FillAnyCPForCP = 289,

            /// <summary>
            /// [*]
            /// </summary>
            [Comment("[*]")]
            INVALID = 290,

            /// <summary>
            /// ステータスポイント 力 再配分
            /// </summary>
            [Comment("ステータスポイント 力 再配分")]
            StatusPointPowerRedistribution = 291,

            /// <summary>
            /// ステータスポイント 敏捷 再配分
            /// </summary>
            [Comment("ステータスポイント 敏捷 再配分")]
            StatusPointAgilityRedistribution = 292,

            /// <summary>
            /// ステータスポイント 健康 再配分
            /// </summary>
            [Comment("ステータスポイント 健康 再配分")]
            StatusPointHealthRedistribution = 293,

            /// <summary>
            /// ステータスポイント 知識 再配分
            /// </summary>
            [Comment("ステータスポイント 知識 再配分")]
            StatusPointKnowledgeRedistribution = 294,

            /// <summary>
            /// ステータスポイント 知恵 再配分
            /// </summary>
            [Comment("ステータスポイント 知恵 再配分")]
            StatusPointWisdomRedistribution = 295,

            /// <summary>
            /// ステータスポイント カリスマ 再配分
            /// </summary>
            [Comment("ステータスポイント カリスマ 再配分")]
            StatusPointCharismaRedistribution = 296,

            /// <summary>
            /// ステータスポイント 運 再配分
            /// </summary>
            [Comment("ステータスポイント 運 再配分")]
            StatusPointLuckAllocation = 297,

            /// <summary>
            /// ワールドマップ上のフィールドを自由に移動できる。
            /// </summary>
            [Comment("ワールドマップ上のフィールドを自由に移動できる。")]
            YouCanMoveTheFieldOnTheWorldMapFreely = 298,

            /// <summary>
            /// 戦闘不能になったパーティーメンバーを全員復活させて[0]秒間最大HP、CPを[1]％増加させる。
            /// </summary>
            [Comment("戦闘不能になったパーティーメンバーを全員復活させて[0]秒間最大HP、CPを[1]％増加させる。")]
            ReviveAllPartyMembersWhoBecameUnableToFightAndIncreaseAnyMaximumHPAndCPAnyTimeForAnySeconds = 299,

            /// <summary>
            /// 一日の秘密ダンジョン入場制限回数を[0]回増加させる。
            /// </summary>
            [Comment("一日の秘密ダンジョン入場制限回数を[0]回増加させる。")]
            IncreaseTheSecretDungeonAdmissionLimitNumberOfTheDayAnyTime = 300,

            /// <summary>
            /// ミニペットがエサを食べて獲得する際の経験値を[0]ポイント増加させる。
            /// </summary>
            [Comment("ミニペットがエサを食べて獲得する際の経験値を[0]ポイント増加させる。")]
            IncreaseTheExperiencePointWhenMiniPetsEatAndFeedAnyPoint = 301,

            /// <summary>
            /// 移動速度 [0]％増加
            /// </summary>
            [Comment("移動速度 [0]％増加")]
            MovementSpeedAnyPercentIncrease = 302,

            /// <summary>
            /// HP [0]％すぐに回復
            /// </summary>
            [Comment("HP [0]％すぐに回復")]
            HPAnyPercentSoonRecovery = 303,

            /// <summary>
            /// HP [0]00ポイントすぐに回復
            /// </summary>
            [Comment("HP [0]00ポイントすぐに回復")]
            HPAny00PointRecoverySoon = 304,

            /// <summary>
            /// 一日のギルドダンジョン入場制限回数を[0]回増加させる。
            /// </summary>
            [Comment("一日のギルドダンジョン入場制限回数を[0]回増加させる。")]
            IncreaseTheNumberOfAdmissionTimesOfGuildDungeonOfADayAnyTime = 305,

            /// <summary>
            /// ペットの名前をno nameに戻し、名前を変更することができる。
            /// </summary>
            [Comment("ペットの名前をno nameに戻し、名前を変更することができる。")]
            YouCanRenameYourPetsNameBackToNoName = 306,

            /// <summary>
            /// [0]時間の間、すべてのプレイヤーの経験値とアイテムドロップ率を[1]ポイント増加する。
            /// </summary>
            [Comment("[0]時間の間、すべてのプレイヤーの経験値とアイテムドロップ率を[1]ポイント増加する。")]
            DuringAnyTime_IncreaseTheExperiencePointAndItemDropRateOfAllPlayersAnyPoint = 307,

            /// <summary>
            /// [0]％確率で物理ダメージの[1]％を 体力吸収
            /// </summary>
            [Comment("[0]％確率で物理ダメージの[1]％を 体力吸収")]
            AnyPercentProbabilityToAbsorbAnyPercentageOfPhysicalDamage = 308,

            /// <summary>
            /// ミニペットを調和させる。
            /// </summary>
            [Comment("ミニペットを調和させる。")]
            HarmonizeMiniPets = 309,

            /// <summary>
            /// ミニペットのオーラを取り込む
            /// </summary>
            [Comment("ミニペットのオーラを取り込む")]
            CaptureMiniPetAura = 310,

            /// <summary>
            /// [0]0レベル以下のアイテム 1つを能力に関係なく使用可能にする。
            /// </summary>
            [Comment("[0]0レベル以下のアイテム 1つを能力に関係なく使用可能にする。")]
            MakeOneItemBelowAny0LevelAvailableRegardlessOfAbility = 311,

            /// <summary>
            /// 火属性 ダメージ [+0]
            /// </summary>
            [Comment("火属性 ダメージ [+0]")]
            FireAttributeDamagePlusAny2 = 312,

            /// <summary>
            /// 水属性 ダメージ  [+0]
            /// </summary>
            [Comment("水属性 ダメージ  [+0]")]
            WaterAttributeDamagePlusAny2 = 313,

            /// <summary>
            /// 風属性 ダメージ  [+0]
            /// </summary>
            [Comment("風属性 ダメージ  [+0]")]
            WindAttributeDamagePlusAny2 = 314,

            /// <summary>
            /// 大地属性 ダメージ  [+0]
            /// </summary>
            [Comment("大地属性 ダメージ  [+0]")]
            EarthDamageDamagePlusAny2 = 315,

            /// <summary>
            /// 光属性 ダメージ  [+0]
            /// </summary>
            [Comment("光属性 ダメージ  [+0]")]
            LightAttributeDamagePlusAny2 = 316,

            /// <summary>
            /// 闇属性 ダメージ  [+0]
            /// </summary>
            [Comment("闇属性 ダメージ  [+0]")]
            DarkDamageDamagePlusAny2 = 317,

            /// <summary>
            /// 霊術師のスキルレベル [+0]
            /// </summary>
            [Comment("霊術師のスキルレベル [+0]")]
            SpiritistSkillLevelPlusAny = 318,

            /// <summary>
            /// [0]％の確率でアイテムの称号を付ける
            /// </summary>
            [Comment("[0]％の確率でアイテムの称号を付ける")]
            TitleTheItemWithTheProbabilityOfAnyPercent = 319,

            /// <summary>
            /// [0]％の確率でアイテムの称号をコピーする
            /// </summary>
            [Comment("[0]％の確率でアイテムの称号をコピーする")]
            CopyTheTitleOfTheItemWithTheProbabilityOfAnyPercent = 320,

            /// <summary>
            /// 選択したアイテムの称号を削除する
            /// </summary>
            [Comment("選択したアイテムの称号を削除する")]
            DeleteTheTitleOfTheSelectedItem = 321,

            /// <summary>
            /// 残り体力をすぐに減少 [0]％
            /// </summary>
            [Comment("残り体力をすぐに減少 [0]％")]
            ImmediateDecreaseInPhysicalStrengthRemainingAnyPercent = 322,

            /// <summary>
            /// [0]％の確率でアイテムの称号を付ける【 取引不可 】
            /// </summary>
            [Comment("[0]％の確率でアイテムの称号を付ける【 取引不可 】")]
            TitleTheItemWithTheProbabilityOfAnyPercent_NoTransactions_ = 323,

            /// <summary>
            /// [0]％の確率でアイテムの称号をコピーする【 取引不可 】
            /// </summary>
            [Comment("[0]％の確率でアイテムの称号をコピーする【 取引不可 】")]
            CopyTheTitleOfTheItemWithTheProbabilityOfAnyPercent_NoTransactions_ = 324,

            /// <summary>
            /// 選択したアイテムの称号を削除する
            /// </summary>
            [Comment("選択したアイテムの称号を削除する")]
            DeleteTheTitleOfTheSelectedItem2 = 325,

            /// <summary>
            /// イベント用風船
            /// </summary>
            [Comment("イベント用風船")]
            EventBalloons2 = 326,

            /// <summary>
            /// すべての能力値 [+0]
            /// </summary>
            [Comment("すべての能力値 [+0]")]
            AllAbilityValuesPlusAny = 327,

            /// <summary>
            /// 選択したステータス [+0]増加
            /// </summary>
            [Comment("選択したステータス [+0]増加")]
            SelectedStatusPlusAnyIncrease = 328,

            /// <summary>
            /// [0]分間 レベル[1]の狩人に変身
            /// </summary>
            [Comment("[0]分間 レベル[1]の狩人に変身")]
            MakeAHangerOfAnyAnyLevelAHunter = 329,

            /// <summary>
            /// [0]分間 レベル[1]のアサシンに変身
            /// </summary>
            [Comment("[0]分間 レベル[1]のアサシンに変身")]
            TransformedToAssassinOfAnyAnyLevelOfAny = 330,

            /// <summary>
            /// [0]分間 レベル[1]のブラックメイジに変身
            /// </summary>
            [Comment("[0]分間 レベル[1]のブラックメイジに変身")]
            MakeItAAnyMinuteLevelBlackMage = 331,

            /// <summary>
            /// 現在、フィールドにいるモンスターを召喚する。
            /// </summary>
            [Comment("現在、フィールドにいるモンスターを召喚する。")]
            CurrentlySummonMonstersInTheField = 332,

            /// <summary>
            /// 刻印レベル[0]
            /// </summary>
            [Comment("刻印レベル[0]")]
            MarkingLevelAny = 333,

            /// <summary>
            /// [0]％の確率で刻印レベルを 1～[1]増加
            /// </summary>
            [Comment("[0]％の確率で刻印レベルを 1～[1]増加")]
            IncreaseTheStampingLevel1ToAnyWithTheProbabilityOfAnyPercent = 334,

            /// <summary>
            /// [0]％の確率で刻印レベルを[1]増加
            /// </summary>
            [Comment("[0]％の確率で刻印レベルを[1]増加")]
            AnyIncrementOfStampingLevelWithAnyPercentProbability = 335,

            /// <summary>
            /// [0]％の確率で受けたダメージ [0]％減少
            /// </summary>
            [Comment("[0]％の確率で受けたダメージ [0]％減少")]
            DamageReceivedAtAnyPercentProbabilityReceivedAnyPercentDecrease = 336,

            /// <summary>
            /// [0]秒ごとにCPを [0]回復
            /// </summary>
            [Comment("[0]秒ごとにCPを [0]回復")]
            CPAnyRecoveryEverySecond = 337,

            /// <summary>
            /// 獲得経験値 [0]％増加
            /// </summary>
            [Comment("獲得経験値 [0]％増加")]
            AcquiredExperienceValueAnyPercentageIncrease = 338,

            /// <summary>
            /// [0]％の確率で火属性ダメージ [0]追加
            /// </summary>
            [Comment("[0]％の確率で火属性ダメージ [0]追加")]
            AdditionalProbabilityOfAnyPercentFireAttributeAddedAny = 339,

            /// <summary>
            /// すべてのステータス [0]増加
            /// </summary>
            [Comment("すべてのステータス [0]増加")]
            AllStatusAnyAnyIncrease = 340,

            /// <summary>
            /// [0]秒間 すべてのパーティーメンバー無敵状態
            /// </summary>
            [Comment("[0]秒間 すべてのパーティーメンバー無敵状態")]
            AnyPartyInvincibleStateForAllPartyMembers = 341,

            /// <summary>
            /// レベル[0]のファミリアに [0]分間 変身
            /// </summary>
            [Comment("レベル[0]のファミリアに [0]分間 変身")]
            TransformAnyMinuteIntoLevelFamilyAnyFamily = 342,

            /// <summary>
            /// 最大体力 [0]増加
            /// </summary>
            [Comment("最大体力 [0]増加")]
            MaximumPhysicalStrengthAny = 343,

            /// <summary>
            /// 使用時、[0]分間 攻撃力 [0]％増加
            /// </summary>
            [Comment("使用時、[0]分間 攻撃力 [0]％増加")]
            AtUse_AnyMinutesIncreasesAttackPowerAnyPercentage = 344,

            /// <summary>
            /// レベル[0]のヘルプリズン使用
            /// </summary>
            [Comment("レベル[0]のヘルプリズン使用")]
            UseOfAnyLevelOfHERPRUZEN = 345,

            /// <summary>
            /// ミニペットのエサ
            /// </summary>
            [Comment("ミニペットのエサ")]
            MinipetFood4 = 346,

            /// <summary>
            /// ミニペットのスキルポイント 再配分
            /// </summary>
            [Comment("ミニペットのスキルポイント 再配分")]
            RedistributionOfMiniPetSkillPoints = 347,

            /// <summary>
            /// ミニペットの進化形態変化
            /// </summary>
            [Comment("ミニペットの進化形態変化")]
            ChangesInEvolutionaryFormOfMinipets = 348,

            /// <summary>
            /// 最大HP [+0]
            /// </summary>
            [Comment("最大HP [+0]")]
            MaxHPPlusAny = 349,

            /// <summary>
            /// 最大CP [+0]
            /// </summary>
            [Comment("最大CP [+0]")]
            MaxCPPlusAny = 350,

            /// <summary>
            /// 防御力 [+0]％
            /// </summary>
            [Comment("防御力 [+0]％")]
            DefensePowerPlusAnyPercent = 351,

            /// <summary>
            /// 最大HP [+0]％
            /// </summary>
            [Comment("最大HP [+0]％")]
            MaxHPPlusAnyPercent = 352,

            /// <summary>
            /// 最大CP [+0]％
            /// </summary>
            [Comment("最大CP [+0]％")]
            MaxCPPlusAnyPercent = 353,

            /// <summary>
            /// 残り体力を [(0*1)] ポイントに変更
            /// </summary>
            [Comment("残り体力を [(0*1)] ポイントに変更")]
            ChangeRemainingPhysicalStrengthToAnyTimesAnyPoint = 354,

            /// <summary>
            /// フィールド移動
            /// </summary>
            [Comment("フィールド移動")]
            FieldMove = 355,

            /// <summary>
            /// RED STONE獲得可能レベル [+0]
            /// </summary>
            [Comment("RED STONE獲得可能レベル [+0]")]
            REDSTONEAcceptableLevelPlusAny = 356,
        }
        
        /// <summary>
        /// アイテムの義務
        /// </summary>
        public void ItemDuty(ItemEffect m_value)
        {
            switch (m_value)
            {
                /// <summary>
                /// スタックアイテム [0]個
                /// </summary>
                case ItemEffect.StackItemAny:
                    break;
                /// <summary>
                /// 無限弾丸
                /// </summary>
                case ItemEffect.InfiniteBullet:
                    break;
            }
        }

        /// <summary>
        /// 回復系アイテムの機能
        /// </summary>
        public void ItemFunction_Cure(ItemEffect m_value)
        {
            switch (m_value)
            {
                /// <summary>
                /// HP回復 [0]ポイント
                /// </summary>
                case ItemEffect.HPRecoveryAnyPoint:
                    break;
                /// <summary>
                /// HP回復 [0]％
                /// </summary>
                case ItemEffect.HPRecoveryAnyPercent:
                    break;
                /// <summary>
                /// CP充填 [0]ポイント
                /// </summary>
                case ItemEffect.CPFillingAnyPoint:
                    break;
                /// <summary>
                /// CP充填 [0]％
                /// </summary>
                case ItemEffect.CPFillAnyPercent:
                    break;
                /// <summary>
                /// HP回復 [0]ポイント- CP充填 [0]ポイント
                /// </summary>
                case ItemEffect.HPRecoveryAnyPointAndCPFillingAnyPoint:
                    break;
                /// <summary>
                /// HP回復 [0]％- CP充填 [0]％
                /// </summary>
                case ItemEffect.HPRecoveryAnyPercentAndCPFillingAnyPercent:
                    break;
                /// <summary>
                /// 戦闘不能のキャラクター復活- HP回復 [0]％
                /// </summary>
                case ItemEffect.BattleImpossibleCharacterResurrectionAndHPRecoveryAnyPercent:
                    break;
                /// <summary>
                /// 戦闘不能状態で復活- HP回復 [0]％
                /// </summary>
                case ItemEffect.ResurrectionInNonCombatStateAndHPRecoveryAnyPercent:
                    break;
                /// <summary>
                /// 状態異常の中和 [0]％
                /// </summary>
                case ItemEffect.NeutralizationOfStateAnomaliesAnyPercent:
                    break;
                /// <summary>
                /// すべての異常系の状態治療
                /// </summary>
                case ItemEffect.TreatmentOfAllAbnormalSystemConditions:
                    break;
                /// <summary>
                /// すべての低下系の状態治療
                /// </summary>
                case ItemEffect.ConditionTreatmentOfAllLoweringSystem:
                    break;
                /// <summary>
                /// すべての呪い系の状態治療 
                /// </summary>
                case ItemEffect.TreatmentOfTheStatusOfAllCurses:
                    break;
                /// <summary>
                /// すべての状態異常の治療
                /// </summary>
                case ItemEffect.TreatmentOfAllConditionAbnormalities:
                    break;
                /// <summary>
                /// 毒状態の治療
                /// </summary>
                case ItemEffect.TreatmentOfPoisonousCondition:
                    break;
                /// <summary>
                /// 死亡ペナルティー時間を90％減少して、[0]秒の間、最大HPを[1]％にする。
                /// </summary>
                case ItemEffect.ReduceTheDeathPenaltyTimeBy90PercentAndSetAnyMaximumPercentForAnySecondsForAnySeconds:
                    break;
            }
        }

        /// <summary>
        /// 制限時間付きのパワーアップアイテムの機能
        /// </summary>
        public void ItemFunction_PowerUpWithTimeLimit(ItemEffect m_value)
        {
            switch (m_value)
            {
                /// <summary>
                /// 力 [+0] [1]秒
                /// </summary>
                case ItemEffect.StrengthPlusAnyAnySeconds:
                    break;
                /// <summary>
                /// 敏捷 [+0] [1]秒
                /// </summary>
                case ItemEffect.AgilityPlusAnyAnySeconds:
                    break;
                /// <summary>
                /// 健康 [+0] [1]秒
                /// </summary>
                case ItemEffect.ConditionPlusAnyAnySeconds:
                    break;
                /// <summary>
                /// 知恵 [+0] [1]秒
                /// </summary>
                case ItemEffect.WisdomPlusAnyAnySeconds:
                    break;
                /// <summary>
                /// 知識 [+0] [1]秒
                /// </summary>
                case ItemEffect.KnowledgePlusAnyAnySeconds:
                    break;
                /// <summary>
                /// カリスマ [+0] [1]秒
                /// </summary>
                case ItemEffect.CharismaPlusAnyAnySeconds:
                    break;
                /// <summary>
                /// 幸運 [+0] [1]秒
                /// </summary>
                case ItemEffect.LuckeyPlusAnyAnySeconds:
                    break;
                /// <summary>
                /// 攻撃力 [+1]％ [0]秒
                /// </summary>
                case ItemEffect.AttackPowerPlusAnyPercentAnySeconds:
                    break;
                /// <summary>
                /// 防御力 [+1]％ [0]秒
                /// </summary>
                case ItemEffect.DefensePowerPlusAnyPercentAnySeconds:
                    break;
                /// <summary>
                /// 最大HP [+1]％ [0]秒
                /// </summary>
                case ItemEffect.MaxHPPlusAnyPercentAnySeconds:
                    break;
                /// <summary>
                /// 最大CP [+1]％ [0]秒
                /// </summary>
                case ItemEffect.MaxCPPlusAnyPercentAnySeconds:
                    break;
                /// <summary>
                /// 武器ダメージを最高に維持 [0]秒
                /// </summary>
                case ItemEffect.MaintainWeaponDamageAtMaximumMax:
                    break;
                /// <summary>
                /// CPを最高に維持 [0]秒
                /// </summary>
                case ItemEffect.MaintainCPAtMaximumAnySeconds:
                    break;
                /// <summary>
                /// 使用可能回数[1]回(維持時間 [0]分)
                /// </summary>
                case ItemEffect.AvailableNumberOfTimesAny_maintenanceTimeAny_:
                    break;
                /// <summary>
                /// [0]分の間、攻撃力を [1]％ 増加させる。
                /// </summary>
                case ItemEffect.IncreaseTheAttackPowerAnyPercentDuringAnyMinutes:
                    break;
                /// <summary>
                /// [0]分の間、防御力を [1]％ 増加させる。
                /// </summary>
                case ItemEffect.IncreaseYourDefensePowerAnyAmountDuringAnyMinutes:
                    break;
                /// <summary>
                /// [0]分の間、HPを [1]％ 増加させる。
                /// </summary>
                case ItemEffect.IncreaseHPAnyPercentForAnyMinutes:
                    break;
                /// <summary>
                /// [0]分の間、CPを [1]％ 増加させる。
                /// </summary>
                case ItemEffect.IncreaseCPAnyPercentForAnyMinutes:
                    break;
                /// <summary>
                /// [0]分の間、武器ダメージが最高に維持される。
                /// </summary>
                case ItemEffect.WeaponDamageIsKeptAtItsMaximumDuringAnyMinutes:
                    break;
                /// <summary>
                /// [0]分の間、CPがいつも最高に維持される。
                /// </summary>
                case ItemEffect.CPIsAlwaysKeptHighestForAnyMinutes:
                    break;
                /// <summary>
                /// 力を [0]ほど [1]分の間、上昇させる。
                /// </summary>
                case ItemEffect.IncreaseForceAsMuchAsAnyForAny:
                    break;
                /// <summary>
                /// 敏捷を [0]ほど [1]分の間、上昇させる。
                /// </summary>
                case ItemEffect.IncreaseAgilityAsMuchAsAnyForAny:
                    break;
                /// <summary>
                /// 健康を [0]ほど [1]分の間、上昇させる。
                /// </summary>
                case ItemEffect.IncreaseHealthAsMuchAsAnyForAny:
                    break;
                /// <summary>
                /// 知恵を [0]ほど [1]分の間、上昇させる。
                /// </summary>
                case ItemEffect.RaiseWisdomForAnyAmountAsMuchAsAny:
                    break;
                /// <summary>
                /// 知識を [0]ほど [1]分の間、上昇させる。
                /// </summary>
                case ItemEffect.IncreaseKnowledgeAsMuchAsAnyForAny:
                    break;
                /// <summary>
                /// カリスマを [0]ほど [1]分の間、上昇させる。
                /// </summary>
                case ItemEffect.IncreaseTheCharismaAsMuchAsAnyForAny:
                    break;
                /// <summary>
                /// 運を [0]ほど [1]分の間、上昇させる。
                /// </summary>
                case ItemEffect.IncreaseLuckForAnyAmountAsMuchAsAny:
                    break;
                /// <summary>
                /// [0]分の間、攻撃力を [1]0％ 増加させる。
                /// </summary>
                case ItemEffect.IncreaseTheAttackPowerAny0PercentForAnyMinutes:
                    break;
                /// <summary>
                /// [0]分の間、防御力 [1]0％ 増加させる。
                /// </summary>
                case ItemEffect.IncreaseYourDefenseAnyAnyPercentageForAnyMinutes:
                    break;
                /// <summary>
                /// 幸運[+0]00 [1]分
                /// </summary>
                case ItemEffect.FortunePlusAny00AnyMinutes:
                    break;
            }
        }

        /// <summary>
        /// 能力上昇アイテムの機能
        /// </summary>
        public void ItemFunction_IncreaseAbility(ItemEffect m_value)
        {
            switch (m_value)
            {
                /// <summary>
                /// 経験値 1.5倍- アイテム ドロップ率 1.5倍- 街　無制限の帰還可能- プレミアムゾーン進入- タウン  テレポーター無料使用- 魔法のカーペット無限召喚可能
                /// </summary>
                case ItemEffect.ExperienceValue15TimesAndItemDropRate15TimesAndTownUnlimitedReturnableAndPremiumZoneEntryAndTownTeleporterFreeUseAndMagicCarpetInfiniteSummonable:
                    break;
                /// <summary>
                /// 自分のレベルより [0]レベル高い制限レベルのアイテム使用可能
                /// </summary>
                case ItemEffect.YouCanUseItemsOfAnyLevelHigherThanYourOwnLevel:
                    break;
                /// <summary>
                /// レベル [0]が上昇する。
                /// </summary>
                case ItemEffect.LevelAnyRises:
                    break;
                /// <summary>
                /// 座っている時、回復速度が [0]％ 増加する。
                /// </summary>
                case ItemEffect.WhenSitting_TheRecoveryRateIncreasesAnyPercent:
                    break;
            }
        }

        /// <summary>
        /// 確率的能力上昇アイテムの機能
        /// </summary>
        public void ItemFunction_StochasticIncreaseAbility(ItemEffect m_value)
        {
            switch (m_value)
            {
                /// <summary>
                /// 力 [+0] (成功確率 : [1]％)
                /// </summary>
                case ItemEffect.PowerPlusAny_SuccessProbability_AnyPercent_:
                    break;
                /// <summary>
                /// 敏捷 [+0] (成功確率 : [1]％)
                /// </summary>
                case ItemEffect.AgilityPlusAny_successProbability_AnyPercent_:
                    break;
                /// <summary>
                /// 健康 [+0] (成功確率 : [1]％)
                /// </summary>
                case ItemEffect.HealthPlusAny_SuccessProbability_AnyPercent_:
                    break;
                /// <summary>
                /// 知恵 [+0] (成功確率 : [1]％)
                /// </summary>
                case ItemEffect.WisdomPlusAny_successProbability_AnyPercent_:
                    break;
                /// <summary>
                /// 知識 [+0] (成功確率 : [1]％)
                /// </summary>
                case ItemEffect.KnowledgePlusAny_probabilityOfSuccess_AnyPercent_:
                    break;
                /// <summary>
                /// カリスマ [+0] (成功確率 : [1]％)
                /// </summary>
                case ItemEffect.CharismaPlusAny_probabilityOfSuccess_AnyPercent_:
                    break;
                /// <summary>
                /// 幸運 [+0] (成功確率 : [1]％)
                /// </summary>
                case ItemEffect.GoodLuckPlusAny_SuccessProbability_AnyPercent_:
                    break;
                /// <summary>
                /// 最大 HP [+0] (失敗確率 : [0]*[1]％)
                /// </summary>
                case ItemEffect.MaxHPPlusAny_failureProbability_AnyTimesAnyPercent_:
                    break;
                /// <summary>
                /// 最大 CP [+0] (失敗確率 : [0]*[1]％)
                /// </summary>
                case ItemEffect.MaxCPPlusAny_failureProbability_AnyTimesAnyPercent_:
                    break;
                /// <summary>
                /// 武器 ダメージ  [+0]％ (失敗確率 : [0]+0.65％)
                /// </summary>
                case ItemEffect.WeaponDamagePlusAnyPercent_failureProbability_AnyPlus065Percent_:
                    break;
                /// <summary>
                /// 鎧 防御力 [+0] (失敗確率 : [0]+0.65％)
                /// </summary>
                case ItemEffect.ArmorDefensePowerPlusAny_failureProbability_AnyPlus065Percent_:
                    break;
            }
        }

        /// <summary>
        /// 生成系アイテムの機能
        /// </summary>
        public void ItemFunction_Production(ItemEffect m_value)
        {
            switch (m_value)
            {
                /// <summary>
                /// [0]個の制限なしの称号の作成可能
                /// </summary>
                case ItemEffect.CreateAnyNumberOfUnlimitedTitles:
                    break;
                /// <summary>
                /// 称号 [0]個  生成 (成功確率 : [1]％)
                /// </summary>
                case ItemEffect.TitleCreationOfAnyNumber_successProbability_AnyPercent_:
                    break;
                /// <summary>
                /// 使用者に [0]レベル フロンティア　タイトルを付与します。
                /// </summary>
                case ItemEffect.GrantTheUserAnyLevelFrontierTitle:
                    break;
                /// <summary>
                /// ドロップレベル [0]のレア　アイテムをランダムに生成する。成功確率 [1]％
                /// </summary>
                case ItemEffect.RareItemsOfDropLevelAnyAreRandomlyGeneratedSuccessProbabilityAnyPercent:
                    break;
                /// <summary>
                /// ドロップレベル [0]のユニーク　アイテムをランダムに生成する。成功確率 [1]％
                /// </summary>
                case ItemEffect.RandomlyGenerateUniqueItemsOfDropLevelAnySuccessProbabilityAnyPercent:
                    break;
                /// <summary>
                /// ドロップレベル [0]のスーパー　ユニークをランダムに生成する。成功確率 [1]％
                /// </summary>
                case ItemEffect.RandomlyGenerateASuperUniqueWithADropLevelOfAnySuccessProbabilityAnyPercent:
                    break;
                /// <summary>
                /// 現在の位置を場所スロット[0]に覚えてクリスタルを[1]個生成する。
                /// </summary>
                case ItemEffect.RememberTheCurrentPositionInPlaceSlotAny_AndGenerateAnyNumberOfCrystals:
                    break;
                /// <summary>
                /// ギルドを作成してくれる。
                /// </summary>
                case ItemEffect.ItWillCreateAGuild:
                    break;
                /// <summary>
                /// ドロップレベル [0]～[1] 間のノーマル　アイテムをランダムに生成する。成功確率 100％
                /// </summary>
                case ItemEffect.DropLevelGenerateRandomItemsBetweenAnyAndAnyNormalSuccessProbability100Percent:
                    break;
                /// <summary>
                /// ペットの飼育記録書である。
                /// </summary>
                case ItemEffect.ItIsAPetsBreedingRecord:
                    break;
                /// <summary>
                /// 神秘的なアイテムボックス
                /// </summary>
                case ItemEffect.MysteriousItemBox:
                    break;
                /// <summary>
                /// 旅行者用のカバンの大きさを[0]箇所拡張させる。
                /// </summary>
                case ItemEffect.ExtendTheSizeOfTheBagForTravelersAnyLocation:
                    break;
                /// <summary>
                /// 称号削除
                /// </summary>
                case ItemEffect.DeleteTitle:
                    break;
                /// <summary>
                /// 垂直バナー設置
                /// </summary>
                case ItemEffect.VerticalBannerInstallation:
                    break;
                /// <summary>
                /// 垂直バナー設置
                /// </summary>
                case ItemEffect.VerticalBannerInstallation2:
                    break;
            }
        }

        /// <summary>
        /// 操作系アイテムの機能
        /// </summary>
        public void ItemFunction_Operation(ItemEffect m_value)
        {
            switch (m_value)
            {
                /// <summary>
                /// ジム・モリのアイテム エンチャント
                /// </summary>
                case ItemEffect.JimMorisItemEnchantment:
                    break;
                /// <summary>
                /// 壊れたアイテムを不器用に修理する。
                /// </summary>
                case ItemEffect.RepairBrokenItemsClumsy:
                    break;
                /// <summary>
                /// アイテムエンチャント
                /// </summary>
                case ItemEffect.ItemEnchantment:
                    break;
                /// <summary>
                /// 壊れたアイテムを完璧に修理する。
                /// </summary>
                case ItemEffect.RepairBrokenItemsPerfectly:
                    break;
                /// <summary>
                /// アイテムにかかっている呪いを解いてくれる。
                /// </summary>
                case ItemEffect.ItWillSolveTheCurseThatIsHangingOnTheItem:
                    break;
                /// <summary>
                /// アイテム改良制限を解いてくれる。
                /// </summary>
                case ItemEffect.ItSolvesTheItemImprovementRestriction:
                    break;
                /// <summary>
                /// ギルドのレベルを [0]にしてくれる。
                /// </summary>
                case ItemEffect.ItMakesTheLevelOfTheGuildAny:
                    break;
                /// <summary>
                /// ギルドマークを選ぶことができる選択の幅を増やしてくれる。
                /// </summary>
                case ItemEffect.ItIncreasesTheRangeOfChoicesThatYouCanChooseGuildMarks:
                    break;
                /// <summary>
                /// ギルドマークを選ぶことができる選択の幅を増やしてくれる。
                /// </summary>
                case ItemEffect.ItIncreasesTheRangeOfChoicesThatYouCanChooseGuildMarks2:
                    break;
                /// <summary>
                /// ギルドマークを選ぶことができる選択の幅を増やしてくれる。
                /// </summary>
                case ItemEffect.ItIncreasesTheRangeOfChoicesThatYouCanChooseGuildMarks3:
                    break;
                /// <summary>
                /// ギルドマークを選ぶことができる選択の幅を増やしてくれる。
                /// </summary>
                case ItemEffect.ItIncreasesTheRangeOfChoicesThatYouCanChooseGuildMarks4:
                    break;
                /// <summary>
                /// ギルドマークを選ぶことができる選択の幅を増やしてくれる。
                /// </summary>
                case ItemEffect.ItIncreasesTheRangeOfChoicesThatYouCanChooseGuildMarks5:
                    break;
                /// <summary>
                /// ギルドマークを選ぶことができる選択の幅を増やしてくれる。
                /// </summary>
                case ItemEffect.ItIncreasesTheRangeOfChoicesThatYouCanChooseGuildMarks6:
                    break;
                /// <summary>
                /// ギルドマークを選ぶことができる選択の幅を増やしてくれる。
                /// </summary>
                case ItemEffect.ItIncreasesTheRangeOfChoicesThatYouCanChooseGuildMarks7:
                    break;
                /// <summary>
                /// ギルドマークを選ぶことができる選択の幅を増やしてくれる。
                /// </summary>
                case ItemEffect.ItIncreasesTheRangeOfChoicesThatYouCanChooseGuildMarks8:
                    break;
                /// <summary>
                /// 露店商の立て札の内容の色を選択できるようにしてくれる。
                /// </summary>
                case ItemEffect.ItMakesItPossibleToSelectTheColorOfTheContentOfTheStandingVendorStandingTenders:
                    break;
                /// <summary>
                /// 露店商の立て札の内容を太い字で書くことができるようにしてくれる。
                /// </summary>
                case ItemEffect.ItMakesItPossibleToWriteTheContentOfTheStandingVendorsStandUpTenderInBoldLetters:
                    break;
                /// <summary>
                /// 露店商の名前をより長く 書くことができるようにしてくれる。
                /// </summary>
                case ItemEffect.ItMakesItPossibleToWriteTheNameOfTheStallVendorLonger:
                    break;
                /// <summary>
                /// 露店商の立て札の周りにきらめく効果をくれる。
                /// </summary>
                case ItemEffect.ItGivesASparklingEffectAroundStandingVendorsStandingTenders:
                    break;
                /// <summary>
                /// 露店商の関連機能をすべて可能にしてくれる。(アシスタント除外)
                /// </summary>
                case ItemEffect.ItAllowsAllRelatedFunctionsOfStallVendors_ExcludeAssistant_:
                    break;
                /// <summary>
                /// 頼もしい立て札
                /// </summary>
                case ItemEffect.ConfidentLetterpress:
                    break;
                /// <summary>
                /// 派手な立て札
                /// </summary>
                case ItemEffect.FlashyStandingBug:
                    break;
                /// <summary>
                /// 不思議な立て札
                /// </summary>
                case ItemEffect.MysteriousStandUpBill:
                    break;
                /// <summary>
                /// きれいな立て札
                /// </summary>
                case ItemEffect.ACleanStandUpBadge:
                    break;
                /// <summary>
                /// 情熱的な立て札
                /// </summary>
                case ItemEffect.PassionateStandUpNotes:
                    break;
                /// <summary>
                /// 拡声器
                /// </summary>
                case ItemEffect.Loudspeaker:
                    break;
                /// <summary>
                /// 魔力補充キット
                /// </summary>
                case ItemEffect.MagicalSupplementKit:
                    break;
                /// <summary>
                /// デラックスアイテムの耐久度を[0]％上昇させる。
                /// </summary>
                case ItemEffect.IncreaseTheDurabilityOfDeluxeItemsByAnyPercentage:
                    break;
            }
        }

        /// <summary>
        /// アイテムの機能
        /// </summary>
        public void ItemFunction(ItemEffect m_value)
        {
            switch (m_value)
            {
                /// <summary>
                /// 宝箱の鍵 レベル [0]
                /// </summary>
                case ItemEffect.TreasureBoxKeyLevelAny:
                    break;
                /// <summary>
                /// 門の鍵 レベル [0]
                /// </summary>
                case ItemEffect.GateKeyLevelAny:
                    break;
                /// <summary>
                /// 万能鍵 レベル [0]
                /// </summary>
                case ItemEffect.UniversalKeyLevelAny:
                    break;
                /// <summary>
                /// 街へ帰還する
                /// </summary>
                case ItemEffect.ReturnToTheCity:
                    break;
                /// <summary>
                /// 街往復ポータル
                /// </summary>
                case ItemEffect.RoundtripStreetPortal:
                    break;
                /// <summary>
                /// スキルポイント 再配分
                /// </summary>
                case ItemEffect.SkillPointRedistribution:
                    break;
                /// <summary>
                /// ステータスポイント 再配分
                /// </summary>
                case ItemEffect.RedistributionOfStatusPoints:
                    break;
                /// <summary>
                /// ダメージ　オプション変換
                /// </summary>
                case ItemEffect.DamageOptionConversion:
                    break;
                /// <summary>
                /// 危険なダメージ  オプション変換
                /// </summary>
                case ItemEffect.DangerousDamageOptionConversion:
                    break;
                /// <summary>
                /// 場所スロット [0] で覚えている場所にテレポートする。
                /// </summary>
                case ItemEffect.TeleportToTheLocationYouRememberInPlaceSlotAny:
                    break;
                /// <summary>
                /// 魔法のカーペットを召喚する。
                /// </summary>
                case ItemEffect.SummonAMagicalCarpet:
                    break;
                /// <summary>
                /// 持っていると魔法のカーペットの模様が変わる。
                /// </summary>
                case ItemEffect.TheMagicCarpetPatternWillChangeWhenYouHaveIt:
                    break;
                /// <summary>
                /// ○× クイズチケット
                /// </summary>
                case ItemEffect.OXQuizTicket:
                    break;
                /// <summary>
                /// HP回復機能のあるタートル絨毯(カーペット)を召喚する。
                /// </summary>
                case ItemEffect.SummonATurtleCarpet_carpet_WithHPRecoveryFunction:
                    break;
                /// <summary>
                /// 移動してもCPが減少しないリスの絨毯(カーペット)を召喚する。
                /// </summary>
                case ItemEffect.SummonACarpetOfSquirrelThatDoesNotDecreaseCPEvenIfItMoves:
                    break;
                /// <summary>
                /// 魔法絨毯(カーペット)よりも移動速度が早いドレイク絨毯(カーペット)を召喚する。
                /// </summary>
                case ItemEffect.SummonADrakeCarpet_carpet_WhichMovesFasterThanAMagicCarpet_carpet_:
                    break;
                /// <summary>
                /// 乗ったままで攻撃可能なギア絨毯(カーペット)を召喚する。
                /// </summary>
                case ItemEffect.SummonAGearCarpet_carpet_ThatCanBeAttackedWhileRiding:
                    break;
                /// <summary>
                /// アイテムのかけら
                /// </summary>
                case ItemEffect.FragmentOfItem:
                    break;
                /// <summary>
                /// 秘密ダンジョン鍵
                /// </summary>
                case ItemEffect.SecretDungeonKey:
                    break;
                /// <summary>
                /// ワールドマップへフィールド移動
                /// </summary>
                case ItemEffect.FieldMoveToWorldMap:
                    break;
            }
        }

        /// <summary>
        /// 残り
        /// </summary>
        public void ItemFunction_After(ItemEffect m_value)
        {
            switch (m_value)
            {
                /// <summary>
                /// 宝物地図
                /// </summary>
                case ItemEffect.TreasureMap:
                    break;
                /// <summary>
                /// ギルドスキルポイント再分配(石像スキル除外)
                /// </summary>
                case ItemEffect.GuildSkillPointRedistribution_stoneSkillExclusion_:
                    break;
                /// <summary>
                /// ウェイポイントスロット[0]に記憶している場所に通じるポータルを開く。
                /// </summary>
                case ItemEffect.OpenAPortalThatLeadsToTheLocationStoredInWaypointSlotAny:
                    break;
                /// <summary>
                /// カップルリング２つを生成する。
                /// </summary>
                case ItemEffect.CoupleRingsToProduceTwo:
                    break;
                /// <summary>
                /// カップルリング
                /// </summary>
                case ItemEffect.CoupleRing:
                    break;
                /// <summary>
                /// 同じ種類のアイテム２つを交ぜる。
                /// </summary>
                case ItemEffect.MixTwoItemsOfTheSameType:
                    break;
                /// <summary>
                /// 花火
                /// </summary>
                case ItemEffect.fireworks:
                    break;
                /// <summary>
                /// どのような花火が出るか、使う前にはわからない。
                /// </summary>
                case ItemEffect.IDoNotKnowWhatKindOfFireworksWillComeOutBeforeUsingIt:
                    break;
                /// <summary>
                /// 選択ステータス増加
                /// </summary>
                case ItemEffect.IncreaseSelectionStatus:
                    break;
                /// <summary>
                /// プレゼント用カップルリング
                /// </summary>
                case ItemEffect.CoupleRingForGifts:
                    break;
                /// <summary>
                /// パートナー無しカップルリング
                /// </summary>
                case ItemEffect.NoPartnerCoupleRing:
                    break;
                /// <summary>
                /// 本にカテゴリー追加
                /// </summary>
                case ItemEffect.AddCategoryToBook:
                    break;
                /// <summary>
                /// 本にページ追加
                /// </summary>
                case ItemEffect.AddPageToBook:
                    break;
                /// <summary>
                /// 本
                /// </summary>
                case ItemEffect.Book:
                    break;
                /// <summary>
                /// プリンセス変身時の武器
                /// </summary>
                case ItemEffect.WeaponsWhenPrincessTransformation:
                    break;
                /// <summary>
                /// ギルドホール移動アイテム
                /// </summary>
                case ItemEffect.GuildhallMoveItem:
                    break;
                /// <summary>
                /// ギルドガーディアン召喚
                /// </summary>
                case ItemEffect.SummonGuildGuardian:
                    break;
                /// <summary>
                /// ギルド石像関連アイテム
                /// </summary>
                case ItemEffect.GuildStoneStatuesRelatedItems:
                    break;
                /// <summary>
                /// ギルド石像関連アイテム
                /// </summary>
                case ItemEffect.GuildStoneStatuesRelatedItems2:
                    break;
                /// <summary>
                /// 運営者に変身することができる。
                /// </summary>
                case ItemEffect.YouCanTransformIntoAnOperator:
                    break;
                /// <summary>
                /// 最終ダメージが[0]分間[1]0％増加する。
                /// </summary>
                case ItemEffect.TheFinalDamageIncreasesByAny0PercentAnyMinute:
                    break;
                /// <summary>
                /// モンスターを倒す時に獲得する経験値が[0]分間[1]0％増加する。
                /// </summary>
                case ItemEffect.TheExperienceValueYouEarnWhenYouDefeatAMonsterIncreasesAnyAnyPercentageAnyMinute:
                    break;
                /// <summary>
                /// アイテムドロップ確率が[0]分間[1]0％増加する。
                /// </summary>
                case ItemEffect.TheItemDropProbabilityIncreasesAnyAnyPercentForAnyMinute:
                    break;
                /// <summary>
                /// 死亡ペナルティーをすべて回復させる。
                /// </summary>
                case ItemEffect.RecoverAllDeathPenalty:
                    break;
                /// <summary>
                /// 移動中も増加する。
                /// </summary>
                case ItemEffect.ItAlsoIncreasesDuringTravel:
                    break;
                /// <summary>
                /// イベント用風船
                /// </summary>
                case ItemEffect.EventBalloons:
                    break;
                /// <summary>
                /// 神秘的なアイテムボックス
                /// </summary>
                case ItemEffect.MysteriousItemBox2:
                    break;
                /// <summary>
                /// 最終ダメージ[+1]％[0]分
                /// </summary>
                case ItemEffect.LastDamagePlusAnyPercentAnyMinutes:
                    break;
                /// <summary>
                /// ミニペット召喚
                /// </summary>
                case ItemEffect.MiniPetSummon:
                    break;
                /// <summary>
                /// ミニペット期間延長
                /// </summary>
                case ItemEffect.ExtendedMiniPetTerm:
                    break;
                /// <summary>
                /// 全ミニペット期間延長
                /// </summary>
                case ItemEffect.ExtendedMiniPetPeriod:
                    break;
                /// <summary>
                /// ミニペット封印
                /// </summary>
                case ItemEffect.MiniPetSeal:
                    break;
                /// <summary>
                /// ミニペット名前変更
                /// </summary>
                case ItemEffect.MiniPetNameChange:
                    break;
                /// <summary>
                /// ミニペットが入った袋
                /// </summary>
                case ItemEffect.BagContainingMiniPet:
                    break;
                /// <summary>
                /// ミニペットのエサ
                /// </summary>
                case ItemEffect.MinipetFood:
                    break;
                /// <summary>
                /// ミニペットのエサ
                /// </summary>
                case ItemEffect.MinipetFood2:
                    break;
                /// <summary>
                /// ミニペットのエサ
                /// </summary>
                case ItemEffect.MinipetFood3:
                    break;
                /// <summary>
                /// ミニペットを2体同時に活性化できるようになる- 期間 [0]日
                /// </summary>
                case ItemEffect.YouWillBeAbleToActivate2MiniPetsAtTheSameTimeAndPeriodAnyDay:
                    break;
                /// <summary>
                /// 称号レベル上昇
                /// </summary>
                case ItemEffect.TitleLevelRise:
                    break;
                /// <summary>
                /// 取引不可アイテムを取引可能に変更
                /// </summary>
                case ItemEffect.ChangeDealNoDealItemToBeTradable:
                    break;
                /// <summary>
                /// ギルドスキルポイント再分配(石像スキル除外)
                /// </summary>
                case ItemEffect.GuildSkillPointRedistribution_stoneSkillExclusion_2:
                    break;
                /// <summary>
                /// 自分のレベルより[0]レベル高いレベル制限アイテムの使用が可能
                /// </summary>
                case ItemEffect.YouCanUseLevelRestrictedItemsThatAreAnyLevelHigherThanYourOwnLevel:
                    break;
                /// <summary>
                /// スキル１つ初期化
                /// </summary>
                case ItemEffect.InitializeOneSkill:
                    break;
                /// <summary>
                /// ステータス１つ初期化
                /// </summary>
                case ItemEffect.InitializeOneStatus:
                    break;
                /// <summary>
                /// 称号除去
                /// </summary>
                case ItemEffect.TitleRemoval:
                    break;
                /// <summary>
                /// アイテム複製
                /// </summary>
                case ItemEffect.ItemDuplication:
                    break;
                /// <summary>
                /// 一般アイテム称号アップグレード
                /// </summary>
                case ItemEffect.GeneralItemTitleUpgrade:
                    break;
                /// <summary>
                /// 一般アイテム性能向上
                /// </summary>
                case ItemEffect.GeneralItemPerformanceImprovement:
                    break;
                /// <summary>
                /// ユニークアイテム称号アップグレード
                /// </summary>
                case ItemEffect.UniqueItemTitleUpgrade:
                    break;
                /// <summary>
                /// ユニークアイテム性能向上
                /// </summary>
                case ItemEffect.UniqueItemPerformanceImprovement:
                    break;
                /// <summary>
                /// 同じ種類のアイテム２つを交ぜる
                /// </summary>
                case ItemEffect.MixTwoItemsOfTheSameType2:
                    break;
                /// <summary>
                /// 性向数値選択
                /// </summary>
                case ItemEffect.SelectionOfTendencyNumber:
                    break;
                /// <summary>
                /// タイムスタンプ
                /// </summary>
                case ItemEffect.TimeStamp:
                    break;
                /// <summary>
                /// 称号追加
                /// </summary>
                case ItemEffect.TitleAdded:
                    break;
                /// <summary>
                /// 狩りでの経験値ボーナス
                /// </summary>
                case ItemEffect.ExperienceBonusOnHunting:
                    break;
                /// <summary>
                /// バッジに刻印する
                /// </summary>
                case ItemEffect.EngraveOnABadge:
                    break;
                /// <summary>
                /// 生命石
                /// </summary>
                case ItemEffect.LifeStone:
                    break;
                /// <summary>
                /// 魔法絨毯(カーペット)よりも移動速度が早いファイヤードレイク絨毯(カーペット)を召喚する。
                /// </summary>
                case ItemEffect.SummonAFiredrakeCarpet_carpet_ThatMovesFasterThanAMagicCarpet_carpet_:
                    break;
                /// <summary>
                /// 火炎の激怒
                /// </summary>
                case ItemEffect.FuriousRage:
                    break;
                /// <summary>
                /// 水の波動
                /// </summary>
                case ItemEffect.WaterWave:
                    break;
                /// <summary>
                /// 風の加護
                /// </summary>
                case ItemEffect.WindProtection:
                    break;
                /// <summary>
                /// 大地の響き
                /// </summary>
                case ItemEffect.EchoesOfTheEarth:
                    break;
                /// <summary>
                /// ルーンを刻印する
                /// </summary>
                case ItemEffect.EngraveRune:
                    break;
                /// <summary>
                /// 覚醒の種
                /// </summary>
                case ItemEffect.AwakeningSeed:
                    break;
                /// <summary>
                /// 覚醒の果実
                /// </summary>
                case ItemEffect.AwakedFruits:
                    break;
                /// <summary>
                /// ギルドマーク・シグナルパッケージ
                /// </summary>
                case ItemEffect.GuildMarkSignalPackage:
                    break;
                /// <summary>
                /// ギルドマーク・ユニーク１パッケージ
                /// </summary>
                case ItemEffect.GuildMarkEUnique1Package:
                    break;
                /// <summary>
                /// ギルドマーク・ユニーク２パッケージ
                /// </summary>
                case ItemEffect.GuildMarkEUnique2Package:
                    break;
                /// <summary>
                /// HPを [0]00ポイント回復
                /// </summary>
                case ItemEffect.Any00PointRecoveryHP:
                    break;
                /// <summary>
                /// CPを [0]00ポイント充填
                /// </summary>
                case ItemEffect.FillAnyCPForCP:
                    break;
                /// <summary>
                /// [*]
                /// </summary>
                case ItemEffect.INVALID:
                    break;
                /// <summary>
                /// ステータスポイント 力 再配分
                /// </summary>
                case ItemEffect.StatusPointPowerRedistribution:
                    break;
                /// <summary>
                /// ステータスポイント 敏捷 再配分
                /// </summary>
                case ItemEffect.StatusPointAgilityRedistribution:
                    break;
                /// <summary>
                /// ステータスポイント 健康 再配分
                /// </summary>
                case ItemEffect.StatusPointHealthRedistribution:
                    break;
                /// <summary>
                /// ステータスポイント 知識 再配分
                /// </summary>
                case ItemEffect.StatusPointKnowledgeRedistribution:
                    break;
                /// <summary>
                /// ステータスポイント 知恵 再配分
                /// </summary>
                case ItemEffect.StatusPointWisdomRedistribution:
                    break;
                /// <summary>
                /// ステータスポイント カリスマ 再配分
                /// </summary>
                case ItemEffect.StatusPointCharismaRedistribution:
                    break;
                /// <summary>
                /// ステータスポイント 運 再配分
                /// </summary>
                case ItemEffect.StatusPointLuckAllocation:
                    break;
                /// <summary>
                /// ワールドマップ上のフィールドを自由に移動できる。
                /// </summary>
                case ItemEffect.YouCanMoveTheFieldOnTheWorldMapFreely:
                    break;
                /// <summary>
                /// 戦闘不能になったパーティーメンバーを全員復活させて[0]秒間最大HP、CPを[1]％増加させる。
                /// </summary>
                case ItemEffect.ReviveAllPartyMembersWhoBecameUnableToFightAndIncreaseAnyMaximumHPAndCPAnyTimeForAnySeconds:
                    break;
                /// <summary>
                /// 一日の秘密ダンジョン入場制限回数を[0]回増加させる。
                /// </summary>
                case ItemEffect.IncreaseTheSecretDungeonAdmissionLimitNumberOfTheDayAnyTime:
                    break;
                /// <summary>
                /// ミニペットがエサを食べて獲得する際の経験値を[0]ポイント増加させる。
                /// </summary>
                case ItemEffect.IncreaseTheExperiencePointWhenMiniPetsEatAndFeedAnyPoint:
                    break;
                /// <summary>
                /// HP [0]％すぐに回復
                /// </summary>
                case ItemEffect.HPAnyPercentSoonRecovery:
                    break;
                /// <summary>
                /// HP [0]00ポイントすぐに回復
                /// </summary>
                case ItemEffect.HPAny00PointRecoverySoon:
                    break;
                /// <summary>
                /// 一日のギルドダンジョン入場制限回数を[0]回増加させる。
                /// </summary>
                case ItemEffect.IncreaseTheNumberOfAdmissionTimesOfGuildDungeonOfADayAnyTime:
                    break;
                /// <summary>
                /// ペットの名前をno nameに戻し、名前を変更することができる。
                /// </summary>
                case ItemEffect.YouCanRenameYourPetsNameBackToNoName:
                    break;
                /// <summary>
                /// [0]時間の間、すべてのプレイヤーの経験値とアイテムドロップ率を[1]ポイント増加する。
                /// </summary>
                case ItemEffect.DuringAnyTime_IncreaseTheExperiencePointAndItemDropRateOfAllPlayersAnyPoint:
                    break;
                /// <summary>
                /// ミニペットを調和させる。
                /// </summary>
                case ItemEffect.HarmonizeMiniPets:
                    break;
                /// <summary>
                /// ミニペットのオーラを取り込む
                /// </summary>
                case ItemEffect.CaptureMiniPetAura:
                    break;
                /// <summary>
                /// [0]0レベル以下のアイテム 1つを能力に関係なく使用可能にする。
                /// </summary>
                case ItemEffect.MakeOneItemBelowAny0LevelAvailableRegardlessOfAbility:
                    break;
                /// <summary>
                /// [0]％の確率でアイテムの称号を付ける
                /// </summary>
                case ItemEffect.TitleTheItemWithTheProbabilityOfAnyPercent:
                    break;
                /// <summary>
                /// [0]％の確率でアイテムの称号をコピーする
                /// </summary>
                case ItemEffect.CopyTheTitleOfTheItemWithTheProbabilityOfAnyPercent:
                    break;
                /// <summary>
                /// 選択したアイテムの称号を削除する
                /// </summary>
                case ItemEffect.DeleteTheTitleOfTheSelectedItem:
                    break;
                /// <summary>
                /// 残り体力をすぐに減少 [0]％
                /// </summary>
                case ItemEffect.ImmediateDecreaseInPhysicalStrengthRemainingAnyPercent:
                    break;
                /// <summary>
                /// [0]％の確率でアイテムの称号を付ける【 取引不可 】
                /// </summary>
                case ItemEffect.TitleTheItemWithTheProbabilityOfAnyPercent_NoTransactions_:
                    break;
                /// <summary>
                /// [0]％の確率でアイテムの称号をコピーする【 取引不可 】
                /// </summary>
                case ItemEffect.CopyTheTitleOfTheItemWithTheProbabilityOfAnyPercent_NoTransactions_:
                    break;
                /// <summary>
                /// 選択したアイテムの称号を削除する
                /// </summary>
                case ItemEffect.DeleteTheTitleOfTheSelectedItem2:
                    break;
                /// <summary>
                /// イベント用風船
                /// </summary>
                case ItemEffect.EventBalloons2:
                    break;
                /// <summary>
                /// 選択したステータス [+0]増加
                /// </summary>
                case ItemEffect.SelectedStatusPlusAnyIncrease:
                    break;
                /// <summary>
                /// [0]分間 レベル[1]の狩人に変身
                /// </summary>
                case ItemEffect.MakeAHangerOfAnyAnyLevelAHunter:
                    break;
                /// <summary>
                /// [0]分間 レベル[1]のアサシンに変身
                /// </summary>
                case ItemEffect.TransformedToAssassinOfAnyAnyLevelOfAny:
                    break;
                /// <summary>
                /// [0]分間 レベル[1]のブラックメイジに変身
                /// </summary>
                case ItemEffect.MakeItAAnyMinuteLevelBlackMage:
                    break;
                /// <summary>
                /// 現在、フィールドにいるモンスターを召喚する。
                /// </summary>
                case ItemEffect.CurrentlySummonMonstersInTheField:
                    break;
                /// <summary>
                /// 刻印レベル[0]
                /// </summary>
                case ItemEffect.MarkingLevelAny:
                    break;
                /// <summary>
                /// [0]％の確率で刻印レベルを 1～[1]増加
                /// </summary>
                case ItemEffect.IncreaseTheStampingLevel1ToAnyWithTheProbabilityOfAnyPercent:
                    break;
                /// <summary>
                /// [0]％の確率で刻印レベルを[1]増加
                /// </summary>
                case ItemEffect.AnyIncrementOfStampingLevelWithAnyPercentProbability:
                    break;
                /// <summary>
                /// [0]秒ごとにCPを [0]回復
                /// </summary>
                case ItemEffect.CPAnyRecoveryEverySecond:
                    break;
                /// <summary>
                /// [0]秒間 すべてのパーティーメンバー無敵状態
                /// </summary>
                case ItemEffect.AnyPartyInvincibleStateForAllPartyMembers:
                    break;
                /// <summary>
                /// レベル[0]のファミリアに [0]分間 変身
                /// </summary>
                case ItemEffect.TransformAnyMinuteIntoLevelFamilyAnyFamily:
                    break;
                /// <summary>
                /// 使用時、[0]分間 攻撃力 [0]％増加
                /// </summary>
                case ItemEffect.AtUse_AnyMinutesIncreasesAttackPowerAnyPercentage:
                    break;
                /// <summary>
                /// レベル[0]のヘルプリズン使用
                /// </summary>
                case ItemEffect.UseOfAnyLevelOfHERPRUZEN:
                    break;
                /// <summary>
                /// ミニペットのエサ
                /// </summary>
                case ItemEffect.MinipetFood4:
                    break;
                /// <summary>
                /// ミニペットのスキルポイント 再配分
                /// </summary>
                case ItemEffect.RedistributionOfMiniPetSkillPoints:
                    break;
                /// <summary>
                /// ミニペットの進化形態変化
                /// </summary>
                case ItemEffect.ChangesInEvolutionaryFormOfMinipets:
                    break;
                /// <summary>
                /// 残り体力を [(0*1)] ポイントに変更
                /// </summary>
                case ItemEffect.ChangeRemainingPhysicalStrengthToAnyTimesAnyPoint:
                    break;
                /// <summary>
                /// フィールド移動
                /// </summary>
                case ItemEffect.FieldMove:
                    break;
            }
        }
    }
    
}
