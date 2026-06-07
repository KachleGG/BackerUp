namespace BackerUp.Admin.Server.Models.DTOs
{
    public class ClientResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<int> JobIds { get; set; } = new();
    }

    public class CreateClientRequest
    {
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<int>? JobIds { get; set; }
    }

    public class UpdateClientRequest
    {
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<int>? JobIds { get; set; }
    }
}
