using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using System.IO;
using RedStoneLib.Packets;
using RedStoneLib.Model.Effect;
using RedStoneLib.Karmas;
using RedStoneLib.Model.Base;
using static RedStoneLib.Model.Effect.PlayerEffect;

namespace RedStoneLib.Model
{
    /// <summary>
    /// プレイヤー
    /// </summary>
    public partial class Player : Actor
    {
        JOB _job;
        int _baseMoveSpeed = 200;
        ushort _baseDefence = 0;
        ushort _level = 0;

        /// <summary>
        /// プレイヤの主キー（シーケンシャル）
        /// </summary>
        [Key]
        public int PlayerId { get; set; }

        /// <summary>
        /// Actorの種類
        /// </summary>
        [NotMapped]
        [SimpleActorInfo(0, 0x02)]
        protected override byte ActorType => 0;

        /// <summary>
        /// レベル +0x1A
        /// </summary>
        [NotMapped]
        [SimpleActorInfo(17, 0x0A)]
        public override ushort Level
        {
            get => _level;
            set
            {
                if (_level != 0)
                {
                    //初期化以外のレベル変更はステータスも変更
                    _level = value;
                    HandleChangeStatus(ChangeStatusInfo.Status, Effect, (int?)(
                        ActorStatusType.Strength | ActorStatusType.Agility |
                        ActorStatusType.Condition | ActorStatusType.Wisdom |
                        ActorStatusType.Inteligence | ActorStatusType.Charisma |
                        ActorStatusType.Luckey));
                }
                else
                {
                    _level = value;
                }
            }
        }

        /// <summary>
        /// 現在経験値
        /// </summary>
        public uint EXP { get; set; }

        /// <summary>
        /// 現在スキルポイント
        /// </summary>
        public uint SkillPoint { get; set; }

        /// <summary>
        /// 現在HP +0xC4
        /// </summary>
        public override uint NowHP { get; set; }

        /// <summary>
        /// 最大HP +0xC8
        /// </summary>
        [NotMapped]
        [SimpleActorInfo(2, type: typeof(ushort))]
        public override uint MaxHP { get; set; }

        /// <summary>
        /// ベースHP
        /// </summary>
        public uint BaseHP { get; set; }

        /// <summary>
        /// 現在CP
        /// </summary>
        public override int NowCP { get; set; }

        /// <summary>
        /// 最大CP
        /// </summary>
        [NotMapped]
        public override uint MaxCP { get; set; }

        /// <summary>
        /// ベースCP
        /// </summary>
        public uint BaseCP { get; set; }

        /// <summary>
        /// 健康値がHPとCPにボーナスとして還元する割合[%]
        /// </summary>
        [NotMapped]
        public ushort StateHPCPBonus { get; set; }

        /// <summary>
        /// レベルがHPとCPにボーナスとして還元する割合[%]
        /// </summary>
        [NotMapped]
        public ushort LevelHPCPBobuns { get; set; }

        /// <summary>
        /// 素のステータス
        /// </summary>
        [NotMapped]
        public ActorStatus BaseStatus { get; set; }

        /// <summary>
        /// 永続的な攻撃力
        /// </summary>
        [NotMapped]
        private Scale<ushort> BaseAttack { get; set; }

        /// <summary>
        /// 永続的な魔法抵抗
        /// </summary>
        private Magic BaseMagicResistance;

        /// <summary>
        /// 永続的な状態異常抵抗
        /// </summary>
        private StatusAbnormal BaseAbnormalResistance;

        /// <summary>
        /// 補正込みステータス
        /// </summary>
        [NotMapped]
        public override ActorStatus Status { get; set; }

        /// <summary>
        /// 攻撃力
        /// </summary>
        [NotMapped]
        public override Func<double, double, Scale<double>> Attack { get; set; }

        /// <summary>
        /// 防御力
        /// </summary>
        [NotMapped]
        public override double Defence { get; set; }

        /// <summary>
        /// 魔法抵抗
        /// </summary>
        [NotMapped]
        public override Magic MagicResistance { get; set; }

        /// <summary>
        /// 性向
        /// </summary>
        public short Tendency { get; set; }

        /// <summary>
        /// 装備
        /// </summary>
        [NotMapped]
        public EquipmentItemCollection EquipmentItems { get; set; }

