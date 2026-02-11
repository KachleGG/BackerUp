using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackerUp.Core
{
    public class PackageEntry
    {
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedUtc { get; set; }
        public int SnapshotCount { get; set; } = 0;
    }
}
