using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackerUp.Core.DTOs
{
    public class CreateUpdateBackupJobRequest
    {
        public string Method { get; set; } = string.Empty;
        public string Timing { get; set; } = string.Empty;
        public List<string> Sources { get; set; } = new();
        public List<string> Targets { get; set; } = new();
        public RetentionDto? Retention { get; set; }
    }
}
