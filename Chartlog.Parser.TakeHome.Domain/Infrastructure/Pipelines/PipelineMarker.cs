using Chartlog.Parser.TakeHome.Domain.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chartlog.Parser.TakeHome.Domain.Infrastructure.Pipelines
{
    public class PipelineMarker<T> : PipelineMarker where T : IntegrationType
    {
        public override string IntegrationType { get; }

        public PipelineMarker(Link link, ILogger log) : base(link, log)
        {
            IntegrationType = Activator.CreateInstance<T>().IntegrationName;
        }

        protected override async Task<ProcessRequest> HandleAsync(ProcessRequest request)
        {
            try
            {
                var result = await _decorator.ExecuteAsync(request).ConfigureAwait(false);

                result.EndTime = DateTime.UtcNow;
                //try to dispose stream if there is one
                try
                {
                    var streamRequest = result as IHasStream;
                    streamRequest?.Stream.Dispose();
                }
                catch (Exception e)
                {
                    _log
                        .Error("Failed to dispose stream", e);
                }

                return result;
            }
            catch (FileProcessorException e)
            {
                _log
                    .Error(e.Message, e);

                throw;
            }

        }
    }

    public abstract class PipelineMarker : Link
    {
        public abstract string IntegrationType { get; }
        protected PipelineMarker(Link decorator, ILogger log) : base(decorator, log)
        {
        }
    }
}
