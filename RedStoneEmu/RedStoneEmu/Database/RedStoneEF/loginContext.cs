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
    public class loginContext : DbContext
    {
        /// <summary>
        /// ゲームサーバーの情報
        /// </summary>
        public DbSet<GameServerInfo> GameServerInfos { get; set; }

        /// <summary>
        /// ログイン履歴
        /// </summary>
        public DbSet<LoginLog> LoginLogs { get; set; }

        public loginContext(DbContextOptions<loginContext> options) : base(options)
        {

        }

        public loginContext()
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            ServerConfig config = new ServerConfig();
            var connectionString = string.Format("Host={0};User Id={1};Password={2};Database={3};Port={4}",
                            config.DatabaseHost,
                            config.DatabaseUsername,
                            config.DatabasePassword,
                            config.DatabaseName,
                            config.DatabasePort);
            optionsBuilder.UseNpgsql(connectionString);
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
