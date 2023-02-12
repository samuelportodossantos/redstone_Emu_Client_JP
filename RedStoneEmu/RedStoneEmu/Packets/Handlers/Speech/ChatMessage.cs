using RedStoneEmu.Games;
using RedStoneLib.Packets;
using RedStoneLib.Packets.RSPacket.Speech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneEmu.Packets.Handlers.Speech
{
    /// <summary>
    /// チャット
    /// </summary>
    [PacketHandlerAttr(0x1029)]
    class ChatMessage : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            Chat.ChatType chatType = (Chat.ChatType)reader.ReadUInt16();
            string toName = reader.ReadSjis(0x12);//耳の場合　送る相手
            string message = reader.ReadSjis((int)(size - 0x14));

            //GMコマンドチェック
            if (context.User.GMLevel > 0)
            {
                message = GMAction.ReceiveGMCommand(context as GameClient, message);
            }

            //全送信
            if (message != "")
            {
                MAPServer.AllMapServers[context.User.MapSerial].SendPacketsToPlayersOnTheMap(
                    new Chat(context.User.CharID, chatType, context.User.Name, message), isNear: true);
            }
        }
    }
}
