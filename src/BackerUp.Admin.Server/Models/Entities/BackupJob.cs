using BackerUp.Admin.Server.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackerUp.Admin.Server.Models.Entities
{
    public class BackupJob
    {
        [Column("id")]
        public int Id { get; set; }

        // store method as string in the database (ENUM) but expose as enum in code
        [Column("method")]
        public string MethodRaw { get; set; } = BackupMethod.Full.ToString();

        [NotMapped]
        public BackupMethod Method
        {
            get
            {
                if (Enum.TryParse<BackupMethod>(MethodRaw, out var val)) return val;
                // fallback: try numeric parse
                if (int.TryParse(MethodRaw, out var idx)) return (BackupMethod)idx;
                return BackupMethod.Full;
            }
            set => MethodRaw = value.ToString();
        }

        [Column("timing")]
        public string Timing { get; set; } = string.Empty;
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        public ICollection<Source> Sources { get; set; } = new List<Source>();
        public ICollection<Target> Targets { get; set; } = new List<Target>();
        public Retention? Retention { get; set; }
        public ICollection<JobClient> JobClients { get; set; } = new List<JobClient>();
    }
}