        /// <summary>
        /// ベルトアイテム
        /// </summary>
        [NotMapped]
        public ItemCollection BeltItems { get; set; }

        /// <summary>
        /// インベントリ
        /// </summary>
        [NotMapped]
        public ItemCollection InventoryItems { get; set; }

        /// <summary>
        /// ユーザID
        /// </summary>
        public string UserID { get; set; }

        /// <summary>
        /// キャラクター名
        /// </summary>
        [SimpleActorInfo(35)]
        public override string Name { get; set; }

        /// <summary>
        /// 種族
        /// </summary>
        [NotMapped]
        public override Breed.ActorRace Race
            => Breed.ActorRace.Human;

        /// <summary>
        /// 職業
        /// </summary>
        [SimpleActorInfo(12, 0x0A, type: typeof(ushort))]
        public override JOB Job
        {
            get => _job;
            set
            {
                _job = value;
                OnJobChanged?.Invoke(value);
            }
        }

        /// <summary>
        /// 所持金
        /// </summary>
        public uint Gold { get; set; }

        /// <summary>
        /// 現在のステータスポイント
        /// </summary>
        public uint StatusPoint { get; set; }

        /// <summary>
        /// X座標
        /// </summary>
        [NotMapped]
        [SimpleActorInfo(27)]
        public override ushort PosX { get; set; }

        /// <summary>
        /// Y座標
        /// </summary>
        [NotMapped]
        [SimpleActorInfo(28)]
        public override ushort PosY { get; set; }

        /// <summary>
        /// サイズ
        /// </summary>
        [NotMapped]
        public override Size<ushort> BodySize { get; set; } 
            = new Size<ushort>(10, 10);

        /// <summary>
        /// GMLevel +0x7F2
        /// </summary>
        [SimpleActorInfo(14, 0x03)]
        public int GMLevel
        {
            get => playerSpecialState.GMLevel;
            set => playerSpecialState.GMLevel = value;
        }

        /// <summary>
        /// デスペナ
        /// </summary>
        public int DeathPenarty
        {
            get => playerSpecialState.DeathPenarty;
            set => playerSpecialState.DeathPenarty = value;
        }

        /// <summary>
        /// 現在地
        /// </summary>
        [NotMapped]
        public ushort MapSerial { get; set; }

        /// <summary>
        /// 走りフラグ
        /// </summary>
        public bool IsRun { get; set; }

        /// <summary>
        /// 方向
        /// </summary>
        [NotMapped]
        [SimpleActorInfo(15, 0x03, type: typeof(byte))]
        public override ActorDirect Direct { get; set; }

        /// <summary>
        /// 移動速度
        /// </summary>
        [NotMapped]
        public ushort MoveSpeed { get; set; }

        /// <summary>
        /// キャラID
        /// </summary>
        [NotMapped]
        [SimpleActorInfo(10, 0x0B)]
        public override ushort CharID { get; set; }

        /// <summary>
        /// SAI用現在HP
        /// </summary>
        [NotMapped]
        [SimpleActorInfo(1)]
        private byte SimpleNowHP => (byte)(NowHP * 255.0 / MaxHP);

        /// <summary>
        /// 攻撃可能 +0x14
        /// +junk size:1
        /// </summary>
        [NotMapped]
        [SimpleActorInfo(3, 0x03, type: typeof(byte))]
        public bool CanAttack { get; private set; }

        /// <summary>
        /// 応急処置 +0xC0
        /// +junk size:4
        /// </summary>
        [NotMapped]
        [SimpleActorInfo(11, 0x0E, 0x3FF)]
        public ushort AidHP { get; set; }

        /// <summary>
        /// カーペットの種類 +0x12C
        /// </summary>
        [NotMapped]
        [SimpleActorInfo(13, 0x05)]
        public byte CarpetType { get; set; }

        /// <summary>
        /// グラフィックのサイズ +0xB8, +0xB6
        /// </summary>
        [NotMapped]
        [SimpleActorInfo(16, 0x05)]
        public ushort BodyWidth { get => BodySize.Width; }

        /// <summary>
        /// +0x77E
        /// </summary>
        [NotMapped]
        [SimpleActorInfo(18, 0x04)]
        public byte Unknown_5 { get; set; }

