using System.Text.Json.Serialization;

namespace Class_Lib;

public class BackupRetention
{
    [JsonPropertyName("count")]
    public int Count { get; set; }
    [JsonPropertyName("size")]
    public int Size { get; set; } = 1;
}
