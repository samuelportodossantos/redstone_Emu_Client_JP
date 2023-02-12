using RedStoneLib.Model.Effect;
using RedStoneLib.Packets;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static RedStoneLib.Model.Player;

namespace RedStoneLib.Model.Base
{
    /// <summary>
    /// アイテムのフライウェイト
    /// </summary>
    [NotMapped]
    public class ItemBase
    {
        /// <summary>
        /// ゲームアイテム全て
        /// </summary>
        public static ItemBase[] AllItemBases { get; private set; } = null;

        /// <summary>
        /// 全てのアイテムを読み込む
        /// </summary>
        public static void Load()
        {
            using (PacketReader br = new PacketReader(Helper.StreamFromAssembly("Scenario.Red_Stone.item.dat")))
            {
                //復号化キーのセット
                br.SetDataEncodeTable(br.ReadInt32());

                //アイテムのサイズ
                int itemLength = br.EncryptionRead<int>();
                Console.WriteLine(itemLength);

                //どこかへのオフセット
                uint unknownOffset = br.ReadUInt32();

                //全アイテム取得
                AllItemBases = br.EncryptionReads<ItemBaseInfo>(itemLength).Select(t => new ItemBase(t)).ToArray();
            }
        }

        /// <summary>
        /// 実態
        /// </summary>
        private ItemBaseInfo m_value;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="itemBase">他のItemBase</param>
        public ItemBase(ItemBase itemBase) : this(itemBase.m_value)
        { }

        /// <summary>
        /// 実態を使うコンストラクタ
        /// </summary>
        /// <param name="itemSourceInfo"></param>
        private ItemBase(ItemBaseInfo itemSourceInfo)
        {
            m_value = itemSourceInfo;
            
            //OPEffect追加
            var notEmptyEffects = m_value.UniqueOPs.Where(t => !t.IsEmpty);
            if (notEmptyEffects.Count() > 0) {
                UniqueOP = new PlayerEffect();
                foreach (var oe in notEmptyEffects.Select(t => t.Value))
                {
                    UniqueOP += (oe.Item1, oe.Item2.TakeWhile(t => t != 0xFFFF).ToArray());
                }
            }
            else
            {
                UniqueOP = null;
            }

            //ダメージ追加
            if (m_value.AttackDamageMin != 0 || m_value.AttackDamageMax != 0)
            {
                if (UniqueOP == null) UniqueOP = new PlayerEffect();
                UniqueOP.ItemBasicAttack = new Scale<ushort> { Min = m_value.AttackDamageMin, Max = m_value.AttackDamageMax };
            }
        }

        /// <summary>
        /// デフォルト
        /// </summary>
        public ItemBase()
        {
            m_value = new ItemBaseInfo();
            m_value.Index = 0xFFFF;
            m_value.Name = "NULL";
            m_value.Shape = 0xFFFF;
        }

        /// <summary>
        /// アイテムのインデックス
        /// </summary>
        public uint Index => m_value.Index;

        /// <summary>
        /// アイテム名
        /// </summary>
        public string Name => m_value.Name;

        /// <summary>
        /// アイテムのタイプ
        /// </summary>
        public ItemType Type => m_value.Type;

        /// <summary>
        /// アイテムのbase price
        /// </summary>
        public uint BasePrice => m_value.BasePrice;

        /// <summary>
        /// 値段ベースから実際に販売する価格を求める種類
        /// </summary>
        public SellingPriceCalculationType SellingType => m_value.SellingPriceCalculationType;

        /// <summary>
        /// 攻撃範囲（武器のみ）
        /// </summary>
        public uint AttackRange => m_value.AttackRange;

        /// <summary>
        /// 攻撃速度[秒]（武器のみ）
        /// </summary>
        public double AttackSpeed => m_value.AttackSpeed / 100.0;

        /// <summary>
        /// 耐久減少型式
        /// </summary>
        public uint Durable => m_value.Durable;

