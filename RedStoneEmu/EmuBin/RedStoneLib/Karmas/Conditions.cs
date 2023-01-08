using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedStoneLib.Model;

namespace RedStoneLib.Karmas.Conditions
{
    /// <summary>
    /// 条件無しでOK
    /// </summary>
    [KarmaItemAttr(0x000)]
    class NoCondition : KarmaItemConditionService
    {
        public override bool HandleKarmaItem(Player player, uint[] value)
        {
            return true;
        }
    }

    /// <summary>
    /// レベル
    /// </summary>
    [KarmaItemAttr(0x001)]
    class CheckLevel : KarmaItemConditionService
    {
        public override bool HandleKarmaItem(Player player, uint[] value)
        {
            return player.Level < value[0];
        }
    }

    /// <summary>
    /// 時間チェック
    /// </summary>
    [KarmaItemAttr(0x193, 1, 2, 3, 4)]
    class CheckDate : KarmaItemConditionService
    {
        public override bool HandleKarmaItem(Player player, uint[] value)
        {
            return DateTime.Now > new DateTime((int)value[0], (int)value[1], (int)value[2], (int)(value[3] % 24), 0, 0);
        }
    }

    /// <summary>
    /// 国チェック
    /// </summary>
    [KarmaItemAttr(0x19C, 1)]
    class CheckCountry : KarmaItemConditionService
    {
        public override bool HandleKarmaItem(Player player, uint[] value)
        {
            return PublicHelper.Country == value[0];
        }
    }
}
