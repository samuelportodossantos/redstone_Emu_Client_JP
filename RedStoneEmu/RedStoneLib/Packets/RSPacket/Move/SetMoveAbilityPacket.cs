using RedStoneLib.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Packets.RSPacket.Move
{
    public class SetMoveAbility : Packet
    {
        Player player;

        public SetMoveAbility(Player player)
        {
            this.player = player;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write((ushort)((ushort)(player.IsRun ? 1 : 0) | (ushort)(player.MoveSpeed << 1)));

            packetSize = (ushort)writer.Length;
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x112D);
        }
    }
}
