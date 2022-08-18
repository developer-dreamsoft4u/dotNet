

using Chartlog.Parser.TakeHome.Domain.Models;
using Serilog;
using System.Text;

namespace Chartlog.Parser.TakeHome.Domain.Infrastructure.Links
{
    public class EncodingValidationLink : Link
    {
       

        public EncodingValidationLink(Link decorator, ILogger log) : base(decorator, log)
        {
        }

        protected override async Task<ProcessRequest> HandleAsync(ProcessRequest request)
        {
            var streamRequest = TypeCheck<ProcessFileRequest>(request);

            //var encoding = _validator.Execute(streamRequest.Stream);
            streamRequest.Encoding = Encoding.UTF8;

            _log
                .Debug("Encoding validation success");

            return await TryRunDecoratorOrReturnAsync(streamRequest).ConfigureAwait(false);
        }
    }
}