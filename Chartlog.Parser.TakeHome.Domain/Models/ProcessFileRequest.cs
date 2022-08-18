using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chartlog.Parser.TakeHome.Domain.Models
{
    public class ProcessFileRequest : ProcessRequest, IHasStream
    {
        public Stream? Stream { get; set; }
        public string? FileExtension { get; set; }
        public string? FileName { get; set; }
        public Encoding? Encoding { get; set; } = null;
    }

    public interface IHasStream
    {
        Stream? Stream { get; set; }
    }
}
