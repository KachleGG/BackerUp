// JobsClients linking table kept as pure relational mapping only. We will no longer expose a dedicated API for it.
using System.ComponentModel.DataAnnotations.Schema;

namespace BackerUp.Admin.Server.Models.Entities
{
    public class JobClient
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("job_id")]
        public int JobId { get; set; }
        [Column("client_id")]
        public Guid ClientId { get; set; }
        [Column("is_active")]
        public bool IsActive { get; set; }

        public BackupJob Job { get; set; } = null!;
        public Client Client { get; set; } = null!;
        public ICollection<Log> Logs { get; set; } = new List<Log>();
    }
}
