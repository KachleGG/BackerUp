using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Class_Lib;

public enum BackupMethod
{
    [JsonPropertyName("full")]
    Full,
    [JsonPropertyName("differential")]
    Differential,
    [JsonPropertyName("incremental")]
    Incremental
}
