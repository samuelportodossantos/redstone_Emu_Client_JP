using RedStoneLib.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Packets.RSPacket.Login
{
    /// <summary>
    /// ログイン時に送信するアバターリスト
    /// </summary>
    public class AvatarListPacket : Packet
    {
        private ushort Packet_security_code;

        private (Player player, string ip)[] Avatars;
        private int ServerId;

        public AvatarListPacket((Player player, string ip)[] avatars, int serverId, ushort packet_security_code)
        {
            Avatars = avatars;
            ServerId = serverId;
            Packet_security_code = packet_security_code;
        }

        public override byte[] Build()
        {

            var writer = new PacketWriter();

            writer.Write(Packet_security_code);
            
            for (int i = 0; i < 6; i++)
            {
                if (i < Avatars.Length)
                {
                    writer.Write((ushort)i);
                    writer.Write(Avatars[i].player.WriteAvatar(Avatars[i].ip));
                }
                else
                {
                    writer.Write((ushort)0xFFFF);
                    writer.WriteBytes(0, 0x2E);
                }
            }

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1103);
        }
    }
}
