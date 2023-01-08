using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedStoneEmu.Database.SMFEF
{
    public partial class smfContext : DbContext
    {
        public virtual DbSet<Members> smf_members { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(string.Format("Host={0};User Id={1};Password={2};Database={3};Port={4}",
                       RedStoneApp.LoginServer.Config.DatabaseHost,
                       RedStoneApp.LoginServer.Config.DatabaseUsername,
                       RedStoneApp.LoginServer.Config.DatabasePassword,
                       RedStoneApp.LoginServer.Config.DatabaseName,
                       RedStoneApp.LoginServer.Config.DatabasePort));
        }
    }
}
