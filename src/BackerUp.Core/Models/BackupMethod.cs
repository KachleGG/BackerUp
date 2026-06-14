using System.Text.Json.Serialization;

namespace BackerUp.Core.Models;

public enum BackupMethod
{
    [JsonPropertyName("full")]
    Full,
    [JsonPropertyName("differential")]
    Differential,
    [JsonPropertyName("incremental")]
    Incremental
}
