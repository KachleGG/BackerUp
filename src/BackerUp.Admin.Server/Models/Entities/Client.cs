using System.ComponentModel.DataAnnotations.Schema;

namespace BackerUp.Admin.Server.Models.Entities
{
    public class Client
    {
        [Column("id")]
        public Guid Id { get; set; }
        [Column("name")]
        public string Name { get; set; } = string.Empty;
        [Column("is_active")]
        public bool IsActive { get; set; }
        [Column("is_approved")]
        public bool IsApproved { get; set; }
        [Column("last_healthcheck_at")]
        public DateTime? LastHealthCheck { get; set; }
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        public ICollection<JobClient> JobClients { get; set; } = new List<JobClient>();
    }
}
