

using Chartlog.Parser.TakeHome.Domain.Models;
using Serilog;

namespace Chartlog.Parser.TakeHome.Domain.Infrastructure.Links
{
    public class ContentNormalizationLink : Link
    {
        private readonly IContentNormalizer _contentNormalizer;

        public ContentNormalizationLink(Link decorator, ILogger log, IContentNormalizer contentNormalizer) : base(decorator, log)
        {
            _contentNormalizer = contentNormalizer;
        }

        protected override async Task<ProcessRequest> HandleAsync(ProcessRequest request)
        {
            var contentRequest = TypeCheck<ProcessContentRequest>(request);

            if (string.IsNullOrWhiteSpace(contentRequest.FileContent))
                throw new FileProcessorException(ErrorTypeEnum.EmptyFile, contentRequest.Encoding.EncodingName);

            contentRequest.FileContent = _contentNormalizer.Normalize(contentRequest.FileContent);

            return await TryRunDecoratorOrReturnAsync(contentRequest);
        }
    }
}