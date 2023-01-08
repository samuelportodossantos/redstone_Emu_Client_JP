using RedStoneLib.Model.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Model
{
    /// <summary>
    /// privateにすべきDB用メンバーまとめ
    /// </summary>
    public partial class Player
    {
        /// <summary>
        /// プレイヤのレベル（DB用）
        /// </summary>
        [Column("Level")]
        private short Level_DB
        {
            get => unchecked_cast<short>(Level);
            set => Level = unchecked_cast<ushort>(value);
        }

        /// <summary>
        /// ステートがHPとCPにボーナスとして還元する割合[%]（DB用）
        /// </summary>
        [Column("StateHPCPBonus")]
        private short StateHPCPBonus_DB
        {
            get => unchecked_cast<short>(StateHPCPBonus);
            set => StateHPCPBonus = unchecked_cast<ushort>(value);
        }

        /// <summary>
        /// レベルがHPとCPにボーナスとして還元する割合[%]（DB用）
        /// </summary>
        [Column("LevelHPCPBobuns")]
        private short LevelHPCPBobuns_DB
        {
            get => unchecked_cast<short>(LevelHPCPBobuns);
            set => LevelHPCPBobuns = unchecked_cast<ushort>(value);
        }

        /// <summary>
        /// ステータス（DB用）
        /// </summary>
        [Column("Status")]
        private string Status_DB
        {
            get => Helper.StructToString(BaseStatus);
            set => BaseStatus = Helper.StringToStruct<ActorStatus>(value);
        }

        /// <summary>
        /// 永続的な最大攻撃力（DB用）
        /// </summary>
        [Column("MaxPower")]
        private short MaxPower_DB
        {
            get => unchecked_cast<short>(BaseAttack.Max);
            set => BaseAttack.SetMax(unchecked_cast<ushort>(value));
        }

        /// <summary>
        /// 永続的な最小攻撃力（DB用）
        /// </summary>
        [Column("MinPower")]
        private short MinPower_DB
        {
            get => unchecked_cast<short>(BaseAttack.Min);
            set => BaseAttack.SetMin(unchecked_cast<ushort>(value));
        }

        /// <summary>
        /// 防御力（DB用）
        /// </summary>
        [Column("Defence")]
        private short Defence_DB
        {
            get => unchecked_cast<short>(Defence);
            set => Defence = unchecked_cast<ushort>(value);
        }

        /// <summary>
        /// 魔法属性抵抗（DB用）
        /// </summary>
        [Column("MResistance")]
        private string Mresistance_DB
        {
            get => Helper.StructToString(BaseMagicResistance);
            set => BaseMagicResistance = Helper.StringToStruct<Magic>(value);
        }

        /// <summary>
        /// 状態異常抵抗（DB用）
        /// </summary>
        [Column("CAResistance")]
        private string Caresistance_DB
        {
            get => Helper.StructToString(BaseAbnormalResistance);
            set => BaseAbnormalResistance = Helper.StringToStruct<StatusAbnormal>(value);
        }

        /// <summary>
        /// 装備（DB用）
        /// </summary>
        [Column("EquipmentItem")]
        private string EquipmentItem_DB
        {
            get => string.Join(",", EquipmentItems.Select(t => t.UniqueID).ToArray());
            set => UniqueIdStringsFor.EquipmentItem = value;
        }

        /// <summary>
        /// ベルトアイテム（DB用）
        /// </summary>
        [Column("BeltItem")]
        private string BeltItem_DB
        {
            get => string.Join(",", BeltItems.Select(t => t.UniqueID).ToArray());
            set => UniqueIdStringsFor.BeltItem = value;
        }

        /// <summary>
        /// インベントリ（DB用）
        /// </summary>
        [Column("InventoryItem")]
        private string InventoryItem_DB
        {
            get => string.Join(",", InventoryItems.Select(t => t.UniqueID).ToArray());
            set => UniqueIdStringsFor.InventoryItem = value;
        }

        /// <summary>
        /// X座標（DB用）
        /// </summary>
        [Column("PosX")]
        private short PosX_DB
        {
            get => unchecked_cast<short>(PosX);
            set => PosX = unchecked_cast<ushort>(value);
        }

        /// <summary>
        /// Y座標（DB用）
        /// </summary>
        [Column("PosY")]
        private short PosY_DB
        {
            get => unchecked_cast<short>(PosY);
            set => PosY = unchecked_cast<ushort>(value);
        }

        /// <summary>
        /// 方向（DB用）
        /// </summary>
        [Column("Direct")]
        private short Direct_DB
        {
            get => unchecked_cast<short>(Direct);
            set => Direct = (ActorDirect)unchecked_cast<ushort>(value);
        }

        /// <summary>
        /// ギルド番号（DB用）
        /// </summary>
        [Column("GuildIndex")]
        private short GuildIndex_DB
        {
            get => unchecked_cast<short>(GuildIndex);
            set => GuildIndex = unchecked_cast<ushort>(value);
        }

        /// <summary>
        /// 現在地（DB用）
        /// </summary>
        [Column("MapSerial")]
        private short MapSerial_DB
        {
            get => unchecked_cast<short>(MapSerial);
            set => MapSerial = unchecked_cast<ushort>(value);
        }
    }
}
