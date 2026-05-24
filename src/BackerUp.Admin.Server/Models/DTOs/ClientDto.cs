namespace BackerUp.Admin.Server.Models.DTOs
{
    public class ClientResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateClientRequest
    {
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class UpdateClientRequest
    {
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
