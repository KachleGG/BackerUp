using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BackerUp.Core.Models;

public class BackupJob
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("sources")]
    public List<string> Sources { get; set; } = new List<string>();

    [JsonPropertyName("targets")]
    public List<string> Targets { get; set; } = new List<string>();

    [JsonPropertyName("method")]
    public BackupMethod Method { get; set; } = BackupMethod.Full;

    [JsonPropertyName("timing")]

    public string Timing { get; set; } = "* */0 * * *";

    [JsonPropertyName("retention")]
    public BackupRetention BackupRetention { get; set; } = new BackupRetention();
}
