using BackerUp.Core.Models;
using System.Text.Json.Serialization;

namespace BackerUp.Client.Models;

public class BackupJobResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("method")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BackupMethod Method { get; set; }

    [JsonPropertyName("timing")]
    public string Timing { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("sources")]
    public List<string> Sources { get; set; } = new();

    [JsonPropertyName("targets")]
    public List<string> Targets { get; set; } = new();

    [JsonPropertyName("retention")]
    public RetentionDto? Retention { get; set; }
}
