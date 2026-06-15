using BackerUp.Admin.Server.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BackerUp.Admin.Server.Data
{
    public class BackerUpDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL("server=mysqlstudenti.litv.sssvt.cz;database=3b1_kachlikmarek_db2;user=kachlikmarek;password=123456");
        }

        public DbSet<BackupJob> BackupJobs { get; set; }
        public DbSet<Source> Sources { get; set; }
        public DbSet<Target> Targets { get; set; }
        public DbSet<Retention> Retentions { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<JobClient> JobsClients { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Log> Logs { get; set; }
    }
}
