using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Class_Lib;

public class BackupJob
{
    [JsonPropertyName("id")]
    public int Id { get; set; } = 0;
    [JsonPropertyName("sources")]
    public List<string> Sources { get; set; }
    [JsonPropertyName("targets")]
    public List<string> Targets { get; set; }
    [JsonPropertyName("method")]
    public BackupMethod Method { get; set; }
    [JsonPropertyName("timing")]
    public string Timing { get; set; }
    [JsonPropertyName("retention")]
    public BackupRetention BackupRetention { get; set; }
}
