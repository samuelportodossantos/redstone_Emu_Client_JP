using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static RedStoneLib.Model.Base.Actor;

namespace RedStoneLib.Model.Effect
{
    /// <summary>
    /// 低下・上昇
    /// </summary>
    public class ActorBuff
    {
        int _damage;
        int _defence;
        int _accuracy;
        int _avoidance;
        int _movingSpeed;
        int _attackSpeed;
        Magic _magicResistance;

        /// <summary>
        /// 攻撃力%
        /// </summary>
        public int Damage
        {
            get => _damage;
            set
            {
                _damage = value;
                SwitchAnim3(value > 0, ActorAnim3.PowerPlus, ActorAnim3.PowerMinus);
            }
        }

        /// <summary>
        /// 防御力%
        /// </summary>
        public int Defence
        {
            get => _defence;
            set
            {
                _defence = value;
                SwitchAnim3(value > 0, ActorAnim3.DefencePlus, ActorAnim3.DefenceMinus);
            }
        }

        /// <summary>
        /// 命中率
        /// </summary>
        public int Accuracy
        {
            get => _accuracy;
            set
            {
                _accuracy = value;
                SwitchAnim3(value > 0, ActorAnim3.AccuracyRatePlus, ActorAnim3.AccuracyRateMinus);
            }
        }

        /// <summary>
        /// 回避率
        /// </summary>
        public int Avoidance
        {
            get => _avoidance;
            set
            {
                _avoidance = value;
                SwitchAnim3(value > 0, ActorAnim3.AvoidanceRatePlus, ActorAnim3.AvoidanceRateMinus);
            }
        }

        /// <summary>
        /// 移動速度
        /// </summary>
        public int MovingSpeed
        {
            get => _movingSpeed;
            set
            {
                _movingSpeed = value;
                SwitchAnim3(value > 0, ActorAnim3.MoveSpeedPlus, ActorAnim3.MoveSpeedMinus);
            }
        }

        /// <summary>
        /// 攻撃速度
        /// </summary>
        public int AttackSpeed
        {
            get => _attackSpeed;
            set
            {
                _attackSpeed = value;
                SwitchAnim3(value > 0, ActorAnim3.AttackSpeedPlus, ActorAnim3.AttackSpeedMinus);
            }
        }

        /// <summary>
        /// 属性抵抗
        /// </summary>
        public Magic MagicResistance
        {
            get => _magicResistance;
            set
            {
                _magicResistance = value;
                SwitchAnim3(value.Sum > 0, ActorAnim3.MagicResistancePlus, ActorAnim3.MagicResistanceMinus);
            }
        }

        public delegate void ChangeAnimDelegate(object actorAnim, bool set);

        /// <summary>
        /// 頭上表示変更イベント
        /// </summary>
        public event ChangeAnimDelegate OnChangeAnim;

        /// <summary>
        /// タイマー
        /// </summary>
        private Dictionary<BuffType, bool> IsTimerStarted = new Dictionary<BuffType, bool>();

        /// <summary>
        /// 頭上表示ステータスのスイッチ
        /// </summary>
        /// <param name="isOn"></param>
        /// <param name="plusAnim"></param>
        /// <param name="minusAnim"></param>
        private void SwitchAnim3(bool isOn, ActorAnim3 plusAnim, ActorAnim3 minusAnim)
        {
            if (isOn)
            {
                OnChangeAnim(plusAnim, true);
                OnChangeAnim(minusAnim, false);
            }
            else
            {
                OnChangeAnim(plusAnim, false);
                OnChangeAnim(minusAnim, true);
            }
        }

        /// <summary>
        /// デリゲートセット必須
        /// </summary>
        /// <param name="changeAnimDelegate"></param>
        public ActorBuff(ChangeAnimDelegate changeAnimDelegate)
        {
            OnChangeAnim += changeAnimDelegate;
        }

        /// <summary>
        /// 時間制限込みのステータス上昇
        /// </summary>
        /// <param name="targetBuff"></param>
        /// <param name="seconds"></param>
        public void SetLimitedTimeAbility(BuffType type, object value, int seconds)
        {
            //同じステータス上昇は消す
            lock (IsTimerStarted)
            {
                if (IsTimerStarted.ContainsKey(type))
                {
                    IsTimerStarted[type] = false;
                }
            }

            //タイマー処理
            Action<object> timer = (t) =>
            {
                BuffType btype = (((BuffType type, object value))t).type;
                object bvalue = (((BuffType type, object value))t).value;

                IsTimerStarted[btype] = true;
                Add(btype, bvalue);
                for (int i = 0; i < seconds * 10 && IsTimerStarted[btype]; i++)
                {
                    Thread.Sleep(100);
                }
                Remove(btype, bvalue);
                IsTimerStarted.Remove(btype);
            };

            Task.Factory.StartNew(timer, (type, value));
        }

        /// <summary>
        /// 加算
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public void Add(BuffType type, object value)
        {
            switch (type)
            {
                case BuffType.Damage:
                    Damage += (int)value;
                    return;
                case BuffType.Defence:
                    Defence += (int)value;
                    return;
                case BuffType.Accuracy:
                    Accuracy += (int)value;
                    return;
                case BuffType.Avoidance:
                    Avoidance += (int)value;
                    return;
                case BuffType.MovingSpeed:
                    MovingSpeed += (int)value;
                    return;
                case BuffType.AttackSpeed:
                    AttackSpeed += (int)value;
                    return;
                case BuffType.MagicResistance:
                    MagicResistance += (Magic)value;
                    return;
            }
        }

        /// <summary>
        /// 減算
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public void Remove(BuffType type, object value)
        {
            switch (type)
            {
                case BuffType.Damage:
                    Damage -= (int)value;
                    return;
                case BuffType.Defence:
                    Defence -= (int)value;
                    return;
                case BuffType.Accuracy:
                    Accuracy -= (int)value;
                    return;
                case BuffType.Avoidance:
                    Avoidance -= (int)value;
                    return;
                case BuffType.MovingSpeed:
                    MovingSpeed -= (int)value;
                    return;
                case BuffType.AttackSpeed:
                    AttackSpeed -= (int)value;
                    return;
                case BuffType.MagicResistance:
                    MagicResistance -= (Magic)value;
                    return;
            }
        }

        /// <summary>
        /// バフのタイプ
        /// </summary>
        public enum BuffType
        {
            Damage,
            Defence,
            Accuracy,
            Avoidance,
            MovingSpeed,
            AttackSpeed,
            MagicResistance,
        }
    }
}
