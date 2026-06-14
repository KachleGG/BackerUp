using BackerUp.Admin.Server.Models.Enums;

namespace BackerUp.Admin.Server.Models.DTOs
{
    public class UpdateBackupJobRequest
    {
        public BackupMethod Method { get; set; }
        public string Timing { get; set; } = string.Empty;
        public List<string> Sources { get; set; } = new();
        public List<string> Targets { get; set; } = new();
        public RetentionDto? Retention { get; set; }
    }
}