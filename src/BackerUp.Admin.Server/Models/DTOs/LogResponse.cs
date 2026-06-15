using BackerUp.Admin.Server.Models.Enums;

namespace BackerUp.Admin.Server.Models.DTOs
{
    public class LogResponse
    {
        public int Id { get; set; }
        public int? JobsClientsId { get; set; }
        public Level Level { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}