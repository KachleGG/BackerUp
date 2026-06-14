using System.Text.Json.Serialization;

namespace BackerUp.Client.Models;

public class RetentionDto
{
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("size")]
    public int Size { get; set; }
}