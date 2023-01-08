using RedStoneLib;
using RedStoneLib.Model;
using RedStoneLib.Model.Base;
using RedStoneLib.Model.Effect;
using RedStoneLib.Packets;
using RedStoneLib.Packets.RSPacket.Action;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using static RedStoneLib.Model.Skill;
using System.Threading.Tasks;

namespace RedStoneEmu.Games
{
    /// <summary>
    /// 攻撃・回復・Buff・Defence
    /// ※コーディング量の見通しがつかないので，増えてきたら上記4つを個別に実装
    /// </summary>
    static class Action
    {
        /// <summary>
        /// スキル使用
        /// </summary>
        /// <param name="sendPacket"></param>
        /// <param name="target"></param>
        /// <param name="skill"></param>
        /// <param name="slv"></param>
        public static void UseSkill(Client context, Actor target, Skill skill, int slv)
        {
            if (context.User.IsButtleNow) return;
            Task.Run(() =>
            {
                context.User.IsButtleNow = true;
                while (context.User.NowHP > 0 &&
                AttackToActor(skill, context.SendPacket, out double totalDamage, slv, context.User, target) &&
                context.User.IsButtleNow) ;
            }).ContinueWith(_ =>
            {
                context.User.IsButtleNow = false;
            });
        }

        /// <summary>
        /// 敵に対して使用
        /// </summary>
        /// <param name="otherPackets"></param>
        /// <param name="slv"></param>
        /// <param name="attacker"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool AttackToActor(Skill skill, SendPacketDelegate sendPacket, out double totalDamage, 
            int slv, Actor attacker, Actor target, bool isMaxDamage = false)
        {
            //チェック
            if (!skill.UsageFlag.HasFlag(SkillUsageFlag.CastEnemy) ||
            !skill.TargetRaceFlag.HasFlag((SkillTargetRace)((1 << (int)target.Race) >> 1)))
            {
                totalDamage = 0;
                return false;
            }

            MAPServer map = MAPServer.AllMapServers[target.MapSerial];

            //物理ダメ
            bool hasPhysicDamage = TryGetPhysicDamageToActor(skill, out double physicDamage, out bool isAvoid, slv, attacker, target, isMaxDamage);

            bool InternalTryGetMagicDamage(out Magic<double> mdamage)
            {
                //魔法の必要性なし
                if (hasPhysicDamage && isAvoid)
                {
                    mdamage = new Magic<double>();
                    return skill.HasSomeMagicDamage;
                }
                return TryGetMagicDamage(skill, out mdamage, slv, attacker, target, hasPhysicDamage, isMaxDamage);
            }

            //魔法ダメ
            bool hasMagicDamage = InternalTryGetMagicDamage(out var magicDamage);

            //攻撃パケット
            var attackPacket = MakePacket(skill, attacker, target, (int)Math.Round(physicDamage), (Magic)magicDamage, isAvoid);
            if (attackPacket != null)
                map.SendPacketsToPlayersOnTheMap(attackPacket, isNear: true, flush: true);

            //前間待機
            double waitTime = attacker.GetAttackSpeedWaitTime(skill, slv);
            Thread.Sleep((int)(waitTime / 3.0));

            totalDamage = physicDamage + magicDamage.SumDouble;
            
            if (totalDamage > target.NowHP)
            {
                //倒した
                target.NowHP = 0;
                map.SendPacketsToPlayersOnTheMap(new KillPacket(target.CharID, attacker.CharID), flush:true);
                map.ReserveDeadActor(target.CharID);

                //EXP
                if (target is Monster monster && attacker is Player player)
                {
                    //EXP手続き
                    StatusController.EXPProcedure(ref player, monster, sendPacket, pkt => map.SendPacketsToPlayersOnTheMap(pkt, isNear: true));

                    //アイテムドロップ
                    DropItem[] dropItems = DropItem.Rottely(player, monster);
                    //登録
                    MAPServer.AllMapServers[player.MapSerial].ReseiveDropItems(dropItems);
                }

            }
            else
            {
                //HP減少
                target.NowHP -= (uint)Math.Floor(totalDamage);
                map.SendPacketsToPlayersOnTheMap(new RemainHPPacket(target.CharID, target.NowHP, target.MaxHP), flush:true);
            }

            //後待機
            Thread.Sleep((int)(waitTime * 2.0 / 3.0));

            //生きてたら継続
            return target.NowHP != 0;
        }

