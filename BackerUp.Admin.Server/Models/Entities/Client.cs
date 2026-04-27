namespace BackerUp.Admin.Server.Models.Entities
{
    public class Client
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<JobClient> JobClients { get; set; } = new List<JobClient>();
    }
}
