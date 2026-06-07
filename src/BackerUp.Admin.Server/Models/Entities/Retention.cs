using System.ComponentModel.DataAnnotations.Schema;

namespace BackerUp.Admin.Server.Models.Entities
{
    public class Retention
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("job_id")]
        public int JobId { get; set; }
        [Column("count")]
        public int Count { get; set; }
        [Column("size")]
        public int Size { get; set; }

        public BackupJob Job { get; set; } = null!;
    }
}
