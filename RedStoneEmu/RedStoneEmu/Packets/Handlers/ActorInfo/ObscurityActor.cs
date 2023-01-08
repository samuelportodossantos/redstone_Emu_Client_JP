using RedStoneLib.Model;
using RedStoneLib.Model.Base;
using RedStoneLib.Packets;
using RedStoneLib.Packets.RSPacket.ActorGenerator;
using RedStoneLib.Packets.RSPacket.ActorInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RedStoneEmu.Packets.Handlers.ActorInfo
{
    /// <summary>
    /// 不明瞭なactorの問い合わせ
    /// ・minimumで配置したactorの問い合わせ
    /// </summary>
    [PacketHandlerAttr(0x1099)]
    class ObscurityActor : PacketHandler
    {
        public override void HandlePacket(Client context, PacketReader reader, uint size)
        {
            //charID
            var charIDs = reader.Reads<ushort>((int)(size / 2));

            //リスポーンされてないmonsterはregen
            //残りはvery simple actor info
            var actors = context.GetMapActors();
            List<Actor> ActorList = new List<Actor>();
            List<Monster> SpawnMonsters = new List<Monster>();
            foreach (var charID in charIDs)
            {
                var target = actors.SingleOrDefault(t => t.CharID == charID);
                if (target == null)
                    break;
                if(target is Monster && !(target as Monster).Spawned)
                {
                    SpawnMonsters.Add(target as Monster);
                }
                else
                {
                    ActorList.Add(target);
                }
            }

            //spawn
            if (SpawnMonsters.Count > 0)
            {
                for (int i = 0; i < SpawnMonsters.Count; i++)
                {
                    var monster = SpawnMonsters[i];
                    context.SendPacket(new RegenMonsterPacket(ref monster));
                }
            }

            //vsa送信
            if (ActorList.Count > 0)
            {
                context.SendPacket(new VerySimpleActorInfoListPacket(ActorList.ToList()));
            }
        }
    }
}
