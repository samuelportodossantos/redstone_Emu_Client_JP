using RedStoneLib.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneEmu.Packets.Handlers.ActorEvent
{
    /// <summary>
    /// 選択肢を選んだ
    /// </summary>
    [PacketHandlerAttr(0x1040)]
    class SelectSpeech : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            //NPCのCharID
            ushort charID = reader.ReadUInt16();
            ushort junk1 = reader.ReadUInt16();

            //選択肢のIndex
            ushort selectionIndex = reader.ReadUInt16(); ;
            ushort junk2 = reader.ReadUInt16();//0xFFFF

            //進行
            int progress = context.User.PlayerEvent.Progress;

            //実行
            if(!context.User.PlayerEvent.Events[progress].Selections[selectionIndex]
                .Execute(context.User, context.SendPacket, charID))
            {
                Logger.WriteWarning("なにもない");
            }
        }
    }
}
