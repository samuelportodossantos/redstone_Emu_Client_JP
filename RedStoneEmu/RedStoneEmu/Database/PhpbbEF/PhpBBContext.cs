using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneEmu.Database.PhpbbEF
{
    public class PhpBBContext : DbContext
    {
        public virtual DbSet<Users> phpbb_users { get; set; }
        public PhpBBContext(DbContextOptions<PhpBBContext> options) : base(options)
        {
        }

        public PhpBBContext()
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
    }
}
