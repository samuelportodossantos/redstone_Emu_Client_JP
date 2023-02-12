﻿using RedStoneLib.Model;
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
    public class VerySimpleActorInfoListPacket : Packet
    {

        protected ushort Count;
        protected byte[] PlayerInfoBytes;

        public VerySimpleActorInfoListPacket(List<Actor> actors)
        {
            Count = (ushort)actors.Count;
            PlayerInfoBytes = actors.SelectMany(actor => VerySimpleActorInfo.ToBytes(actor)).ToArray();
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
            return new PacketHeader(packetSize, 0x1215);
        }
    }
}
