using RedStoneLib.Model;
using RedStoneLib.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Packets.RSPacket.ActorInfo
{
    /// <summary>
    /// Actorリスト情報送信
    /// 送信後したキャラクターは新規登録される
    /// </summary>
    public class VerySimpleActorInfoListPacket : SimpleActorInfoListPacket
    {

        public VerySimpleActorInfoListPacket(List<Actor> actors)
            : base(actors) { }

        /// <summary>
        /// フィールド変数の準備
        /// </summary>
        /// <param name="actors"></param>
        protected override void SetPlayerInfoBytes(List<Actor> actors)
        {
            PlayerInfoBytes = actors.SelectMany(actor => VerySimpleActorInfo.ToBytes(actor)).ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1215);
        }
    }
}
