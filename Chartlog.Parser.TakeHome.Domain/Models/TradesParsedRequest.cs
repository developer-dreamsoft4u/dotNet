using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chartlog.Parser.TakeHome.Domain.Models
{
    public class TradesParsedRequest : ProcessContentRequest
    {
        public IEnumerable<ExternalTrade> ParsedTrades { get; set; }
        public IEnumerable<LineParseIssue> Issues { get; set; }
        public bool IsFatalSession => Issues.Any(a => a.IsFatal);
        public bool ContainsNonFatalErrors => Issues.Any(a => !a.IsFatal);
        public string ValidationErrorMessage { get; set; }

        public TradesParsedRequest(ProcessFileRequest donor,
            FileContainer fileContent,
            IEnumerable<ExternalTrade> parsedTrades,
            IEnumerable<LineParseIssue> issues) : base(donor, fileContent)
        {
            ParsedTrades = parsedTrades;
            Issues = issues;

            if (IsFatalSession || ContainsNonFatalErrors)
            {
                var sb = new StringBuilder();

                var lineParseIssues = Issues as LineParseIssue[] ?? Issues.ToArray();
                if (lineParseIssues.Any(a => a.ErrorType == LineParseIssue.IssueTypes.UnsupportedAsset))
                    sb.Append("Some of the rows in your file contain unsupported assets");
                if (lineParseIssues.Any(a => a.ErrorType == LineParseIssue.IssueTypes.NotANumber))
                    sb.Append(
                        $"{(sb.Length > 0 ? Environment.NewLine : string.Empty)} Some of the rows in your file contain fields that should be numeric but contain alpha characters instead");
                if (lineParseIssues.Any(a => a.ErrorType == LineParseIssue.IssueTypes.MissingRequiredValue))
                    sb.Append(
                        $"{(sb.Length > 0 ? Environment.NewLine : string.Empty)} Some of the rows in your file are missing required values");
                if (lineParseIssues.Any(a => a.ErrorType == LineParseIssue.IssueTypes.NotAValidDate))
                    sb.Append(
                        $"{(sb.Length > 0 ? Environment.NewLine : string.Empty)} Some of the rows in your file do not contain valid dates. Make sure you are selecting the correct date format under \"Advanced Upload Settings\"");
                if (lineParseIssues.Any(a => a.ErrorType == LineParseIssue.IssueTypes.NegativeNumber))
                    sb.Append(
                        $"{(sb.Length > 0 ? Environment.NewLine : string.Empty)} Some of the rows in your file contain negative numbers. Please do not use negative numbers for share count or price");
                if (lineParseIssues.Any(a => a.ErrorType == LineParseIssue.IssueTypes.ZeroValue))
                    sb.Append(
                        $"{(sb.Length > 0 ? Environment.NewLine : string.Empty)} Some of the rows in your file contain zero values. Please do not input a trade if it has zero shares traded");
                if (lineParseIssues.Any(a => a.ErrorType == LineParseIssue.IssueTypes.ShouldNotBeANumber))
                    sb.Append(
                        $"{(sb.Length > 0 ? Environment.NewLine : string.Empty)} Some of the rows in your file contain fields that should be alpha characters but numeric values were found instead");

                ValidationErrorMessage = sb.ToString();
            }
        }
    }
}
