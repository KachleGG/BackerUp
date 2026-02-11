using System.Text.Json.Serialization;

namespace BackerUp.Core;

public class BackupRetention
{
    [JsonPropertyName("count")]
    public int Count { get; set; }
    [JsonPropertyName("size")]
    public int Size { get; set; } = 1;
}
