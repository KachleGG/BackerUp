using BackerUp.Admin.Server.Models.Enums;

namespace BackerUp.Admin.Server.Models.DTOs
{
    public class BackupJobResponse
    {
        public int Id { get; set; }
        public BackupMethod Method { get; set; }
        public string Timing { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<string> Sources { get; set; } = new();
        public List<string> Targets { get; set; } = new();
        public RetentionDto? Retention { get; set; }
    }

    public class CreateBackupJobRequest
    {
        public BackupMethod Method { get; set; }
        public string Timing { get; set; } = string.Empty;
        public List<string> Sources { get; set; } = new();
        public List<string> Targets { get; set; } = new();
        public RetentionDto? Retention { get; set; }
    }

    public class UpdateBackupJobRequest
    {
        public BackupMethod Method { get; set; }
        public string Timing { get; set; } = string.Empty;
        public List<string> Sources { get; set; } = new();
        public List<string> Targets { get; set; } = new();
        public RetentionDto? Retention { get; set; }
    }

    public class RetentionDto
    {
        public int Count { get; set; }
        public int Size { get; set; }
    }
}
