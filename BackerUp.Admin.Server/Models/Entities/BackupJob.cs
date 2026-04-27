using BackerUp.Admin.Server.Models.Enums;

namespace BackerUp.Admin.Server.Models.Entities
{
    public class BackupJob
    {
        public int Id { get; set; }
        public BackupMethod Method { get; set; }
        public string Timing { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public ICollection<Source> Sources { get; set; } = new List<Source>();
        public ICollection<Target> Targets { get; set; } = new List<Target>();
        public Retention? Retention { get; set; }
        public ICollection<JobClient> JobClients { get; set; } = new List<JobClient>();
    }
}