        /// <summary>
        /// スキルをプレイヤーに対して試行
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="slv"></param>
        /// <param name="me"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool TryGetPacketToUsePlayer(Skill skill, out Packet packet, int slv, Actor me, Actor target)
        {
            //チェック
            if (!skill.UsageFlag.HasFlag(SkillUsageFlag.CastEnemy) || target is Player == false)
            {
                packet = null;
                return false;
            }

            packet = MakePacket(skill, me, target);
            return true;
        }

        /// <summary>
        /// 物理ダメージの計算を試行
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="damage"></param>
        /// <param name="isAvoid"></param>
        /// <param name="slv"></param>
        /// <param name="attacker"></param>
        /// <param name="target"></param>
        /// <param name="isMaxDamage"></param>
        /// <returns></returns>
        public static bool TryGetPhysicDamageToActor(Skill skill, out double damage, out bool isAvoid, int slv, Actor attacker, Actor target, bool isMaxDamage = false, bool notAvoid = false)
        {
            //チェック
            if (!skill.DamageFlag.HasFlag(SkillDamageFlag.Physic))
            {
                damage = 0;
                isAvoid = false;
                return false;
            }

            //回避チェック
            if (notAvoid)
            {
                double avoidanceRate = skill.GetTargetAvoidance(attacker, target);
                isAvoid = Helper.Lottery(avoidanceRate);
                if (isAvoid)
                {
                    damage = 0;
                    return true;
                }
            }
            else
            {
                isAvoid = false;
            }

            var baseAttackPowerObject = attacker.Attack(skill.DamagePercent[slv], 0);

            //基本攻撃力
            double baseAttackPower = isMaxDamage ? baseAttackPowerObject.Max : baseAttackPowerObject.RandomValue;
            double baseTargetDefence = target.Defence;

            //基本ダメージ
            double baseDamage = baseAttackPower * baseAttackPower / (baseAttackPower + baseTargetDefence);

            //種族ダメージ
            int raceDamage = (attacker as Player)?.Effect.DamageOfRace[(int)target.Race] ?? 0;

            //最終ダメージ
            damage = baseDamage * (100.0 + raceDamage) / 100.0;
            return true;
        }

        /// <summary>
        /// 知識ダメージの計算を試行（！最終ダメージ計算なし）
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="slv"></param>
        /// <param name="attacker"></param>
        /// <param name="target"></param>
        /// <param name="hasPhysicDamage"></param>
        /// <param name="isMaxDamage"></param>
        /// <returns></returns>
        public static bool TryGetMagicDamage(Skill skill, out Magic<double> damage, int slv, Actor attacker, Actor target, bool hasPhysicDamage, bool isMaxDamage = false)
        {
            //全属性攻撃存在チェック
            if (!skill.HasSomeMagicDamage)
            {
                damage = default(Magic<double>);
                return false;
            }

            //エフェクト
            PlayerEffect ate = (attacker as Player)?.Effect ?? null;
            PlayerEffect tae = (target as Player)?.Effect ?? null;

            //知識の項（物理ダメがあると知識ボーナスなし）
            double knowledgeBonus = hasPhysicDamage ? 1 : attacker.Status.GetKnowledgeBonus();
            double knowledgeTerm = (1.0 + (knowledgeBonus * attacker.Status.Inteligence / 200.0));

            //魔法属性ごとのダメージを取得
            double GetDamageByMagicAttribute(MagicType index)
            {
                //属性攻撃存在チェック
                if (((1 << ((int)index + 2)) & (int)skill.DamageFlag) == 0) return 0.0;

                //ダメージ取得
                var damageObject = skill.SkillInteligenceDamages[(int)index];
                double attrDamage = isMaxDamage ? damageObject.Max(slv) : damageObject.Random(slv);

                //範囲処理
                if (!skill.RangeDecreaseCoef.ExpressionUnavailable)
                {
                    //減少率（割合）
                    double decRate = 1.0 / skill.RangeDecreaseCoef[(int)Math.Round(attacker.DistanceTo(target))];
                    //制限
                    attrDamage *= (skill.RangeDamageAmpRateScale / 100.0).Clip(decRate);
                }

                return hasPhysicDamage ? attrDamage * knowledgeTerm : attrDamage * knowledgeTerm
                    * (100.0 + ate?.IncreaseMagicAttackPower[index] ?? 0) / 100.0//魔法強化
                    * (100.0 - target.MagicResistance[index] + ate?.WeakenMagicResistance[index] ?? 0) / 100.0//魔法弱化
                    * (100.0 + tae?.MagicDamageAbsorption[index] ?? 0) / 100.0;//属性吸収
            }

            damage = new Magic<double>
            {
                Fire = GetDamageByMagicAttribute(MagicType.Fire),
                Water = GetDamageByMagicAttribute(MagicType.Water),
                Wind = GetDamageByMagicAttribute(MagicType.Wind),
                Earth = GetDamageByMagicAttribute(MagicType.Earth),
                Light = GetDamageByMagicAttribute(MagicType.Light),
                Dark = GetDamageByMagicAttribute(MagicType.Dark)
            };
            return true;
        }

