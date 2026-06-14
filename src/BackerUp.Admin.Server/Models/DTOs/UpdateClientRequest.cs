namespace BackerUp.Admin.Server.Models.DTOs
{
    public class UpdateClientRequest
    {
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<int>? JobIds { get; set; }
    }
}