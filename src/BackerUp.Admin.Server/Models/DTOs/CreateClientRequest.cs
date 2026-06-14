namespace BackerUp.Admin.Server.Models.DTOs
{
    public class CreateClientRequest
    {
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsApproved { get; set; } = false;
        public List<int>? JobIds { get; set; }
    }
}