        /// <summary>
        /// 装備可能レベル
        /// </summary>
        public ushort RequiredLevel => m_value.RequiredLevel;

        /// <summary>
        /// 装備可能ステータス
        /// </summary>
        public ActorStatus RequiredStatus => m_value.RequiredStatus;
        
        /// <summary>
        /// 画像のインデックス？不明
        /// </summary>
        public ushort Shape => m_value.Shape;

        /// <summary>
        /// 装備時の画像のインデックス
        /// </summary>
        public ushort ImageShapesIndex => m_value.ImageShapesIndex;

        /// <summary>
        /// クエスト情報　※下位2ビット以上がクエストID
        /// </summary>
        public ushort QuestID => (ushort)(m_value.QuestInfo >> 2);

        /// <summary>
        /// 重ね置き可能数
        /// </summary>
        public ushort StackableNum => m_value.StackableNum;

        /// <summary>
        /// ドロップレベル
        /// </summary>
        public ushort DropLevel => m_value.DropLevel;

        /// <summary>
        /// アイテム効果の最大．最小値
        /// </summary>
        public Scale<ushort>[] ValueScales => new Scale<ushort>[] {
            new Scale<ushort>(m_value.MinValue1, m_value.MaxValue1),
            new Scale<ushort>(m_value.MinValue2, m_value.MaxValue2)};

        /// <summary>
        /// 価格係数
        /// </summary>
        public ushort PriceFactor => m_value.PriceFactor;

        /// <summary>
        /// アイテムに関するフラグ
        /// </summary>
        public ItemFlags Flags => m_value.Flags;

        /// <summary>
        /// ドロップ係数
        /// </summary>
        public ushort DropCoefficient => m_value.DropCoefficient;

        /// <summary>
        /// 装備要求に関するフラグ
        /// </summary>
        public ItemRequiredFlags RequireFlags => m_value.RequireFlags;

        /// <summary>
        /// 装備可能な職業
        /// </summary>
        public ItemRequiredJobFlags RequireJobFlags => m_value.RequireJobFlags;

        /// <summary>
        /// 色？
        /// </summary>
        public byte ColorShape => m_value.ColorShape;

        /// <summary>
        /// アイテム固有OPの効果
        /// </summary>
        public readonly PlayerEffect UniqueOP;

        /// <summary>
        /// 空フラグ
        /// </summary>
        public bool IsEmpty { get => m_value.Index >= 0xFFFF; }

        /// <summary>
        /// ItemEffectの集合
        /// </summary>
        public (PlayerEffect.ItemEffect effect, ushort[] paramIndex)[] ItemEffects
            => m_value.UniqueEffects.Where(t => t.Effect != PlayerEffect.ItemEffect.EMPTY).Select(t => (t.Effect, t.NeedValueIndex)).ToArray();

        /// <summary>
        /// Uフラグ（固有OPで判断）
        /// </summary>
        public bool IsUniqueItem
            => m_value.UniqueOPs.Any(t => !t.IsEmpty);

        /// <summary>
        /// override
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Name;

        /// <summary>
        /// ItemBaseInfoサイズの取得
        /// </summary>
        /// <returns></returns>
        public static int GetItemBaseInfoSize()
            => Marshal.SizeOf(typeof(ItemBaseInfo));

        /// <summary>
        /// アイテムに関するフラグ
        /// </summary>
        [Flags]
        public enum ItemFlags : ushort
        {
            /// <summary>
            /// 使用可能
            /// </summary>
            CanUse = 0x80,

            /// <summary>
            /// DXアイテム
            /// </summary>
            IsDXItem = 0x100,

            /// <summary>
            /// 破壊不可能
            /// </summary>
            CantDestruction = 0x2000
        }

        /// <summary>
        /// アイテムの装備に関するフラグ
        /// </summary>
        public enum ItemRequiredFlags : ushort
        {
            /// <summary>
            /// ネクロが装備可能
            /// </summary>
            CanEquipNecro = 0x20
        }

