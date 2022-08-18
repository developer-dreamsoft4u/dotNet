using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chartlog.Parser.TakeHome.Domain.Models
{
    public class LineParseIssue
    {
        public readonly int LineNumber;
        public readonly string Message;
        public readonly string ColumnHeaderValue;
        public readonly IssueTypes ErrorType;
        public readonly bool IsFatal;
        public readonly string LineContent;

        public static List<IssueTypes> FatalIssues = new List<IssueTypes>()
            {
                IssueTypes.MissingRequiredValue,
                IssueTypes.NotANumber,
                IssueTypes.NotAValidDate,
                IssueTypes.ShouldNotBeANumber
            };

        public LineParseIssue(int lineNumber, IssueTypes errorType, string columnHeaderValue, string lineContent)
        {
            LineContent = lineContent;
            LineNumber = lineNumber;
            ColumnHeaderValue = columnHeaderValue;
            ErrorType = errorType;

            if (FatalIssues.Contains(errorType))
                IsFatal = true;

            switch (errorType)
            {
                case IssueTypes.MissingRequiredValue:
                    Message = $"Row {lineNumber} is missing a value for the column {columnHeaderValue}.";
                    break;
                case IssueTypes.NotANumber:
                    Message = $"Row {lineNumber} should contain a number value for the column {columnHeaderValue}.";
                    break;
                case IssueTypes.UnsupportedAsset:
                    Message = $"Row {lineNumber} contains an unsupported asset.";
                    break;
                case IssueTypes.NotAValidDate:
                    Message = $"Row {lineNumber} has an invalid date for the column {columnHeaderValue}";
                    break;
                case IssueTypes.NegativeNumber:
                    Message = $"Row {lineNumber} has a negative value for the column {columnHeaderValue}";
                    break;
                case IssueTypes.ZeroValue:
                    Message = $"Row {lineNumber} has a value of zero for the column {columnHeaderValue}";
                    break;
                case IssueTypes.ShouldNotBeANumber:
                    Message = $"Row {lineNumber} should not be a number for the column {columnHeaderValue}";
                    break;
                case IssueTypes.UnsupportedMarket:
                    Message = $"Row {lineNumber} is from an unsupported market";
                    break;
            }
        }

        public enum IssueTypes
        {
            /// <summary>
            /// For when a column is missing a value and is required
            /// </summary>
            MissingRequiredValue,
            /// <summary>
            /// For foreign assets or anything other than equities
            /// </summary>
            UnsupportedAsset,
            /// <summary>
            /// For when a column is expected to be a number but is instead a random string
            /// </summary>
            NotANumber,
            /// <summary>
            /// For when a column is expected to be a date but is not parsable as one
            /// </summary>
            NotAValidDate,
            /// <summary>
            /// For when a column should not have a negative number
            /// </summary>
            NegativeNumber,
            /// <summary>
            /// For when a column should not contain a zero value
            /// </summary>
            ZeroValue,
            /// <summary>
            /// For when a column should be text but comes in as a number
            /// </summary>
            ShouldNotBeANumber,
            /// <summary>
            /// For when an asset from an unsupported market is detected
            /// </summary>
            UnsupportedMarket
        }
    }
}
