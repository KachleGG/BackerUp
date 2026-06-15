using BackerUp.Admin.Server.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackerUp.Admin.Server.Models.Entities
{
    public class Log
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("jobs_clients_id")]
        public int? JobsClientsId { get; set; }
        [Column("level")]
        public Level Level { get; set; }
        [Column("description")]
        public string Description { get; set; } = string.Empty;
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        public JobClient? JobClient { get; set; }
    }
}
