

using Chartlog.Parser.TakeHome.Domain.Models;
using Serilog;

namespace Chartlog.Parser.TakeHome.Domain.Infrastructure.Links
{
    public class FileExtensionValidationLink<T> : Link where T : IntegrationType
    {
        private readonly Lazy<IntegrationType> Type = new Lazy<IntegrationType>(Activator.CreateInstance<T>); 

        public FileExtensionValidationLink(Link decorator, ILogger log) : base(decorator, log)
        {
        }

        protected override async Task<ProcessRequest> HandleAsync(ProcessRequest request)
        {
            var fileRequest = TypeCheck<ProcessFileRequest>(request);

            var extension = fileRequest.FileExtension?.ToLower().Trim();
            var supportedExtensions = Type.Value.FileExtensionWhiteList;

            if (extension == null || !supportedExtensions.Contains(extension))
                throw new FileProcessorException(ErrorTypeEnum.WrongFileExtension, $"Uploads must have a file extension of {string.Join(",", Type.Value.FileExtensionWhiteList)}");

            return await TryRunDecoratorOrReturnAsync(fileRequest);
        }
    }
}
