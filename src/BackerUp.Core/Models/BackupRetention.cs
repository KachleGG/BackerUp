using System.Text.Json.Serialization;

namespace BackerUp.Core.Models;

public class BackupRetention
{
    [JsonPropertyName("count")]
    public int Count { get; set; } = 3;

    [JsonPropertyName("size")]
    public int Size { get; set; } = 1;
}
