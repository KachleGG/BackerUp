namespace BackerUp.Client.Models;

public class ClientResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsApproved { get; set; }
    public DateTime? LastHealthCheck { get; set; }
    public bool IsOnline { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<int>? JobIds { get; set; }
}