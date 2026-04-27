using BackerUp.Admin.Server.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BackerUp.Admin.Server.Data
{
    public class BackerUpDbContext : DbContext
    {
        public BackerUpDbContext(DbContextOptions<BackerUpDbContext> options) : base(options)
        {
        }

        public DbSet<BackupJob> BackupJobs { get; set; }
        public DbSet<Source> Sources { get; set; }
        public DbSet<Target> Targets { get; set; }
        public DbSet<Retention> Retentions { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<JobClient> JobsClients { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Log> Logs { get; set; }
    }
}