        /// <summary>
        /// 名前が消える +0x7F4, +0x780
        /// </summary>
        [NotMapped]
        [SimpleActorInfo(19, 0x01, type: typeof(byte))]
        public bool NoNameFlag { get; set; }

        /// <summary>
        /// プレイヤーの横につくギルドペット +0xE0
        /// +junk size:4
        /// </summary>
        [NotMapped]
        [SimpleActorInfo(21, 0x07, 0x03)]
        public byte GuildPetIndex { get; set; }

        /// <summary>
        /// ギルド番号 +0xD6
        /// </summary>
        [NotMapped]
        [SimpleActorInfo(22, 0x0C)]
        public ushort GuildIndex { get; set; }

        /// <summary>
        /// ミニペット1
        /// </summary>
        [SimpleActorInfo(23, 0x05)]
        public byte MiniPet1 { get; set; }

        /// <summary>
        /// ミニペット2
        /// </summary>
        [SimpleActorInfo(24, 0x05)]
        public byte MiniPet2 { get; set; }

        /// <summary>
        /// 転生回数 +0x13E
        /// </summary>
        [SimpleActorInfo(25, 0x04)]
        public byte RebornNumber { get; set; }

        /// <summary>
        /// ベル +0x13D
        /// </summary>
        [NotMapped]
        [SimpleActorInfo(26, 0x06, 0x01, typeof(byte))]
        public bool IsBell { get => otherHeadType.HasFlag(ActorHeadType.Bell); }

        /// <summary>
        /// 主にスキル用のアニメ +0x2F0
        /// </summary>
        [NotMapped]
        [SimpleActorInfo(29, type: typeof(uint))]
        public override ActorAnim1 Anim1 { get; set; }

        /// <summary>
        /// +0x2F4
        /// </summary>
        [NotMapped]
        [SimpleActorInfo(30, type: typeof(uint))]
        public override ActorAnim2 Anim2 { get; set; }

        /// <summary>
        /// +0x2F8
        /// </summary>
        [NotMapped]
        [SimpleActorInfo(31, type: typeof(uint))]
        public override ActorAnim3 Anim3 { get; set; }

        /// <summary>
        /// +0x2FC
        /// </summary>
        [NotMapped]
        [SimpleActorInfo(32, type: typeof(uint))]
        public override ActorAnim4 Anim4 { get; set; }

        /// <summary>
        /// +0x300
        /// </summary>
        [NotMapped]
        [SimpleActorInfo(33, type: typeof(uint))]
        public override ActorAnim5 Anim5 { get; set; }

        /// <summary>
        /// +0x23BA
        /// </summary>
        [NotMapped]
        [SimpleActorInfo(34, type: typeof(ushort))]
        public ActorAnim6 Anim6 { get; set; } = ActorAnim6.None;

        /// <summary>
        /// 取りうるレベルのスケール
        /// </summary>
        [NotMapped]
        public override Scale<ushort> LevelScale { get; set; } = new Scale<ushort> { Max = 999, Min = 1 };
        
        public delegate void JobChangedDelegate(JOB changedJob);

        /// <summary>
        /// マップのヘッダ
        /// </summary>
        public Map.MapHeader MapHeader;

        /// <summary>
        /// ジョブチェンジ後ののイベント
        /// </summary>
        public event JobChangedDelegate OnJobChanged;

        /// <summary>
        /// プレイヤーが発生中のイベント
        /// </summary>
        public (Event[] Events, int Progress) PlayerEvent;

        /// <summary>
        /// アイテムによる効果
        /// </summary>
        public PlayerEffect Effect;

        /// <summary>
        /// 支援系
        /// </summary>
        [NotMapped]
        public override ActorBuff Buff { get; set; }

        /// <summary>
        /// LoginServer用
        /// </summary>
        /// <param name="userID"></param>
        public Player(string userID)
        {
            UserID = userID;
        }

        /// <summary>
        /// paramaterLess
        /// </summary>
        public Player()
            :base()
        {
            LevelScale = new Scale<ushort> { Max = 999, Min = 1 };
            InitItems();
            Effect = new PlayerEffect();
            Effect.OnChangeStatus += HandleChangeStatus;
        }

