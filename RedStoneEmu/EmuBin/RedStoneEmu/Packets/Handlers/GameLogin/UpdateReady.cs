using RedStoneLib;
using RedStoneLib.Model;
using RedStoneLib.Model.Base;
using RedStoneLib.Packets;
using RedStoneLib.Packets.RSPacket.ActorInfo;
using RedStoneLib.Packets.RSPacket.BCS;
using RedStoneLib.Packets.RSPacket.Move;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneEmu.Packets.Handlers.GameLogin
{
    /// <summary>
    /// アップデート準備完了
    /// GameLoginHandler: 5th
    /// </summary>
    [PacketHandlerAttr(0x1024)]
    class UpdateReady : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            //stop
            context.SendPacket(new StopPacket(context.User));

            //移動結果
            context.SendPacket(new MoveFaildMessagePacket(MoveFaildMessagePacket.MoveFaildMessage.OnlySetPos,
                context.User.PosX, context.User.PosY, context.User.MoveSpeed, context.User.IsRun));

            //ブロードキャストサーバー情報送信（暫定）
            context.SendPacket(new BCSInfoPacket(IPAddressProvider.LocalIP, 56621));

            //自分以外フラグ
            Func<Actor, bool> isNotMe = t => !(t is Player && t.Name == context.User.Name);

            //minimumActorInfo リスポーンしたモンスターは出さない
            context.SendPacket(new MinimumActorInfoList(context.GetMapActors(context.User.CharID).Where(t => isNotMe(t) && !(t is Monster && (t as Monster).Respawned)).ToList()));

            //npc verysimple リスポーンしてないモンスターは出さない
            context.SendPacket(new VerySimpleActorInfoListPacket(context.GetMapActors().Where(t => isNotMe(t) && !(t is Monster && !(t as Monster).Respawned)).ToList()));

            //アップデート完了
            ((GameClient)context).IsUpdateReady = true;

            //モンスターテスト
            /*Map targetMap = Map.AllMaps[context.User.MapSerial];
            var targetMob = context.GetMapActors().FirstOrDefault(t => t is Monster && t.Name=="リビングデッド") as Monster;
            if (targetMob != null)
            {
                targetMob.CharID = targetMap.NewCharIDs.First();
                targetMob.PosX = context.User.PosX;
                targetMob.PosY = context.User.PosY;
                context.SendPacket(new TestSend<VerySimpleMonster>(new List<VerySimpleMonster> { new VerySimpleMonster(targetMob) }));
            }*/
        }
    }

    class TestSend<T> : RedStoneLib.Packets.Packet
    {
        protected ushort Count;
        protected byte[] PlayerInfoBytes;

        public TestSend(List<T> actors)
        {
            Count = (ushort)actors.Count;
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
            return new PacketHeader(packetSize, 0x1215);
        }
    }
}
