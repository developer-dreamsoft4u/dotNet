

using Chartlog.Parser.TakeHome.Domain.Models;
using Serilog;

namespace Chartlog.Parser.TakeHome.Domain.Infrastructure.Links
{
    public class StreamConversionLink : Link
    {
        private readonly IStreamService _streamService;

        public StreamConversionLink(Link decorator, ILogger log, IStreamService streamService) : base(decorator, log)
        {
            _streamService = streamService;
        }

        protected override async Task<ProcessRequest> HandleAsync(ProcessRequest request)
        {
            var streamRequest = TypeCheck<ProcessFileRequest>(request);

            if (streamRequest.Encoding == null)
                throw new ArgumentException(nameof(request), $"The parameter passed into {GetType().Name} does not have the `Encoding` property set. This class needs the encoding in order to convert the stream to text");

            var content = _streamService.ConvertToText(streamRequest.Stream, streamRequest.Encoding);

            return await TryRunDecoratorOrReturnAsync(new ProcessContentRequest(streamRequest, content))
                .ConfigureAwait(false);
        }
    }
}