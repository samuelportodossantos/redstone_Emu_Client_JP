using RedStoneLib.Packets;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RedStoneLib.Packets.RSPacket.Quests;

namespace RedStoneEmu.Packets.Handlers.Other
{
    [PacketHandlerAttr(0x106A)]
    class CancelQuest : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            ushort index = reader.ReadUInt16();
            ushort questID = reader.ReadUInt16();

            if (context.User.ProgressQuests.Values.All(t => t.Index != questID))
            {
                //受注していないクエストのキャンセル
                context.SendPacket(new ResultCancelQuestPacket());
                return;
            }

            //インベからクエ品消す
            foreach(var item in context.User.InventoryItems.Select((v,i)=>new { v, i }).ToArray())
            {
                if (!item.v.IsEmpty && item.v.Base.QuestID == questID)
                {
                    ((GameClient)context).RemoveDBItems.Add(item.v);
                    context.User.InventoryItems[item.i] = new RedStoneLib.Model.Item();
                }
            }

            //マジバからアイテム消す

            //クエスト除去
            context.User.ProgressQuests.Remove(index);
            context.SendPacket(new ResultCancelQuestPacket(index));
        }
    }
}
