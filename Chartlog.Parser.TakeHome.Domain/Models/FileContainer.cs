using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chartlog.Parser.TakeHome.Domain.Models
{
    public class FileContainer
    {
        public string? Content { get; set; }
        public byte[]? Binaries { get; set; }
    }
}
