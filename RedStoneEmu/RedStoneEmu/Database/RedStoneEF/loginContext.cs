using Microsoft.EntityFrameworkCore;
using RedStoneLib.Model;
using RedStoneLib.Packets.RSPacket.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneEmu.Database.RedStoneEF
{
    /// <summary>
    /// ログイン用のContext
    /// </summary>
    public class loginContext: DbContext
    {
        /// <summary>
        /// ゲームサーバーの情報
        /// </summary>
        public DbSet<GameServerInfo> GameServerInfos { get; set; }

        /// <summary>
        /// ログイン履歴
        /// </summary>
        public DbSet<LoginLog> LoginLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(string.Format("Host={0};User Id={1};Password={2};Database={3};Port={4}",
                RedStoneApp.LoginServer.Config.DatabaseHost,
                RedStoneApp.LoginServer.Config.DatabaseUsername,
                RedStoneApp.LoginServer.Config.DatabasePassword,
                RedStoneApp.LoginServer.Config.DatabaseName,
                RedStoneApp.LoginServer.Config.DatabasePort));
        }

        /// <summary>
        /// ここでuniqueなどを設定
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GameServerInfo>()
                 .HasIndex(u => u.ServerId)
                 .IsUnique();
            modelBuilder.Entity<GameServerInfo>()
                 .HasIndex(u => u.ServerName)
                 .IsUnique();
            modelBuilder.Entity<GameServerInfo>()
                 .HasIndex(u => u.Host)
                 .IsUnique();
            modelBuilder.Entity<LoginLog>()
                 .HasIndex(u => u.Datetime)
                 .IsUnique();
        }
    }
}
