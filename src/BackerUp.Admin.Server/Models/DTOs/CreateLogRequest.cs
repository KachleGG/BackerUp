using BackerUp.Admin.Server.Models.Enums;

namespace BackerUp.Admin.Server.Models.DTOs
{
    public class CreateLogRequest
    {
        public int? JobsClientsId { get; set; }
        public Level Level { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}