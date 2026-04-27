namespace BackerUp.Admin.Server.Models.Entities
{
    public class Retention
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public int Count { get; set; }
        public int Size { get; set; }

        public BackupJob Job { get; set; } = null!;
    }
}
