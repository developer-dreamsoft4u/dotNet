

using Chartlog.Parser.TakeHome.Domain.Models;
using Serilog;

namespace Chartlog.Parser.TakeHome.Domain.Infrastructure.Links
{
    public class FileDelimiterValidationLink<T> : Link where T : IntegrationType
    {
        private readonly IHeaderValidator<T> _headerValidator;
        private readonly IFileDelimiterValidator _fileDelimiterValidator;
        private readonly Lazy<T> Type = new Lazy<T>(Activator.CreateInstance<T>);
        public FileDelimiterValidationLink(Link decorator,
            ILogger log,
            IHeaderValidator<T> headerValidator,
            IFileDelimiterValidator fileDelimiterValidator) : base(decorator, log)
        {
            _headerValidator = headerValidator;
            _fileDelimiterValidator = fileDelimiterValidator;
        }

        protected override async Task<ProcessRequest> HandleAsync(ProcessRequest request)
        {
            var content = TypeCheck<ProcessContentRequest>(request);
            var targetIndex = _headerValidator.ValidateColumnHeaders(content.FileContent);

            _fileDelimiterValidator.Execute(content.FileContent, targetIndex,
                Type.Value.RequiredHeaders, 
                Type.Value.Delimiter);

            //if nothing blows up then the delimiter has been validated
            return await TryRunDecoratorOrReturnAsync(content);
        }

     
    }
}
