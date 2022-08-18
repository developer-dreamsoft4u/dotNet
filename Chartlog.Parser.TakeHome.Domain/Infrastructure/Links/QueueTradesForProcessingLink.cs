

using Chartlog.Parser.TakeHome.Domain.Models;
using Serilog;

namespace Chartlog.Parser.TakeHome.Domain.Infrastructure.Links
{
    public class QueueTradesForProcessingLink : Link
    {
        

        public QueueTradesForProcessingLink(Link decorator,
ILogger log) : base(decorator, log)
        {

        }

        protected override async Task<ProcessRequest> HandleAsync(ProcessRequest request)
        {
            _log.Information("Queueing trades for processing");

            return await TryRunDecoratorOrReturnAsync(request);
        }

    }
}
