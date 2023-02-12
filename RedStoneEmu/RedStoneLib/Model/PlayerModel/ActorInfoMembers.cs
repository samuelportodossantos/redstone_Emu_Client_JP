using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Model
{
    /// <summary>
    /// privateにすべきActorInfo用メンバーまとめ
    /// </summary>
    public partial class Player
    {
        /// <summary>
        /// SAI用透明フラグ
        /// +0x13C
        /// </summary>
        [NotMapped]
        [SimpleActorInfo(5, 0x02, type: typeof(byte))]
        private ActorColor SimpleTransparentFlag => color.HasFlag(ActorColor.BodyTransparent)
                ? ActorColor.BodyTransparent : ActorColor.BodyNormal;

        /// <summary>
        /// SAI用武器Shape +0x1C
        /// </summary>
        [NotMapped]
        [SimpleActorInfo(6, 0x04)]
        [VerySimpleActorInfo(2, 0x04)]
        private ushort WeaponShape { get => EquipmentItems.Weapon.Base.Shape; }

        /// <summary>
        /// SAI用武器の光 +0x1E
        /// </summary>
        [NotMapped]
        [SimpleActorInfo(7, 0x08)]
        [VerySimpleActorInfo(3, 0x08)]
        private ushort WeaponLight { get => EquipmentItems.Weapon.GetWeaponColorizeEffect(); }

        /// <summary>
        /// SAI用盾Shape +0x20
        /// </summary>
        [NotMapped]
        [SimpleActorInfo(8, 0x04)]
        [VerySimpleActorInfo(4, 0x04)]
        private ushort ShieldShape { get => EquipmentItems.Shield.Base.Shape; }

        /// <summary>
        /// SAI用鎧Shape +0x24
        /// </summary>
        [NotMapped]
        [SimpleActorInfo(9, 0x02)]
        [VerySimpleActorInfo(5, 0x02)]
        private ushort BodyShape { get => EquipmentItems.Body.Base.Shape; }

        /// <summary>
        ///SAI用 鎧の光 +0x26
        /// </summary>
        [NotMapped]
        [SimpleActorInfo(10, 0x03)]
        [VerySimpleActorInfo(6, 0x03)]
        private ushort BodyLight { get => EquipmentItems.Body.GetWeaponColorizeEffect(); }
        
        /// <summary>
        /// SAI用名前barの色が青であることを伝える
        /// </summary>
        [NotMapped]
        [SimpleActorInfo(21, 0x02)]
        private int NotifyNameBarBlue { get => (byte)color >> 5; }
    }
}
