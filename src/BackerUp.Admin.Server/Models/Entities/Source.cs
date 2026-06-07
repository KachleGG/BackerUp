using System.ComponentModel.DataAnnotations.Schema;

namespace BackerUp.Admin.Server.Models.Entities
{
    public class Source
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("job_id")]
        public int JobId { get; set; }
        [Column("path")]
        public string Path { get; set; } = string.Empty;

        public BackupJob Job { get; set; } = null!;
    }
}
