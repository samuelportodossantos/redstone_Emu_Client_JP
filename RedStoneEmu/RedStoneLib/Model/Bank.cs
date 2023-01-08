using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using static RedStoneLib.Model.Player;

namespace RedStoneLib.Model
{
    public class Bank
    {
        /// <summary>
        /// 銀行全て
        /// key:session
        /// </summary>
        public static Dictionary<ushort, Bank> AllBanks = new Dictionary<ushort, Bank>();

        /// <summary>
        /// BankKey
        /// </summary>
        [Key]
        public int BankKey { get; set; }

        /// <summary>
        /// セッション
        /// </summary>
        [NotMapped]
        public ushort BankSession
            => (ushort)(BankKey & 0xFFFF);

        /// <summary>
        /// ユーザID
        /// </summary>
        public string UserID { get; set; }

        /// <summary>
        /// 保管されてるアイテムのユニークID
        /// </summary>
        public string UniqueItemString { get; set; }

        /// <summary>
        /// 保管されているアイテム
        /// </summary>
        [NotMapped]
        public ItemCollection Items = new ItemCollection(96);

        /// <summary>
        /// 一時的なインベ
        /// </summary>
        [NotMapped]
        public ItemCollection TemporaryInventory { get; set; }

        /// <summary>
        /// 一時的な装備
        /// </summary>
        [NotMapped]
        public ItemCollection TemporaryEquipment { get; set; }

        /// <summary>
        /// 一時的なベルト
        /// </summary>
        [NotMapped]
        public ItemCollection TemporaryBelt { get; set; }

        /// <summary>
        /// 保管されてるゴールド
        /// </summary>
        public uint Gold { get; set; }

        /// <summary>
        /// 不明 アイテム個数?
        /// </summary>
        public uint Unknown { get; set; } = 0x28;

        public Bank(string userID)
        {
            UserID = userID;
            Gold = 0;
            UniqueItemString = string.Join(',', Enumerable.Range(0, 96).Select(_ => 0));
        }

        /// <summary>
        /// 銀行チェックサム
        /// </summary>
        /// <param name="player"></param>
        /// <param name="inventorySize"></param>
        /// <returns></returns>
        public uint GetCheckSum(Player player, uint inventorySize=30)
        {
            //アイテム配列のチェックサム
            uint ItemsCheckSum(Item[] targetItem, int init=0)
            {
                uint res = 0;
                foreach ((var item, var i) in targetItem.Select((v, i) => (v, i)))
                {
                    uint checkSum = item?.GetCheckSum() ?? 0;
                    if (checkSum == 0) continue;

                    res += (uint)(checkSum + (i + 1) * 3 + init);
                }
                return res;
            }

            uint result = (uint)Helper.StringToSjisByte(UserID).Sum(t => t);
            result += Unknown;
            result += Gold;
            result += 0xCCCC;

            //銀行アイテム
            result += ItemsCheckSum(Items.ToArray());

            result += (ushort)(GetBankTax(player.Titles[1]) * 100.0);
            result += (uint)GetMaxSlotCount(player.Titles[1]);
            result += inventorySize;
            result += player.Gold;

            //その他アイテム
            result += ItemsCheckSum(TemporaryEquipment.ToArray());
            result += ItemsCheckSum(player.BeltItems.ToArray(), init: 18 * 3);
            result += ItemsCheckSum(TemporaryInventory.ToArray());

            return result;
        }

        /// <summary>
        /// 称号レベルから最大保管量を取得
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static int GetMaxSlotCount(byte level)
            => level * 6 + 36;

        /// <summary>
        /// 金利
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static double GetBankTax(byte level)
            => 1.1 - level * 0.1;
    }
}
