using RedStoneLib.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Packets.RSPacket.ActorInfo
{
    /// <summary>
    /// プレイヤー情報（自分）を返す
    /// </summary>
    public class PlayerInfoPacket : Packet
    {
        Player.PlayerInfo PlayerInfo;

        public PlayerInfoPacket(Player.PlayerInfo playerInfo)
        {
            PlayerInfo = playerInfo;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write((ushort)0);//junk
            writer.WriteStruct(PlayerInfo);
            
            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x123D);
        }
    }
}
