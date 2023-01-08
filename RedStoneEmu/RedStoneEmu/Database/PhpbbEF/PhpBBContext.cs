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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(string.Format("Host={0};User Id={1};Password={2};Database={3};Port={4}",
                       RedStoneApp.LoginServer.Config.DatabaseHost,
                       RedStoneApp.LoginServer.Config.DatabaseUsername,
                       RedStoneApp.LoginServer.Config.DatabasePassword,
                       "phpbb",
                       RedStoneApp.LoginServer.Config.DatabasePort));
        }
    }
}