        /// <summary>
        /// アイテムのを備可能な職業のフラグ
        /// </summary>
        [Flags]
        public enum ItemRequiredJobFlags : uint
        {
            /// <summary>
            /// 全員装備可能
            /// </summary>
            Anyone = uint.MaxValue,

            Swordsman = 0x01,
            Warrior = 0x02,
            Wizard = 0x04,
            Wolfman = 0x08,
            Bishop = 0x10,
            Angel = 0x20,
            Thief = 0x40,
            Monk = 0x80,
            Lancer = 0x100,
            Archer = 0x200,
            Tamer = 0x400,
            Summoner = 0x800,
            Princess = 0x1000,
            LittleWitch = 0x2000,
            Necro = 0x4000,
            Demon = 0x8000,
            NumerologyTeacher = 0x10000,
            Fighter = 0x20000,
            LightMaster = 0x40000
        }

        /// <summary>
        /// アイテムソース構造体
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        private struct ItemBaseInfo
        {
            /// <summary>
            /// アイテムのインデックス
            /// </summary>
            public uint Index;

            /// <summary>
            /// アイテム名
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x40)]
            public string Name;

            public uint Unknown_0;
            public uint Unknown_1;

            /// <summary>
            /// アイテムのタイプ
            /// </summary>
            public ItemType Type;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x12)]
            public byte[] Unknown_2;

            /// <summary>
            /// アイテムのbase price
            /// </summary>
            public uint BasePrice;

            /// <summary>
            /// 値段ベースから実際に販売する価格を求める種類
            /// </summary>
            public SellingPriceCalculationType SellingPriceCalculationType;

            /// <summary>
            /// 攻撃範囲（武器のみ）
            /// </summary>
            public uint AttackRange;

            /// <summary>
            /// 攻撃速度（武器のみ）
            /// </summary>
            public ushort AttackSpeed;

            /// <summary>
            /// 最小攻撃力（武器のみ）
            /// </summary>
            public ushort AttackDamageMin;

            /// <summary>
            /// 最大攻撃力（武器のみ）
            /// </summary>
            public ushort AttackDamageMax;

            /// <summary>
            /// 耐久減少型式
            /// </summary>
            public uint Durable;

            public uint Unknown_3;

            /// <summary>
            /// 装備可能レベル
            /// </summary>
            public ushort RequiredLevel;

            /// <summary>
            /// 装備可能ステータス
            /// </summary>
            public ActorStatus RequiredStatus;

            public ushort Unknown_4;

            /// <summary>
            /// Shape
            /// </summary>
            public ushort Shape;

            public ushort Unknown_5;

            /// <summary>
            /// 装備時の画像のインデックス
            /// </summary>
            public ushort ImageShapesIndex;

            /// <summary>
            /// クエスト情報　※下位2ビット以上がクエストID
            /// </summary>
            public ushort QuestInfo;

            /// <summary>
            /// 重ね置き可能数
            /// </summary>
            public ushort StackableNum;

            /// <summary>
            /// ドロップレベル
            /// </summary>
            public ushort DropLevel;

            /// <summary>
            /// アイテム効果の最大．最小値（装備のみ）
            /// </summary>
            public ushort MinValue1;
            public ushort MaxValue1;
            public ushort MinValue2;
            public ushort MaxValue2;

            /// <summary>
            /// アイテム効果（装備・使用可能系アイテムとバッジ）
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x04)]
            public UniqueEffect[] UniqueEffects;

            /// <summary>
            /// アイテム固有OP（ユニークアイテムに多い）
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x06)]
            public UniqueOP[] UniqueOPs;

            /// <summary>
            /// 価格係数
            /// </summary>
            public ushort PriceFactor;

            /// <summary>
            /// アイテムに関するフラグ
            /// </summary>
            public ItemFlags Flags;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x14)]
            public byte[] Unknown_6;

            /// <summary>
            /// ドロップ係数
            /// </summary>
            public ushort DropCoefficient;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x12)]
            public byte[] Unknown_7;

            /// <summary>
            /// 色
            /// </summary>
            public byte ColorShape;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x05)]
            public byte[] Unknown_8;

            /// <summary>
            /// 装備要求に関するフラグ
            /// </summary>
            public ItemRequiredFlags RequireFlags;

            /// <summary>
            /// 装備可能な職業
            /// </summary>
            public ItemRequiredJobFlags RequireJobFlags;


            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x40)]
            public byte[] Unknown_9;

            /// <summary>
            /// アイテム固有Effect
            /// </summary>
            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public struct UniqueEffect
            {
                /// <summary>
                /// 固有Effectのインデックス
                /// </summary>
                public PlayerEffect.ItemEffect Effect;

                /// <summary>
                /// 必要な番号
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x04)]
                public ushort[] NeedValueIndex;

                /// <summary>
                /// override
                /// </summary>
                /// <returns></returns>
                public override string ToString()
                {
                    //リフレクション
                    var fieldInfo = Effect.GetType().GetField(Effect.ToString());
                    //コメントなどの属性を抜き出す
                    var attributes = (Comment[])fieldInfo.GetCustomAttributes(typeof(Comment), false);

                    return attributes[0].Str;
                }
            }

            /// <summary>
            /// アイテム固有OP
            /// </summary>
            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public struct UniqueOP
            {
                /// <summary>
                /// 固有OPのインデックス
                /// </summary>
                private PlayerEffect.OPEffect Effect;

                /// <summary>
                /// 固有OPの値
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x08)]
                private ushort[] Values;

                /// <summary>
                /// PlayerEffectに加える値
                /// </summary>
                public (PlayerEffect.OPEffect, ushort[]) Value
                    => (Effect, Values);

                /// <summary>
                /// 空フラグ
                /// </summary>
                public bool IsEmpty
                    => Effect == PlayerEffect.OPEffect.EMPTY;
                
                /// <summary>
                /// override
                /// </summary>
                /// <returns></returns>
                public override string ToString()
                {
                    //リフレクション
                    var fieldInfo = Effect.GetType().GetField(Effect.ToString());
                    //コメントなどの属性を抜き出す
                    var attributes = (Comment[])fieldInfo.GetCustomAttributes(typeof(Comment), false);

                    return attributes[0].Str;
                }
            }
        }

        /// <summary>
        /// PriceBaseから販売価格を計算する種類（別途OPによる値上げが存在）
        /// </summary>
        public enum SellingPriceCalculationType : ushort
        {
            /// <summary>
            /// ベースプライスのみ
            /// </summary>
            OnlyBasePrice = 0,

            /// <summary>
            /// 所持数のみ
            /// </summary>
            OnlyNum = 1,

            /// <summary>
            /// ベースプライス*所持数
            /// </summary>
            BasePrice_times_Num = 2,

            /// <summary>
            /// ベースプライス*耐久力
            /// </summary>
            BasePrice_times_Endurance = 3,

            /// <summary>
            /// ベースプライス*（所持数+1）
            /// </summary>
            BasePrice_times__Num_plus_1__ = 4,

            /// <summary>
            /// 所持数*耐久力/ベースプライス
            /// </summary>
            Num_times_Endurance_devide_BasePrace = 5,

            /// <summary>
            /// 特殊ケース
            /// </summary>
            Special_BasePrice_times_Num = 6,

            /// <summary>
            /// ベースプライス*個数*耐久力
            /// </summary>
            BasePrice_times_Num_times_Endurance = 7,
        }

        /// <summary>
        /// アイテムの種類
        /// </summary>
        public enum ItemType : ushort
        {
            /// <summary>
            /// 帽子
            /// </summary>
            Hat = 0,

            /// <summary>
            /// 冠
            /// </summary>
            Head = 1,

            /// <summary>
            /// グローブ
            /// </summary>
            Globe = 2,

            /// <summary>
            /// 槍投
            /// </summary>
            Yarito = 3,

            /// <summary>
            /// クロー
            /// </summary>
            Crow = 4,

            /// <summary>
            /// 手首
            /// </summary>
            Wrist = 5,

            /// <summary>
            /// ベルト
            /// </summary>
            Belt = 6,

            /// <summary>
            /// 足
            /// </summary>
            Shoes = 7,

            /// <summary>
            /// 首
            /// </summary>
            Neck = 8,

            /// <summary>
            /// 指
            /// </summary>
            Ring = 9,

            /// <summary>
            /// 耳
            /// </summary>
            Ear = 10,

            /// <summary>
            /// 背中
            /// </summary>
            Back = 11,

            /// <summary>
            /// ブロ
            /// </summary>
            Brooch = 12,

            /// <summary>
            /// 腕刺青
            /// </summary>
            ArmTattoo = 13,

            /// <summary>
            /// 肩刺青
            /// </summary>
            ShoulderTattoo = 14,

            /// <summary>
            /// 十字架
            /// </summary>
            Cross = 15,

            /// <summary>
            /// 鎧
            /// </summary>
            Armor = 16,

            /// <summary>
            /// 職鎧
            /// </summary>
            Shokuyoroi = 17,

            /// <summary>
            /// 片手剣
            /// </summary>
            OneHandedSword = 18,

            /// <summary>
            /// 盾
            /// </summary>
            Shield = 19,

            /// <summary>
            /// 両手剣
            /// </summary>
            DoubleHandedSword = 20,

            /// <summary>
            /// 杖
            /// </summary>
            Rod = 21,

            /// <summary>
            /// 牙
            /// </summary>
            Fang = 22,

            /// <summary>
            /// 棍棒
            /// </summary>
            Club = 23,

            /// <summary>
            /// 翼
            /// </summary>
            Wing = 24,

            /// <summary>
            /// 短剣
            /// </summary>
            Dagger = 25,

            /// <summary>
            /// 弓
            /// </summary>
            Bow = 26,

            /// <summary>
            /// 矢
            /// </summary>
            Arrow = 27,

            /// <summary>
            /// 槍
            /// </summary>
            Spear = 28,

            /// <summary>
            /// 笛
            /// </summary>
            Whistle = 29,

            /// <summary>
            /// スリング
            /// </summary>
            Sling = 30,

            /// <summary>
            /// ボトル
            /// </summary>
            Bottle = 31,

            /// <summary>
            /// 棒
            /// </summary>
            Steck = 32,

            /// <summary>
            /// 鞭
            /// </summary>
            Whip = 33,

            /// <summary>
            /// 原石
            /// </summary>
            Gemstone = 34,

            /// <summary>
            /// 赤POT
            /// </summary>
            RedPOT = 35,

            /// <summary>
            /// 青POT
            /// </summary>
            BluePOT = 36,

            /// <summary>
            /// 水薬
            /// </summary>
            LiquidMedicine = 37,

            /// <summary>
            /// 能力アップ
            /// </summary>
            CapacityUp = 38,

            /// <summary>
            /// 異常回復
            /// </summary>
            AbnormalRecovery = 39,

            /// <summary>
            /// 復活系
            /// </summary>
            Resurrection = 40,

            /// <summary>
            /// 鍵
            /// </summary>
            Key = 41,

            /// <summary>
            /// 帰還
            /// </summary>
            Return = 42,

            /// <summary>
            /// 必殺技の巻物
            /// </summary>
            ScrollOfDeathblow = 43,

            /// <summary>
            /// お菓子
            /// </summary>
            Candy = 44,

            /// <summary>
            /// 霊薬
            /// </summary>
            UnusuallyEffectiveMedicine = 45,

            /// <summary>
            /// 魔法液
            /// </summary>
            MagicSolution = 46,

            /// <summary>
            /// セッティング原石
            /// </summary>
            SettingsGemstone = 47,

            /// <summary>
            /// その他特殊アイテム
            /// </summary>
            OtherItems = 48,

            /// <summary>
            /// クエストアイテム
            /// </summary>
            QuestItem = 49,

            /// <summary>
            /// 課金アイテム
            /// </summary>
            Premium = 50,

            /// <summary>
            /// エンチャント系
            /// </summary>
            Enchantment = 51,

            /// <summary>
            /// ロト系
            /// </summary>
            Lotto = 52,

            /// <summary>
            /// 鎌
            /// </summary>
            Sickle = 54,

            /// <summary>
            /// 闘士武器
            /// </summary>
            FighterCrow = 55,

            /// <summary>
            /// 本
            /// </summary>
            Book = 56,

            None = 0xFFFF,
        }

        /// <summary>
        /// パケット送信用
        /// </summary>
        public enum ItemResult : ushort
        {
            OK = 0,
            OK2 = 1,

            //以下取引系
            NoSpace_trade = 9,//	インベの空きがない
            OnlyOne = 11,//	同じものを一つ以上持てない
            ThreeBadge = 12,//	バッジは3個以上持てない
            Trade_Cancel1 = 13,//	取引キャンセル
            Trade_Cancel2 = 14,//	取引キャンセル（with取引終了）
            Trade_Cancel_ChangedInventory = 15,//	取引キャンセル（インベントリ情報が変更）
            Trade_Cancel_Error = 16,//	取引キャンセル（アイテム情報に問題）
            LimitGold = 17,//	ゴールドの限界
            LimitIngot = 18,//	相手はこれ以上金のインゴットを持てない

            //以下アイテム取得系
            ItemInfoError = 0x1C,//アイテム情報に問題が
            ItemCopy = 0x3F,//アイテムコピーの疑惑があるアイテムです
            Item_disappeared = 0x40,//アイテムが消えました
            Death_Me = 0x41,//死んだ状態ではアイテムを拾えません
            NoOwner = 0x42,//持ち主がいるアイテムです
            GetThrowWeapon = 0x43,//投げた武器を拾いました
            GetTHrowWeapom2 = 0x44,//投げた武器を拾いました
            NoSpace = 0x46,//インベントリがいっぱい
            Over2Billion = 0x47,//20億ゴールド以上持てない
            Particular_quest = 0x48,//特定クエストのためのアイテムです
            CantAction = 0x4A,//現在何の行動もできない状態です
            Same2Items = 0x62,//同じアイテムを2個以上持てない
            Badge3 = 0x63, //バッジは3個以上持てない
            CantDestroy = 0x78,//そのアイテムは壊せません
            ItemInfoError2 = 0x79,//アイテム情報に問題が（破壊用）

            //以下装備系
            NeedBelt = 0x69,//	ベルトを着用する必要があります
            CantEquipRing = 0x67,//	指輪を装着できません
            LowAbility = 0x65,//	能力が足りません
            NoEquipment = 0x66,//	装備アイテムではありません
            NotEmptyInventory = 0x6A,//	インベントリに空きスペースがない
            ActionNow = 0x6B,//	行動中には装備ができない
            CanBeEquipThere = 0x6C,//	そこに装備できるアイテムではありません

            AlreadyEquip = 0x6E,//既に装備中
            ActionNow_Dequip = 0x6F,//行動中には装備の解除ができない
            CantTakeShield = 0x70,//盾を取ることができない
            CantTakeWeapon = 0x71,//武器を取ることができない
            CantChangeShield = 0x72,//	盾を変えることができない状態です
            CamtChangeWeapon = 0x73,//	武器を変えることができない状態です
            EquipMAX = 0x80,//	これ以上装備することができません
            AskItemData = 0x1C,//	AskItemData
            AskItemData2 = 0x68,//	AskItemData
            AlreadyEquipment = 0x64,//	既に装備中です
            AlreadyEquipment2 = 0x6E,//	既に装備中です

        }
    }
}
