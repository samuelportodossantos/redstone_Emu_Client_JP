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
    public class SimpleActorInfoListPacket : Packet
    {
        protected ushort Count;
        protected byte[] PlayerInfoBytes;

        public SimpleActorInfoListPacket(List<Actor> actors)
        {
            Count = (ushort)actors.Count;
            SetPlayerInfoBytes(actors);
        }

        /// <summary>
        /// プレイヤ情報の準備
        /// </summary>
        /// <param name="actors"></param>
        protected virtual void SetPlayerInfoBytes(List<Actor> actors)
        {
            PlayerInfoBytes = actors.SelectMany(actor => SimpleActorInfo.ToBytes(actor)).ToArray();
        }
        
        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write(Count);

            //構造体書き込み
            writer.Write(PlayerInfoBytes);

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1179);
        }
    }
}
