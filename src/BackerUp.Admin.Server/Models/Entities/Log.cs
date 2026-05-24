using BackerUp.Admin.Server.Models.Enums;

namespace BackerUp.Admin.Server.Models.Entities
{
    public class Log
    {
        public int Id { get; set; }
        public int JobsClientsId { get; set; }
        public Level Level { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public JobClient JobClient { get; set; } = null!;
    }
}
