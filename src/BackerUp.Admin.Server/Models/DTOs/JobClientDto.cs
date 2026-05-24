namespace BackerUp.Admin.Server.Models.DTOs
{
    public class JobClientResponse
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public Guid ClientId { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateJobClientRequest
    {
        public int JobId { get; set; }
        public Guid ClientId { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateJobClientRequest
    {
        public bool IsActive { get; set; }
    }
}
