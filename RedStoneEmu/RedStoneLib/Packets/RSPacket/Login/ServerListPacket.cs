﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneLib.Packets.RSPacket.Login
{
    /// <summary>
    /// サーバーリスト
    /// </summary>
    public class ServerListPacket : Packet
    {
        ushort PacketSize = 0;
        private ushort Packet_security_code;
        List<GameServerInfo> GameServerInfos;

        public ServerListPacket(List<GameServerInfo> gameServerInfos, ushort packet_security_code)
        {
            Packet_security_code = packet_security_code;
            GameServerInfos = gameServerInfos;
        }

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            // server count
            writer.Write((ushort)1);

            // PacketSecurityCode
            writer.Write(Packet_security_code);
            
            for (int i = 0; i < GameServerInfos.Count(); i++)
            {
                writer.WriteSjis(GameServerInfos[i].ServerName, 0x20);

                writer.Write((ushort)GameServerInfos[i].ServerType);// is TestServer
                writer.Write((uint)i);// server cnum
            }

            PacketSize = (ushort)writer.ToArray().Count();
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(PacketSize, 0x1102);
        }
    }

    /// <summary>
    /// ゲームサーバーのタイプ
    /// </summary>
    public enum GameServerType : int
    {
        Normal = 0,
        Test = 1
    }

    [Table("game_server_info")]
    public class GameServerInfo
    {
        public int Id { get; set; }
        public bool Enable { get; set; }
        public int ServerId { get; set; }
        public string? ServerName { get; set; }

        public string? Host { get; set; }
        public GameServerType ServerType { get; set; }
    }
}
