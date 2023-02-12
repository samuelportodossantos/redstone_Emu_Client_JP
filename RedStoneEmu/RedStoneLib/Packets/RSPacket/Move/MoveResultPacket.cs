using RedStoneLib.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Packets.RSPacket.Move
{
    /// <summary>
    /// 移動結果のパケット
    /// MaxHP, NowHP, NowCP, Move_Speed, isRunが必要
    /// </summary>
    public class MoveResultPacket : Packet
    {
        public MoveResultPacket(Player player)
        {
            MaxHP = player.MaxHP;
            NowHP = player.NowHP;
#if DEBUG
            nowCP = (short)player.MaxCP;
#else
            nowCP = (short)player.NowCP;
#endif
            MoveSpeed = player.MoveSpeed;
            is_run = player.IsRun ? (byte)1 : (byte)0;
        }

        uint MaxHP, NowHP;
        ushort MoveSpeed;
        short nowCP;
        byte is_run;//ダッシュフラグ

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            if (is_run == 0)
            {
                MoveSpeed /= 2;
            }
            writer.Write((ushort)((ushort)(is_run & 1) | (ushort)(MoveSpeed << 1)));

            writer.Write((ushort)(NowHP*10000.0 / MaxHP));//nowHP
            writer.Write(nowCP);//nowCP

            packetSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(packetSize, 0x1129);
        }
    }
}
