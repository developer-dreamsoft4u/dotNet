using Chartlog.Parser.TakeHome.Domain.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chartlog.Parser.TakeHome.Domain.Infrastructure
{
    /// <summary>
    /// Most csv files contain the account number within each data row. Some other files contain the account number in a completely
    /// different section of the csv file than the section containing the data rows of trades. Use this class for situations when
    /// the account number is not separate from the data rows
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class TradeParsingLinkWithNoSeparateAccount<T> : TradeParsingLink<T> where T : IntegrationType
    {
        protected TradeParsingLinkWithNoSeparateAccount(Link decorator, ILogger log, IHeaderValidator<T> headerValidator) : base(decorator, log, headerValidator)
        {
        }
      
        protected override async Task<ProcessRequest> HandleAsync(ProcessRequest request)
        {
            var contentResult = TypeCheck<ProcessContentRequest>(request);

            var filteredContent = FilterContent(contentResult.FileContent);
            var columnMappings = MapColumns(filteredContent.HeaderLine);
            List<ExternalTrade> parsedTrades = new List<ExternalTrade>();
            var issues = new List<LineParseIssue>();
            try
            {
                parsedTrades = ParseTrades(columnMappings, filteredContent, contentResult);

                parsedTrades.ForEach(a => a.Account = a.Account?.ToUpper());
            }
            catch (ParsingFailureException e)
            {
                parsedTrades.ForEach(a => a.Account = a.Account.ToUpper());

                if (e.Issues.Any(a => a.IsFatal))
                {

                    var res = new TradesParsedRequest(contentResult,
                        new FileContainer()
                        {
                            Binaries = contentResult.FileBinaries,
                            Content = contentResult.FileContent
                        },
                        parsedTrades,
                        e.Issues);

                    throw new FileProcessorException(ErrorTypeEnum.TradeFileParsingError, res.ValidationErrorMessage, e.Issues, res.SessionId.Value);
                }

                if (e.Issues.Any())
                    issues = e.Issues.ToList();
            }


            var response = new TradesParsedRequest(contentResult,
                new FileContainer()
                {
                    Binaries = contentResult.FileBinaries,
                    Content = contentResult.FileContent
                }, parsedTrades
                ,
                issues);
            response = await base.HandleAsync(response) as TradesParsedRequest;

            return await TryRunDecoratorOrReturnAsync(response);
        }
    }
}
