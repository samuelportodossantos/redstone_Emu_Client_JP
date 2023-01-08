using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedStoneLib.Model;
using RedStoneLib.Packets.RSPacket.KarmaPacket;

namespace RedStoneLib.Karmas.Commands
{
    /// <summary>
    /// 文章のジャンプ
    /// </summary>
    [KarmaItemAttr(0x000, 1)]
    class SkipSpeech : KarmaItemExecuteService
    {
        public override void HandleKarmaItem(Player player, uint[] value, ushort npcCharID, SendPacketDelegate sendPacket)
        {
            int progress = (int)value[0];
            player.PlayerEvent.Progress = progress;
            if (player.PlayerEvent.Events[progress].AutoStart)
            {
                //自動実行
                player.PlayerEvent.Events[progress].Execute(player, sendPacket, npcCharID);
            }
            else
            {
                //表示
                sendPacket(new ComplexSpeechPacket(npcCharID, player.PlayerEvent.Events[progress]));
            }
        }
    }

    /// <summary>
    /// 閉じる
    /// </summary>
    [KarmaItemAttr(0x001)]
    class CloseDialog : KarmaItemExecuteService
    {
        public override void HandleKarmaItem(Player player, uint[] value, ushort npcCharID, SendPacketDelegate sendPacket)
        {
            sendPacket(new EndDialogPacket());
        }
    }

    /// <summary>
    /// 店を開く
    /// </summary>
    [KarmaItemAttr(0x002)]
    class OpenDhop : KarmaItemExecuteService
    {
        public override void HandleKarmaItem(Player player, uint[] value, ushort npcCharID, SendPacketDelegate sendPacket)
        {
            Map targetmap = Map.AllMaps[player.MapSerial];
            sendPacket(new OpenShopPacket(targetmap.Shops[value[0]]));
        }
    }
}
