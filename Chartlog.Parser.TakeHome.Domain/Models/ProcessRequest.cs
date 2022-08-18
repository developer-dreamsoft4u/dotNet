using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chartlog.Parser.TakeHome.Domain.Models
{
    public class ProcessRequest
    {
        public Guid? UserId { get; set; }
        public Guid? SessionId { get; set; }
        public string? Integration { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}
