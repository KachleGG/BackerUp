namespace BackerUp.Admin.Server.Models.Entities
{
    public class JobClient
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public Guid ClientId { get; set; }
        public bool IsActive { get; set; }

        public BackupJob Job { get; set; } = null!;
        public Client Client { get; set; } = null!;
        public ICollection<Log> Logs { get; set; } = new List<Log>();
    }
}
