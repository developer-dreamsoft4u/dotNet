

using Chartlog.Parser.TakeHome.Domain.Models;
using Serilog;

namespace Chartlog.Parser.TakeHome.Domain.Infrastructure.Links
{
    public class RequiredColumnHeadersValidationLink<T> : Link where T : IntegrationType
    {
        private readonly IHeaderValidator<T> _columnHeaderValidator;
        private readonly IEnumerable<IHeaderValidator> _allValidators;
        private readonly Lazy<T> _type = new Lazy<T>(Activator.CreateInstance<T>);

        public RequiredColumnHeadersValidationLink(Link decorator,
            ILogger log,
            IHeaderValidator<T> columnHeaderValidator,
            IEnumerable<IHeaderValidator> allValidators) : base(decorator, log)
        {
            _columnHeaderValidator = columnHeaderValidator;
            _allValidators = allValidators;
        }

        protected override async Task<ProcessRequest> HandleAsync(ProcessRequest request)
        {
            var contentRequest = TypeCheck<ProcessContentRequest>(request);
            try
            {
                _columnHeaderValidator.ValidateColumnHeaders(contentRequest.FileContent);

                //if no error is thrown, let's continue
                return await TryRunDecoratorOrReturnAsync(contentRequest).ConfigureAwait(false);
            }
            catch (FileProcessorException e)
            {
                if (e.Type ==ErrorTypeEnum.MissingAllColumns)
                {
                  

                    //store file, userid, email, date, and selected upload type in storage for investigation
                    //await _unknownFileStorageAction
                    //    .StoreUploadAsync(new UnknownFileStorageAction.StorageArgs()
                    //    {
                    //        FileContent = contentRequest.FileContent,
                    //        FileName = contentRequest.FileName,
                    //        SelectedIntegration = contentRequest.Integration,
                    //        Stream = contentRequest.Stream,
                    //        TimeUploaded = DateTime.UtcNow,
                    //        UserEmail = "", //TODO
                    //        UserId = contentRequest.UserId,
                    //        UserTimezone = contentRequest.UserTimezone,
                    //        UserCulture = contentRequest.UserCulture
                    //    }).ConfigureAwait(false);
                    //throw new FileProcessorException(TradeUploadSession.ErrorTypeEnum.UnsupportedFileTypeUploaded, "It looks like you are uploading an unsupported broker / platform");
                }

                throw;
            }
        }


        private class FindResult
        {
            public bool PotentialMatchFound { get; set; }
            public string FriendlyIntegrationName { get; set; }
        }
    }
}
