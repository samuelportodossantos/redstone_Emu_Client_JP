using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RedStoneLib.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedStoneLib.Model;
using RedStoneEmu.Games;

namespace RedStoneEmu.Packets.Handlers.Action
{
    /// <summary>
    /// ターゲットに対してスキルを使う
    /// </summary>
    [PacketHandlerAttr(0x1025)]
    class UseSkillToTarget : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            ushort toCharID = reader.ReadUInt16();

            //使用したスキルID
            byte skillSlotNum_Using = reader.ReadByte();

            //左に登録してあるスキルID（Auto2はチェック入ってないと0xFF）
            byte skillSlotNum_Auto1 = reader.ReadByte();
            byte skillSlotNum_Auto2 = reader.ReadByte();

            var target = context.GetMapActors().Single(t => t.CharID == toCharID);
            var skillIndex = context.User.PlayerSkills[skillSlotNum_Using].Index;
            var skill = Skill.AllSkills[skillIndex];
            Games.Action.UseSkill(context, target, skill, 1);

            //反撃
            (target as Monster).ReactsTo(context.User);
        }
    }
}