        /// <summary>
        /// アイテム初期化
        /// </summary>
        private void InitItems()
        {
            InventoryItems = new ItemCollection(42);
            BeltItems = new ItemCollection(5);
            EquipmentItems = new EquipmentItemCollection(this, 18);
            InventoryItems.OnChangeEffect += HandleChangeEffect;
            EquipmentItems.OnChangeEffect += HandleChangeEffect;
        }

        /// <summary>
        /// ステータス全更新
        /// </summary>
        public void Reflesh()
        {
            foreach(ChangeStatusInfo info in Enum.GetValues(typeof(ChangeStatusInfo)))
            {
                HandleChangeStatus(info, Effect);
            }
        }

        /// <summary>
        /// アイテム効果変更イベントハンドラ
        /// </summary>
        /// <param name="pe"></param>
        /// <param name="reset"></param>
        public void HandleChangeEffect(PlayerEffect pe, bool reset)
        {
            if (!reset)
            {
                Effect += pe;
                if (pe.ItemBasicAttack.HasValue) Effect.ItemBasicAttack += pe.ItemBasicAttack;
            }
            else
            {
                Effect -= pe;
                if (pe.ItemBasicAttack.HasValue) Effect.ItemBasicAttack -= pe.ItemBasicAttack;
            }
        }

        /// <summary>
        /// ステータス変更イベントハンドラ
        /// </summary>
        /// <param name="infos"></param>
        /// <param name="pe"></param>
        /// <param name="peIndex"></param>
        public void HandleChangeStatus(ChangeStatusInfo infos, PlayerEffect pe, int? peIndex = null)
        {
            //ステータス以外の変更
            void NotStatusChangeStatus(ChangeStatusInfo info)
            {
                switch (info)
                {
                    case ChangeStatusInfo.HP:
                        MaxHP = (uint)(((Level * LevelHPCPBobuns + Status.Condition * StateHPCPBonus) / 100.0 + BaseHP) * (1 + pe.MaxHPRate / 100) + pe.MaxHP);
                        return;
                    case ChangeStatusInfo.CP:
                        MaxCP = (uint)(((Level * LevelHPCPBobuns + Status.Charisma * StateHPCPBonus) / 100.0 + BaseCP) * (1 + pe.MaxCPRate / 100) + pe.MaxCP);
                        return;
                    case ChangeStatusInfo.MoveSpeed:
                        MoveSpeed = (ushort)(_baseMoveSpeed * (100.0 + pe.AbilityRate.MovingSpeed) / 100.0);
                        return;
                    case ChangeStatusInfo.AttackPower:
                        Attack = (np, n) => ((1 + pe.ItemBasicAttack) * (100.0 + pe.AbilityRate.Damage + np) / 100.0 + pe.DamageScale + n) * (1.0 + Status.Strength / 200.0);
                        return;
                    case ChangeStatusInfo.Defence:
                        double skillPercent = 0;
                        double skillPlus = 0;
                        Defence = ((_baseDefence + pe.Defense) * (1.0 + (pe.AbilityRate.Defense + skillPercent) / 100.0) + skillPlus) * (1 + Status.Condition / 100.0);
                        return;
                    case ChangeStatusInfo.MagicResistance:
                        MagicResistance = (Magic)pe.MagicResistance + Status.GetResistanceByWisdom() + MapHeader.MagicRising + BaseMagicResistance - MapHeader.MagicDecline;
                        return;
                }
            }

            foreach (ChangeStatusInfo info in ((IEnumerable<ChangeStatusInfo>)Enum.GetValues(typeof(ChangeStatusInfo))).Where(t => infos.HasFlag(t)))
            {
                switch (info)
                {
                    case ChangeStatusInfo.Status://ステ変更
                        if (peIndex.HasValue)
                        {
                            ActorStatusType index = (ActorStatusType)peIndex.Value;
                            Status = Status.Set(index, (ushort)Math.Min(BaseStatus[index] + pe.Status[index] + pe.StatusRaiseRetio[index] * Level, pe.FixationStatus[index].ToInt32(null)));

                            switch (index)//影響するステータス変更
                            {
                                case ActorStatusType.Strength:
                                    if (!infos.HasFlag(ChangeStatusInfo.AttackPower)) NotStatusChangeStatus(ChangeStatusInfo.AttackPower);
                                    break;
                                case ActorStatusType.Condition:
                                    if (!infos.HasFlag(ChangeStatusInfo.HP)) NotStatusChangeStatus(ChangeStatusInfo.HP);
                                    if (!infos.HasFlag(ChangeStatusInfo.Defence)) NotStatusChangeStatus(ChangeStatusInfo.Defence);
                                    break;
                                case ActorStatusType.Charisma:
                                    if (!infos.HasFlag(ChangeStatusInfo.CP)) NotStatusChangeStatus(ChangeStatusInfo.CP);
                                    break;
                                case ActorStatusType.Wisdom:
                                    if (!infos.HasFlag(ChangeStatusInfo.MagicResistance)) NotStatusChangeStatus(ChangeStatusInfo.MagicResistance);
                                    break;
                            }
                        }
                        else
                        {
                            //indexがない場合は全更新
                            foreach(ActorStatusType stype in Enum.GetValues(typeof(ActorStatusType)))
                            {
                                HandleChangeStatus(ChangeStatusInfo.Status, pe, (int?)stype);
                            }
                        }
                        break;
                    default:
                        NotStatusChangeStatus(info);
                        break;
                }
            }
        }

