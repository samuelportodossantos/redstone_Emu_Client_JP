using RedStoneLib.Model.Effect;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// プレイヤーに関する構造体関連
/// </summary>
namespace RedStoneLib.Model
{
    public partial class Player
    {
        /// <summary>
        /// プレイヤ情報の構造体
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct PlayerInfo
        {
            /// <summary>
            /// レベル
            /// </summary>
            public uint Level;

            /// <summary>
            /// 現在経験値
            /// </summary>
            public uint EXP;

            /// <summary>
            /// 現在スキルポイント
            /// </summary>
            public uint SkillPoint;

            /// <summary>
            /// 現在HP（1/100）
            /// </summary>
            public uint NowHP;

            /// <summary>
            /// ベースHP
            /// （最大HPの計算時にレベルや健康に関係なく存在する部分）
            /// </summary>
            public uint BaseHP;

            /// <summary>
            /// 現在CP（1/100）
            /// </summary>
            public int NowCP;

            /// <summary>
            /// ベースCP
            /// （最大HPの計算時にレベルやカリスマに関係なく存在する部分）
            /// </summary>
            public uint BaseCP;

            /// <summary>
            /// ステートがHPとCPにボーナスとして還元する割合[%]
            /// </summary>
            public ushort StateHPCPBonus;

            /// <summary>
            /// レベルがHPとCPにボーナスとして還元する割合[%]
            /// </summary>
            public ushort LevelHPCPBobuns;

            /// <summary>
            /// ステータス
            /// </summary>
            public ActorStatus PlayerStatus;

            public ushort unknown_0;

            /// <summary>
            /// 永続的な最大攻撃力
            /// </summary>
            public ushort MaxPower;

            /// <summary>
            /// 永続的な最小攻撃力
            /// </summary>
            public ushort MinPower;

            /// <summary>
            /// 永続的な防御力
            /// </summary>
            public ushort Defence;

            /// <summary>
            /// 性向
            /// </summary>
            public short Tendency;

            /// <summary>
            /// 永続的な魔法属性抵抗
            /// </summary>
            public Magic MResistance;

            /// <summary>
            /// 永続的な状態異常抵抗
            /// </summary>
            public StatusAbnormal CAResistance;
            
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
            /// 装備
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)]
            public Item.ItemInfo[] EquipmentItem;

            /// <summary>
            /// ベルトアイテム
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public Item.ItemInfo[] BeltItem;

