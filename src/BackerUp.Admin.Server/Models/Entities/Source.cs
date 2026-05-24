namespace BackerUp.Admin.Server.Models.Entities
{
    public class Source
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public string Path { get; set; } = string.Empty;

        public BackupJob Job { get; set; } = null!;
    }
}