        /// <summary>
        /// プレイヤー情報構造体の取得
        /// </summary>
        /// <returns></returns>
        public PlayerInfo GetPlayerInfoStruct()
        => new PlayerInfo
        {
            Level = Level,
            EXP = EXP,
            SkillPoint = SkillPoint,

            NowHP = NowHP * 100,
            BaseHP = BaseHP * 100,
            NowCP = NowCP * 100,
            BaseCP = BaseCP * 100,
            StateHPCPBonus = StateHPCPBonus,
            LevelHPCPBobuns = LevelHPCPBobuns,
            PlayerStatus = BaseStatus,
            MaxPower = BaseAttack.Max,
            MinPower = BaseAttack.Min,
            Defence = _baseDefence,

            Tendency = Tendency,

            MResistance = BaseMagicResistance,
            CAResistance = BaseAbnormalResistance,
            AllStatusAbnormalResistance = 0,
            AllStatusDeclineResistance = 0,
            AllStatusSpellResistance = 0,

            EquipmentItem = (Item.ItemInfo[])EquipmentItems,
            BeltItem = (Item.ItemInfo[])BeltItems,
            InventoryItem = (Item.ItemInfo[])InventoryItems,
            UserID = UserID,
            CharName = Name,
            Job = Job,

            Gold = Gold,
            StatusPoint = StatusPoint,
            PosX = PosX,
            PosY = PosY,

            Skills = PlayerSkill.GetInitSkill(Job),
        };

        /// <summary>
        /// アバターで書き込み
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public byte[] WriteAvatar(string ip)
        {
            byte[] result;
            using (PacketWriter bw = new PacketWriter())
            {
                bw.WriteSjis(Name ?? "", 0x12);
                bw.Write((ushort)_job);
                bw.Write(Level);
                bw.Write((ushort)(EquipmentItems.Weapon.Base.Shape));//weapon
                bw.Write((ushort)(EquipmentItems.Shield.Base.Shape));//shield
                bw.Write((ushort)(EquipmentItems.Body.Base.Shape));//body
                bw.Write((ushort)MapSerial);
                bw.WriteSjis(ip, 0x10);
                result = bw.ToArray();
            }
            return result;
        }

        /// <summary>
        /// 隠蔽されたSpecialStateの取得（ResultJoinGamePacket用）
        /// </summary>
        /// <returns></returns>
        public SpecialState GetSpecialState() 
            => playerSpecialState;

        /// <summary>
        /// 特殊なステートの実態
        /// </summary>
        private SpecialState playerSpecialState;

        /// <summary>
        /// 色
        /// </summary>
        private ActorColor color = ActorColor.NameBarGreen;

        /// <summary>
        /// その他の頭上ステータス
        /// </summary>
        private ActorHeadType otherHeadType = ActorHeadType.Normal;
        
        /// <summary>
        /// アイテムのユニークID記録用
        /// </summary>
        public (string EquipmentItem, string BeltItem, string InventoryItem) UniqueIdStringsFor;
    }
}
