using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KappaCopy.App
{
    public sealed class ShellClipboardData
    {
        public List<string> Paths { get; set; } = [];

        public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;
    }
}