            /// <summary>
            /// インベントリ
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 42)]
            public Item.ItemInfo[] InventoryItem;

            /// <summary>
            /// ユーザID
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x14)]
            public string UserID;

            /// <summary>
            /// キャラクター名
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x12)]
            public string CharName;

            /// <summary>
            /// 職業
            /// </summary>
            public JOB Job;

            public ushort unknown_1;

            /// <summary>
            /// 所持金
            /// </summary>
            public uint Gold;

            /// <summary>
            /// 現在のステータスポイント
            /// </summary>
            public uint StatusPoint;

            /// <summary>
            /// X座標
            /// </summary>
            public uint PosX;

            /// <summary>
            /// Y座標
            /// </summary>
            public uint PosY;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public byte[] unknown_2;

            /// <summary>
            /// スキル
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 52)]
            public PlayerSkill[] Skills;
            
            public ushort unknown_3;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public uint[] unknown_4;

            /// <summary>
            /// 称号保留
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public ushort[] Titles;

            /// <summary>
            /// クエスト
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public uint[] Quests;
        }

        /// <summary>
        /// プレイヤーのスキルを扱う
        /// </summary>
        public struct PlayerSkill
        {
            /// <summary>
            /// スキルのインデックス
            /// </summary>
            public ushort Index;

            /// <summary>
            /// 現在のスキルレベル
            /// </summary>
            public ushort Level;

            /// <summary>
            /// 職業から初期PlayerSkill配列を取得
            /// </summary>
            /// <param name="job"></param>
            /// <returns></returns>
            public static PlayerSkill[] GetInitSkill(JOB job)
            {
                //ジョブチェン先の職業
                JOB job2 = (short)job % 2 == 0 ? job + 1 : job - 1;

                //職業の最初のskill index
                ushort firstIndex = Skill.AllSkills.Values.First(t => t.Job == job).Index;

                //取得
                var initialSkills = Skill.AllSkills.Values
                    .Where(t => t.Job == job || t.Job == job2)
#if DEBUG
                    .Select(t => new PlayerSkill { Index = t.Index, Level = firstIndex == t.Index ? (ushort)1 : (ushort)1 })
#else
                    
                    .Select(t => new PlayerSkill { Index = t.Index, Level = firstIndex == t.Index ? (ushort)1 : (ushort)0 })
#endif
                    .ToList();

                //52になるまでループ
                while (initialSkills.Count < 52) initialSkills.Add(new PlayerSkill { Index = ushort.MaxValue, Level = 0 });
                return initialSkills.ToArray();
            }

            /// <summary>
            /// 今までのSP合計値
            /// </summary>
            /// <param name="player"></param>
            /// <returns></returns>
            public static int SummationSP(Player player)
                => player.PlayerSkills.Sum(t => t.Level)
#if DEBUG
                - 50;
#else
                - 1;
#endif

            public override string ToString()
                => $"Level{Level}  {(Skill.AllSkills.TryGetValue(Index, out var value) ? value.Name : "Invalid")}";
        }

        /// <summary>
        /// アイテムのコレクション
        /// </summary>
        public class ItemCollection : IEnumerable<Item>, ICloneable
        {
            /// <summary>
            /// 元アイテム
            /// </summary>
            protected Item[] Items;

            public delegate void ChangeEffectDelegate(PlayerEffect pe, bool reset);

            protected ChangeEffectDelegate _OnChangeEffect;

            /// <summary>
            /// 効果変更イベント
            /// </summary>
            public virtual event ChangeEffectDelegate OnChangeEffect
            {
                add
                {
                    _OnChangeEffect += value;
                    foreach(var item in Items.Where(t => !t.IsEmpty))
                    {
                        OnItemSetting(item, false);
                    }
                }
                remove => _OnChangeEffect -= value;
            }

            public int Count => Items.Length;

            public bool IsReadOnly => false;

            public Item this[int index]
            {
                get => Items[index];
                set
                {
                    bool reset = value.IsEmpty;
                    if (reset && Items[index].IsEmpty) return;//両方null
                    OnItemSetting(reset ? Items[index] : value, reset);
                    Items[index] = value;
                }
            }

            /// <summary>
            /// アイテムのセット前の呼び出し
            /// </summary>
            /// <param name="value"></param>
            /// <param name="reset"></param>
            protected virtual void OnItemSetting(Item value, bool reset)
            {
                if(value.Base.Type == Base.ItemBase.ItemType.Premium)
                {
                    _OnChangeEffect?.Invoke(value.Effect, reset);
                }
            }
            
            /// <summary>
            /// アイテム場所変更
            /// </summary>
            /// <param name="from"></param>
            /// <param name="to"></param>
            /// <returns></returns>
            public bool ChangePlace(int from, int to)
            {
                if (Items[from].IsEmpty) return false;

                Item tmpItem = Items[from];
                Items[from] = Items[to];
                Items[to] = tmpItem;
                return true;
            }

            /// <summary>
            /// 空きスペースに順番でアイテム挿入
            /// </summary>
            /// <param name="injectItem"></param>
            /// <returns></returns>
            public bool InsertItem(Item injectItem)
            {
                if (IsFull) return false;
                var emptyItemIndex = Enumerable.Range(0, Items.Length).First(t => Items[t].IsEmpty);
                Items[emptyItemIndex] = injectItem;

                return true;
            }

            /// <summary>
            /// 指定Indexのアイテム排出
            /// </summary>
            /// <param name="itemIndex"></param>
            /// <param name="num"></param>
            /// <returns></returns>
            public bool EjectItem(ushort itemIndex, int num)
            {
                //個数チェック
                if (GetItemCountByItemIndex(itemIndex) < num) return false;

                for(int i = 0; i < Items.Length; i++)
                {
                    if (Items[i].ItemIndex != itemIndex) continue;
                    
                    if(Items[i].Count <= num)
                    {
                        num -= Items[i].Count;
                        Items[i] = new Item();
                    }
                    else
                    {
                        Items[i].Count -= (byte)num;
                        num = 0;
                    }
                    if (num == 0) break;
                }

                return true;
            }

            /// <summary>
            /// アイテムインデックスからアイテム個数を取得
            /// </summary>
            /// <param name="itemIndex"></param>
            /// <returns></returns>
            public int GetItemCountByItemIndex(ushort itemIndex)
                => Items.Where(t => t.ItemIndex == itemIndex).Sum(t => t.Count);

            /// <summary>
            /// アイテムの残りスペース
            /// </summary>
            public int EmptySpaceCount
                => Items.Count(t => t.IsEmpty);

            /// <summary>
            /// アイテム埋まっている
            /// </summary>
            public bool IsFull => 
                !Items.Any(t => t.IsEmpty);

            /// <summary>
            /// 空アイテムのコレクション
            /// </summary>
            /// <param name="count"></param>
            public ItemCollection(int count)
            {
                Items = new Item[count];
                for(int i = 0; i < count; i++)
                {
                    Items[i] = new Item();
                }
            }

            /// <summary>
            /// 指定インデックスのアイテムを範囲検索
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            public Item[] FindRangeByIndex(ushort index)
                => this.Where(t => t.ItemIndex == index).ToArray();

            /// <summary>
            /// アイテムから生成
            /// </summary>
            /// <param name="items"></param>
            public ItemCollection(Item[] items)
                : this(items.Length)
            {
                for (int i=0;i<items.Length;i++)
                {
                    this[i] = items[i];
                }
            }

            public IEnumerator<Item> GetEnumerator()
            {
                foreach(var item in Items)
                {
                    yield return item;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumerator();

            public object Clone()
            {
                Item[] items = Items.Select(t => (Item)t.Clone()).ToArray();
                return new ItemCollection(items);
            }

            /// <summary>
            /// キャスト
            /// </summary>
            /// <param name="ic"></param>
            public static explicit operator Item.ItemInfo[] (ItemCollection ic)
                => ic.Items.Select(t => t.GetM_Value()).ToArray();

            public static explicit operator Item[] (ItemCollection ic)
                => ic.Items;

            public static explicit operator ItemCollection (Item[] items)
                => new ItemCollection(items);
        }

        /// <summary>
        /// 装備品クラス
        /// </summary>
        public class EquipmentItemCollection : ItemCollection
        {
            /// <summary>
            /// 左手フラグ
            /// </summary>
            private bool IsLeftHand;

            /// <summary>
            /// 左手フラグのセット
            /// </summary>
            /// <returns></returns>
            private void SetIsLeftHand(JOB job)
            {
                switch (job)
                {
                    case JOB.Swordsman:
                    case JOB.Wizard:
                    case JOB.Bishop:
                    case JOB.Thief:
                    case JOB.Lancer:
                    case JOB.Tamer:
                    case JOB.Princess:
                    case JOB.Necro:
                    case JOB.NumerologyTeacher:
                    case JOB.LightMaster:
                        IsLeftHand = true;
                        break;
                    case JOB.Warrior:
                    case JOB.Wolfman:
                    case JOB.Angel:
                    case JOB.Monk:
                    case JOB.Archer:
                    case JOB.Summoner:
                    case JOB.LittleWitch:
                    case JOB.Demon:
                    case JOB.Fighter:
                        IsLeftHand = false;
                        break;
                    default:
                        throw new ArgumentException("職業が正しくありません");
                }
            }

            /// <summary>
            /// 効果変更イベント（職武器考慮）
            /// </summary>
            public override event ChangeEffectDelegate OnChangeEffect
            {
                add
                {
                    _OnChangeEffect += value;

                    //メイン武器のみ
                    if (!Weapon.IsEmpty)
                    {
                        OnItemSetting(Weapon, false);
                    }
                    //メイン武器飛ばす
                    foreach (var item in Items.Skip(1).Take(16).Where(t => !t.IsEmpty))
                    {
                        OnItemSetting(item, false);
                    }
                }
                remove => _OnChangeEffect -= value;
            }

            protected override void OnItemSetting(Item value, bool reset)
            {
                if (value.Effect != null)
                    _OnChangeEffect?.Invoke(value.Effect, reset);
            }

            /// <summary>
            /// 職業考慮した武器
            /// </summary>
            public Item Weapon
            {
                get => IsLeftHand ? WeaponLeft : WeaponRight;
                set
                {
                    if (IsLeftHand)
                    {
                        WeaponLeft = value;
                    }
                    else
                    {
                        WeaponRight = value;
                    }
                }
            }

            /// <summary>
            /// 左武器
            /// </summary>
            public Item WeaponLeft
            {
                get => Items[0];
                set => this[0] = value;
            }

            /// <summary>
            /// 盾
            /// </summary>
            public Item Shield
            {
                get => Items[1];
                set => this[1] = value;
            }

            /// <summary>
            /// 鎧
            /// </summary>
            public Item Body
            {
                get => Items[2];
                set => this[2] = value;
            }

            /// <summary>
            /// 手
            /// </summary>
            public Item Hand
            {
                get => Items[3];
                set => this[3] = value;
            }

            /// <summary>
            /// 頭
            /// </summary>
            public Item Head
            {
                get => Items[4];
                set => this[4] = value;
            }

            /// <summary>
            /// 背中
            /// </summary>
            public Item Back
            {
                get => Items[5];
                set => this[5] = value;
            }

            /// <summary>
            /// 首
            /// </summary>
            public Item Neck
            {
                get => Items[6];
                set => this[6] = value;
            }

            /// <summary>
            /// 腰
            /// </summary>
            public Item Belt
            {
                get => Items[7];
                set => this[7] = value;
            }

            /// <summary>
            /// 足
            /// </summary>
            public Item Shoes
            {
                get => Items[8];
                set => this[8] = value;
            }

            /// <summary>
            /// 指
            /// </summary>
            public Item[] Ring
            {
                get => Items.Skip(9).Take(8).ToArray();
                set
                {
                    for (int i = 0; i < 8; i++)
                    {
                        this[9 + i] = value[i];
                    }
                }
            }

            /// <summary>
            /// 右武器
            /// </summary>
            public Item WeaponRight
            {
                get => Items[17];
                set => this[17] = value;
            }

            public EquipmentItemCollection(Player player, int count)
                : base(count)
            {
                //職系
                SetIsLeftHand(player.Job);
                player.OnJobChanged += SetIsLeftHand;
            }

            public EquipmentItemCollection(Player player, Item[] items)
                : base(items)
            {
                //職系
                SetIsLeftHand(player.Job);
                player.OnJobChanged += SetIsLeftHand;
            }
        }
        
        /// <summary>
        /// 職業
        /// </summary>
        public enum JOB : short
        {
            Swordsman = 0,
            Warrior = 1,
            Wizard = 2,
            Wolfman = 3,
            Bishop = 4,
            Angel = 5,
            Thief = 6,
            Monk = 7,
            Lancer = 8,
            Archer = 9,
            Tamer = 10,
            Summoner = 11,
            Princess = 12,
            LittleWitch = 13,
            Necro = 14,
            Demon = 15,

            /// <summary>
            /// 霊術師
            /// </summary>
            NumerologyTeacher = 16,

            /// <summary>
            /// 闘士
            /// </summary>
            Fighter = 17,
            LightMaster = 18
        }

        /// <summary>
        /// 特殊なステ
        /// GMLevelとデスペナ
        /// </summary>
        public struct SpecialState
        {
            internal ushort m_value;

            public int GMLevel
            {
                get => (m_value >> 2) & 7;
                set => m_value |= (ushort)((value & 7) << 2);
            }

            public int DeathPenarty
            {
                get => (m_value >> 5) & 0xFFF;
                set => m_value |= (ushort)((value & 0xFFF) << 5);
            }

            public ushort ToUInt16()
            {
                return m_value;
            }
        }
    }
    

}