        private static Packet MakePacket(Skill m_value, Actor attacker, Actor target, int physicDamage = 0, Magic magicDamage = default(Magic), bool isMiss = false)
        {
            switch (m_value.MakePacketType)
            {
                //通常攻撃
                case SkillMakePacketType.BasicAttack:
                case SkillMakePacketType.BasicFarAttack:
                    return new BasicAttackPacket(attacker.Pos, attacker.CharID, target.CharID, m_value.Index, physicDamage, magicDamage, isMiss);
                //連続打撃攻撃
                case SkillMakePacketType.ContinuousHitAttack:
                    return null;
                //ラッシュ攻撃
                case SkillMakePacketType.Rush:
                    return null;
                //ディレイ攻撃
                case SkillMakePacketType.Delay:
                    return null;
                //分身攻撃
                case SkillMakePacketType.BunshineAttack:
                    return null;
                //シミター
                case SkillMakePacketType.ScimitarCutting:
                    return null;
                //タンクラッシュ
                case SkillMakePacketType.TankRush:
                    return null;
                //ジャンプ攻撃
                case SkillMakePacketType.JumpAttack:
                    return null;
                //ワイルドスタンプ
                case SkillMakePacketType.WildStamp:
                    return null;
                //オルターリングヒッター
                case SkillMakePacketType.AlternatorRingHitter:
                    return null;
                //サイクロンピーク
                case SkillMakePacketType.CyclonePeak:
                    return null;
                //ワイルドダンス
                case SkillMakePacketType.WildDance:
                    return null;
                //旋風突き
                case SkillMakePacketType.Whirlwashing:
                    return null;
                //サプライジングレイド
                case SkillMakePacketType.SupplyJinGlade:
                    return null;
                //ガーディアンポスト
                case SkillMakePacketType.GuardianPost:
                    return null;
                //オーサムフォートレス
                case SkillMakePacketType.AutumnFortress:
                    return null;
                //バイトハンギング
                case SkillMakePacketType.ByteHanging:
                    return null;
                //バウンシングリニア
                case SkillMakePacketType.BouncingLinear:
                    return null;
                //ファイアーボール
                case SkillMakePacketType.Fireball:
                    return null;
                //マシーンアロー
                case SkillMakePacketType.MachineArrow:
                    return null;
                //ウォーターキャノン
                case SkillMakePacketType.WaterCannon:
                    return null;
                //ピアシングアロー
                case SkillMakePacketType.PiercingArrow:
                    return null;
                //スプレッドアロー
                case SkillMakePacketType.SpreadArrow:
                    return null;
                //ダブルスローイング
                case SkillMakePacketType.DoubleThrowing:
                    return null;
                //ブーメランシールド
                case SkillMakePacketType.BoomerangShield:
                    return null;
                //ジャベリンテンペスト
                case SkillMakePacketType.JavelinTempest:
                    return null;
                //ソニックブロー
                case SkillMakePacketType.SonicBlow:
                    return null;
                //ストレートスパイク
                case SkillMakePacketType.StraightSpike:
                    return null;
                //ランドマーカー
                case SkillMakePacketType.LandMarker:
                    return null;
                //インターバルシューター
                case SkillMakePacketType.IntervalShooter:
                    return null;
                //グライディングファイアー
                case SkillMakePacketType.GlidingFire:
                    return null;
                //トワーリングプロテクター
                case SkillMakePacketType.TowerRingProtector:
                    return null;
                //ヘブンリープレシング
                case SkillMakePacketType.HeavenlyPrecision:
                    return null;
                //ボイドボウ
                case SkillMakePacketType.BoydBow:
                    return null;
                //テイルチェイサー
                case SkillMakePacketType.TailChaser:
                    return null;
                //スピンアライジング
                case SkillMakePacketType.SpinAligning:
                    return null;
                //デュエリング
                case SkillMakePacketType.Dueling:
                    return null;
                //ダブルターゲット
                case SkillMakePacketType.DoubleTarget:
                    return null;
                //ローズガーデン
                case SkillMakePacketType.RoseGarden:
                    return null;
                //花の乙女スペシャル
                case SkillMakePacketType.FlowerMaidSpecial:
                    return null;
                //丸太変身
                case SkillMakePacketType.TransformLog:
                    return null;
                //モンスターバレット
                case SkillMakePacketType.MonsterBarrett:
                    return null;
                //ポーション投げ
                case SkillMakePacketType.PotionThrow:
                    return null;
                //ライトニングワインダー
                case SkillMakePacketType.LightningWinder:
                    return null;
                //ミラーイメージ
                case SkillMakePacketType.MirrorImage:
                    return null;
                //チェーンライトニング
                case SkillMakePacketType.ChainLightning:
                    return null;
                //ファイアーウォール
                case SkillMakePacketType.Firewall:
                    return null;
                //ギガライトニング
                case SkillMakePacketType.GigaLightning:
                    return null;
                //竜巻起こし
                case SkillMakePacketType.ATornadoRaise:
                    return null;
                //ソウルブレイズ
                case SkillMakePacketType.SeoulBlaze:
                    return null;
                //ジャンプ
                case SkillMakePacketType.Jump:
                    return null;
                //ディメンジョンアーマー
                case SkillMakePacketType.DimensionArmor:
                    return null;
                //スパイクシールディング
                case SkillMakePacketType.SpikeShielding:
                    return null;
                //アイススタラグマイト
                case SkillMakePacketType.IceStalagmite:
                    return null;
                //マッスルインフレーション
                case SkillMakePacketType.MuscleInflation:
                    return null;
                //アースヒール
                case SkillMakePacketType.EarthHeel:
                    return null;
                //ダンシングブロッカー
                case SkillMakePacketType.DancingBlocker:
                    return null;
                //防御向上
                case SkillMakePacketType.ImproveDefense:
                    return null;
                //ムーンクライ
                case SkillMakePacketType.MoonCry:
                    return null;
                //グレートガッツ
                case SkillMakePacketType.GreatGuts:
                    return null;
                //回避
                case SkillMakePacketType.Avoidance:
                    return null;
                //アタックインターセプター
                case SkillMakePacketType.AttackInterceptor:
                    return null;
                //テレポーテーション
                case SkillMakePacketType.Teleportation:
                    return null;
                //バフ？
                case SkillMakePacketType.Buff:
                    return null;
                //コンプリートプロテクション
                case SkillMakePacketType.CompleteProtection:
                    return null;
                //シマーリングシールド
                case SkillMakePacketType.ShimmeringRingShield:
                    return null;
                //ミラータワー
                case SkillMakePacketType.MillerTower:
                    return null;
                //攻撃命令
                case SkillMakePacketType.AttackCommand:
                    return null;
                //マウスフル
                case SkillMakePacketType.MouseFull:
                    return null;
                //双連破
                case SkillMakePacketType.Shot:
                    return null;
                //ピューマラッシュ
                case SkillMakePacketType.PumaRush:
                    return null;
                //エクスプロージョン
                case SkillMakePacketType.Explosion:
                    return null;
                //リフレクトライト
                case SkillMakePacketType.ReflectLight:
                    return null;
                //クリムゾン・アイ
                case SkillMakePacketType.CrimsonEye:
                    return null;
            }

            return null;
        }

    }
}
