using Chartlog.Parser.TakeHome.Domain.Infrastructure;
using Chartlog.Parser.TakeHome.Domain.Models;
using Chartlog.Parser.TakeHome.Domain.ParserTwo;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chartlog.Parser.TakeHome.Domain.ParserTwo
{
    public class ParserTwoTradeParser : TradeParsingLinkWithNoSeparateAccount<ParserTwoIntegrationType>
    {
        public ParserTwoTradeParser(Link decorator, ILogger log, IHeaderValidator<ParserTwoIntegrationType> headerValidator) : base(decorator, log, headerValidator)
        {
        }

        protected override bool DetectEndOfTrades(string[] cols, int firstColumnIndex, string culture)
        {
            try
            {
                cols[firstColumnIndex].ParseDateTimeWithCulture(culture);
                return false;
            }
            catch (Exception e)
            {
                return true;
            }
        }

        protected override IEnumerable<HeaderTransformation> MapColumns(string headerLine)
        {
            var helper = CreateMappingHelper(headerLine, IntegrationType);

            helper

                .AddUnderlying("underlying")
                .AddAction("action")
                .AddShareCount("quantity")
                .AddPrice("price");                

            return helper.BuildTransformations();
        }

        protected override List<ExternalTrade> ParseTrades(IEnumerable<HeaderTransformation> headers, FilterResponse filteredResponse, ProcessRequest req)
        {
            var culture = Constants.UsCulture;
            var timezone = Constants.NYIanaTimezone;

            if (filteredResponse.FilteredLines.Length == 0)
                throw new FileProcessorException(ErrorTypeEnum.EmptyFile, "Your file does not contain any trades", req.SessionId.Value);

            var issues = new List<LineParseIssue>();
            var trades = new List<ExternalTrade>();

            for (var i = 0; i < filteredResponse.FilteredLines.Length; i++)
            {
                var adjustedIndex =
                    (filteredResponse.UnfilteredLines.Length - filteredResponse.FilteredLines.Length) + 1 + i;
                var line = filteredResponse.FilteredLines[i];
                var cols = line.Split(new[] { IntegrationType.Delimiter }, StringSplitOptions.None);

                var headerTransformations = headers as HeaderTransformation[] ?? headers.ToArray();
                
                if (!DoesLineContainValuesForAllRequiredHeaders(
                    new List<string>(),
                    cols,
                    headerTransformations,
                    issues,
                    adjustedIndex,
                    line))
                {
                    continue;
                }

                var t = new ExternalTrade();
                ParsingHelper.SetLineScope(t,
                    cols, headerTransformations, issues, adjustedIndex, culture, line, timezone);

                //ParsingHelper.TryAssignDirection(a => a.ToLower().Trim().StartsWith("b") ?
                // Direction.Buy : Direction.Sell);

                ParsingHelper.TryAssignUnderlying();
                if (issues.Any(a =>
                   a.LineNumber == adjustedIndex && a.ErrorType == LineParseIssue.IssueTypes.UnsupportedAsset))
                    continue; //do not add this trade to the parsed trades, we don't want it going to the trade processor

                ParsingHelper.TryAssignAction();
                ParsingHelper.TryAssignShareCount();
                ParsingHelper.TryAssignPrice();
                //ParsingHelper.TryAssignAccount();
                //ParsingHelper.TryAssignTimestamp();
              

                trades.Add(t);

                _log
                    .Information($"Trade {t.Timestamp}, {t.Symbol}, {t.Direction}, {t.Price}, {t.Quantity}");
            }

            if (!issues.Any() && !trades.Any())
            {
                //file was empty, most likely the user has an out of date DAS instance
                throw new FileProcessorException(ErrorTypeEnum.EmptyFile,
                    "Your file was empty and contained no trades");
            }

            if (issues.All(a => a.ErrorType == LineParseIssue.IssueTypes.UnsupportedAsset) &&
                issues.Count == trades.Count)
            {
                throw new FileProcessorException(ErrorTypeEnum.EmptyFile,
                    "All of the trades in the file were of unsupported asset types", issues, req.SessionId.Value);
            }

            if (issues.Any())
            {
                throw new ParsingFailureException(issues, trades);
            }

            return trades;
        }
    }
}
