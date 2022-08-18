

using Chartlog.Parser.TakeHome.Domain.Models;
using Serilog.Core;

namespace Chartlog.Parser.TakeHome.Domain.Infrastructure.Links
{
    public class NoOpLink : Link
    {
        public NoOpLink() : base (default, Logger.None)
        {

        }

        protected override async Task<ProcessRequest> HandleAsync(ProcessRequest request)
        {
            return request;
        }
    }
}
