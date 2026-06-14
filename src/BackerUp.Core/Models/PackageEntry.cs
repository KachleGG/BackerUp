namespace BackerUp.Core.Models
{
    public class PackageEntry
    {
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedUtc { get; set; }
        public int SnapshotCount { get; set; } = 0;
    }
}
