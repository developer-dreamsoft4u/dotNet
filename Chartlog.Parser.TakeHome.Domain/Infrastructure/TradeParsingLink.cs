using Chartlog.Parser.TakeHome.Domain.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chartlog.Parser.TakeHome.Domain.Infrastructure
{
    public abstract class TradeParsingLink<T> : Link where T : IntegrationType
    {
        protected readonly IHeaderValidator<T> _headerValidator;
        protected T IntegrationType => Activator.CreateInstance<T>();

        protected TradeParsingLink(Link decorator, ILogger log, IHeaderValidator<T> headerValidator) : base(decorator, log)
        {
            _headerValidator = headerValidator;
            ParsingHelper = new ParserHelper();
        }

        protected ColumnMapperHelper CreateMappingHelper(string headerLine, IntegrationType type)
        {
            var headers = headerLine.Split(new[] { IntegrationType.Delimiter }, StringSplitOptions.None);
            return new ColumnMapperHelper(headers);
        }

        protected ParserHelper ParsingHelper { get; }

        protected override Task<ProcessRequest> HandleAsync(ProcessRequest request)
        {
            var r = TypeCheck<TradesParsedRequest>(request);
            if (r.IsFatalSession)
                return Task.FromResult(request);

            var ingestionDate = DateTime.UtcNow;
            foreach (var a in r.ParsedTrades)
            {
                if (a.Type == ExternalTrade.TradeType.Option)
                {
                    a.Hash = $"{a.Account}{a.Direction}{a.Quantity}{a.Symbol}{a.Price}{a.Timestamp}{a.StrikePrice}{a.ExpirationDate}{a.OptionOrderType}".ToMD5();
                }
                else
                {
                    //when users select an account during the manual upload process and the trade file contains no account info, trades are hashed with an empty account string
                    var identifier = !string.IsNullOrWhiteSpace(a.Account) ? a.Account : (a.ExistingAccountId > 0 ? a.ExistingAccountId.ToString() : request.UserId.ToString());
                    a.Hash =
                        $"{identifier}{a.Direction}{a.Quantity}{a.Symbol}{a.Price}{a.Timestamp}".ToMD5();
                }
                a.IngestionDate = a.IngestionDate != DateTime.MinValue ? a.IngestionDate : ingestionDate;
                a.Method = ExternalTrade.UploadMethod.Manual;
                a.UserId = request.UserId.Value;
            }

            return Task.FromResult(r as ProcessRequest);
        }

        protected bool DoesLineContainValuesForAllRequiredHeaders(List<string> nonRequiredHeaders,
            string[] lineColumns, IEnumerable<HeaderTransformation> expectedColumns, List<LineParseIssue> issues,
            int adjustedIndex, string lineContent)
        {
            var expectedColumnsCopy = new List<HeaderTransformation>(expectedColumns);


            for (var i = 0; i < expectedColumnsCopy.Count; i++)
            {
                var target = expectedColumnsCopy[i];
                if (target.Index >= lineColumns.Length && !nonRequiredHeaders.Contains(target.Value))
                {
                    issues.Add(new LineParseIssue(adjustedIndex,
                        LineParseIssue.IssueTypes.MissingRequiredValue,
                        target.Value,
                        lineContent));
                    return false;
                }
            }


            return true;
        }

        /// <summary>
        /// Should trim the uploaded file down to the section we care about: where all the trades are contained
        /// </summary>
        /// <param name="rawContent"></param>
        /// <returns></returns>
        protected virtual FilterResponse FilterContent(string rawContent)
        {
            var headerLineIndex = _headerValidator.ValidateColumnHeaders(rawContent);
            var unfilteredLines = rawContent.SplitIntoRows();
            var headerLine = unfilteredLines[headerLineIndex];
            var filteredLines = unfilteredLines.Skip(headerLineIndex + 1).ToArray();
            var filteredContent = string.Join(Environment.NewLine, filteredLines);

            return new FilterResponse(
                rawContent,
                filteredContent,
                filteredLines,
                unfilteredLines,
                headerLine);
        }

        /// <summary>
        /// This method will find the index of the expected header line within the file by searching for a target string using BeginsWith. The method normalizes the supplied string and all lines of the target file before
        /// searching
        /// </summary>
        /// <param name="searchString"></param>
        /// <param name="fileContent"></param>
        /// <returns></returns>
        protected int FindHeaderIndexByString(string searchString, string fileContent)
        {
            var rows = fileContent.SplitIntoRows();
            var accountHeaderIndex = -1;

            for (var i = 0; i < rows.Length; i++)
            {
                var line = rows[i];
                if (line.Trim().ToLower().StartsWith(searchString.ToLower()))
                {
                    accountHeaderIndex = i;
                    break;
                }
            }

            return accountHeaderIndex;
        }

        /// <summary>
        /// Should find the header columns and get the index for each column, we will use that index to parse the trades section.
        /// This way, column header order never matters and it's one way we can make the upload process easier
        /// </summary>
        /// <param name="filteredLines"></param>
        /// <returns></returns>
        protected abstract IEnumerable<HeaderTransformation> MapColumns(string headerLine);

        /// <summary>
        /// Call this from within the ParseTrades method and use it as a signal for when to stop trying to parse rows
        /// </summary>
        /// <param name="cols"></param>
        /// <param name="firstColumnIndex"></param>
        /// <returns></returns>
        protected abstract bool DetectEndOfTrades(string[] cols, int firstColumnIndex, string culture);

        /// <summary>
        /// Parse the data into trades. Throw errors if any trades are invalid, tell the user
        /// which line the problem trade is in
        /// </summary>
        /// <param name="trimmedContent"></param>
        /// <returns></returns>
        protected abstract List<ExternalTrade> ParseTrades(IEnumerable<HeaderTransformation> headers, FilterResponse filteredResponse, ProcessRequest req);

        /// <summary>
        /// For usage when filtering the contents of a csv down to the target area we expect to work with. Removing all the unnecessary text.
        /// </summary>
        public class FilterResponse
        {
            /// <summary>
            /// The content before filtering has taken place
            /// </summary>
            public readonly string UnfilteredContent;
            /// <summary>
            /// The content after filtering has taken place
            /// </summary>
            public readonly string FilteredContent;
            /// <summary>
            /// The filtered contents split line by line
            /// </summary>
            public readonly string[] FilteredLines;
            /// <summary>
            /// The unfiltered content split line by line
            /// </summary>
            public readonly string[] UnfilteredLines;
            /// <summary>
            /// The target header line, unaltered
            /// </summary>
            public readonly string HeaderLine;

            public FilterResponse(string unfilteredContent,
                string filteredContent,
                string[] filteredLines,
                string[] unfilteredLines,
                string headerLine)
            {
                UnfilteredContent = unfilteredContent;
                FilteredContent = filteredContent;
                UnfilteredLines = unfilteredLines;
                FilteredLines = filteredLines;
                HeaderLine = headerLine;
            }
        }

        /// <summary>
        /// For usage in parsing the header columns and assigning indexes to them so trades can be parsed at the correct indexes
        /// </summary>

        public class ParsingFailureException : Exception
        {
            public readonly IEnumerable<ExternalTrade> ParsedTrades;
            public readonly IEnumerable<LineParseIssue> Issues;
            public ParsingFailureException(IEnumerable<LineParseIssue> issues, IEnumerable<ExternalTrade> parsedTrades)
            {
                Issues = issues;
                ParsedTrades = parsedTrades;
            }
        }
        public class ParserHelper
        {
            private ExternalTrade _trade;
            private string[] _cols;
            private IEnumerable<HeaderTransformation> _headers;
            private List<LineParseIssue> _issues;
            private int _lineNumber;
            private string _userCulture;
            private string _unalteredLine;
            private string _userTimezone = Constants.NYIanaTimezone;

            public void SetLineScope(ExternalTrade t, string[] cols, IEnumerable<HeaderTransformation> headers,
                List<LineParseIssue> issues, int lineNumber, string userCulture, string unalteredLine,
                string userTimezone = Constants.NYIanaTimezone)
            {
                _trade = t;
                _cols = cols;
                _headers = headers;
                _issues = issues;
                _lineNumber = lineNumber;
                _userCulture = userCulture;
                _unalteredLine = unalteredLine;
                _userTimezone = userTimezone;
            }

            private void ValidateSetLineScopeWasCalled()
            {
                if (_trade == null || _cols == null || !_cols.Any()
                || _headers == null || !_headers.Any()
                || _lineNumber < 0 | string.IsNullOrWhiteSpace(_userCulture) || string.IsNullOrWhiteSpace(_unalteredLine)
                || string.IsNullOrWhiteSpace(_userTimezone))
                    throw new InvalidOperationException("SetLineScope was not called");
            }

            public void TryAssignTimestamp(Func<string, string> valueTransformer = null)
            {
                ValidateSetLineScopeWasCalled();
                TryAssignTimestamp(_trade, _cols, _headers, _issues,
                    _lineNumber, _userCulture, _unalteredLine, valueTransformer, userTimezone: _userTimezone);
            }
            public void TryAssignTimestamp(ExternalTrade t, string[] cols, IEnumerable<HeaderTransformation> headers,
                List<LineParseIssue> issues, int lineNumber, string userCulture, string unalteredLine, Func<string, string> valueTransformer = null,
                string userTimezone = Constants.NYIanaTimezone)
            {
                try
                {
                    var header = headers.First(a =>
                        a.ColumnType == HeaderTransformation.ExpectedChartlogColumnType.Timestamp);
                    var val = cols[
                        header.Index];

                    if (string.IsNullOrWhiteSpace(val))
                    {
                        issues.Add(new LineParseIssue(lineNumber, LineParseIssue.IssueTypes.MissingRequiredValue,
                            header.Value, unalteredLine));
                        return;
                    }

                    if (valueTransformer != null)
                        val = valueTransformer(val);


                    try
                    {

                        t.Timestamp = val.ParseDateTimeWithCultureAndTimezone(userCulture, userTimezone);
                    }
                    catch (Exception e)
                    {
                        issues.Add(new LineParseIssue(lineNumber,
                            LineParseIssue.IssueTypes.NotAValidDate,
                            headers.First(a => a.ColumnType == HeaderTransformation.ExpectedChartlogColumnType.Timestamp).Value, unalteredLine));
                    }

                }
                catch (Exception e)
                {
                    issues.Add(new LineParseIssue(lineNumber, LineParseIssue.IssueTypes.MissingRequiredValue,
                        headers.First(a => a.ColumnType == HeaderTransformation.ExpectedChartlogColumnType.Timestamp).Value, unalteredLine));
                }
            }

            #region Combine Date and Time --- Numan

            public void TryAssignDateAndTimestampColumns(Func<string, string> valueTransformer = null, string dateColumnName = "date", string timeColumnName = "time", string specialFormat = "")
            {
                ValidateSetLineScopeWasCalled();
                TryAssignDateAndTimestampColumns(_trade, _cols, _headers, _issues,
                    _lineNumber, _userCulture, _unalteredLine, valueTransformer, userTimezone: _userTimezone, dateColumnName: dateColumnName, timeColumnName: timeColumnName, specialFormat: specialFormat);
            }

            public void TryAssignDateAndTimestampColumns(ExternalTrade t, string[] cols, IEnumerable<HeaderTransformation> headers,
                List<LineParseIssue> issues, int lineNumber, string userCulture, string unalteredLine, Func<string, string> valueTransformer = null,
                string userTimezone = Constants.NYIanaTimezone, string dateColumnName = "date", string timeColumnName = "time", string specialFormat = "")
            {
                try
                {
                    var headerDate = headers.First(a => a.Value.ToLower() == dateColumnName && a.ColumnType == HeaderTransformation.ExpectedChartlogColumnType.Timestamp);
                    string valDate = cols[headerDate.Index];

                    if (string.IsNullOrWhiteSpace(valDate))
                    {
                        issues.Add(new LineParseIssue(lineNumber, LineParseIssue.IssueTypes.MissingRequiredValue, headerDate.Value, unalteredLine));
                        return;
                    }

                    var headerTime = headers.First(a => a.Value.ToLower() == timeColumnName && a.ColumnType == HeaderTransformation.ExpectedChartlogColumnType.Timestamp);
                    string valTime = cols[headerTime.Index];

                    if (string.IsNullOrWhiteSpace(valTime))
                    {
                        issues.Add(new LineParseIssue(lineNumber, LineParseIssue.IssueTypes.MissingRequiredValue, headerDate.Value, unalteredLine));
                        return;
                    }
                    var val = valDate + " " + valTime;

                    if (valueTransformer != null) val = valueTransformer(val);
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(specialFormat))
                        {
                            t.Timestamp = val.ParseDateTimeWithCultureAndTimeZoneAndSpecialFormat(
                            userCulture, userTimezone, specialFormat);
                        }
                        else
                        {
                            t.Timestamp = val.ParseDateTimeWithCultureAndTimezone(userCulture, userTimezone);
                        }

                    }
                    catch (Exception e)
                    {
                        issues.Add(new LineParseIssue(lineNumber,
                            LineParseIssue.IssueTypes.NotAValidDate,
                            headers.First(a => a.ColumnType == HeaderTransformation.ExpectedChartlogColumnType.Timestamp).Value, unalteredLine));
                    }

                }
                catch (Exception e)
                {
                    issues.Add(new LineParseIssue(lineNumber, LineParseIssue.IssueTypes.MissingRequiredValue,
                        headers.First(a => a.ColumnType == HeaderTransformation.ExpectedChartlogColumnType.Timestamp).Value, unalteredLine));
                }
            }
            #endregion

            public void TryAssignDirection(Func<string, Direction> valueTransformer = null)
            {
                ValidateSetLineScopeWasCalled();
                TryAssignDirection(_trade, _cols, _headers, _issues,
                    _lineNumber, valueTransformer, _unalteredLine);
            }


            public void TryAssignDirection(ExternalTrade t, string[] cols, IEnumerable<HeaderTransformation> headers,
                List<LineParseIssue> issues, int lineNumber,
                Func<string, Direction> valueTransformer, string unalteredLine)
            {
                var header = headers
                    .First(a =>
                        a.ColumnType == HeaderTransformation.ExpectedChartlogColumnType.Direction);
                var val = cols[header.Index];
                if (string.IsNullOrWhiteSpace(val))
                {
                    issues.Add(new LineParseIssue(lineNumber,
                        LineParseIssue.IssueTypes.MissingRequiredValue,
                        header.Value, unalteredLine));
                    return;
                }

                t.Direction = valueTransformer(val);
            }

            public void TryAssignSymbol(Func<string, string> valueTransformer = null)
            {
                ValidateSetLineScopeWasCalled();
                TryAssignSymbol(_trade, _cols, _headers, _issues,
                    _lineNumber, _unalteredLine, valueTransformer);
            }
            

            public void TryAssignSymbol(ExternalTrade t, string[] cols, IEnumerable<HeaderTransformation> headers,
                List<LineParseIssue> issues, int lineNumber, string unalteredLine, Func<string, string> valueTransformer = null)
            {
                var header = headers
                    .First(a =>
                        a.ColumnType == HeaderTransformation.ExpectedChartlogColumnType.Symbol);
                var val = cols[header.Index].Trim();

                if (string.IsNullOrWhiteSpace(val))
                {
                    issues.Add(new LineParseIssue(lineNumber,
                        LineParseIssue.IssueTypes.MissingRequiredValue,
                        header.Value, unalteredLine));
                    return;
                }

                //check and make sure symbol is actually text and not a number
                if (decimal.TryParse(val, out decimal num))
                {
                    issues.Add(new LineParseIssue(lineNumber,
                        LineParseIssue.IssueTypes.ShouldNotBeANumber, header.Value, unalteredLine));
                    return;
                }

                if (valueTransformer != null)
                    val = valueTransformer(val);

                if (val.Length > 5)
                {
                    issues.Add(new LineParseIssue(
                        lineNumber, LineParseIssue.IssueTypes.UnsupportedAsset,
                        header.Value, unalteredLine));

                    return;
                }

                t.Symbol = val.Trim().ToUpper();
            }

            //V2
            public void TryAssignUnderlying(Func<string, string> valueTransformer = null)
            {
                ValidateSetLineScopeWasCalled();
                TryAssignUnderlying(_trade, _cols, _headers, _issues,
                    _lineNumber, _unalteredLine, valueTransformer);
            }
            public void TryAssignUnderlying(ExternalTrade t, string[] cols, IEnumerable<HeaderTransformation> headers,
               List<LineParseIssue> issues, int lineNumber, string unalteredLine, Func<string, string> valueTransformer = null)
            {
                var header = headers
                    .First(a =>
                        a.ColumnType == HeaderTransformation.ExpectedChartlogColumnType.Underlying);
                var val = cols[header.Index].Trim();

                if (string.IsNullOrWhiteSpace(val))
                {
                    issues.Add(new LineParseIssue(lineNumber,
                        LineParseIssue.IssueTypes.MissingRequiredValue,
                        header.Value, unalteredLine));
                    return;
                }

                //check and make sure symbol is actually text and not a number
                if (decimal.TryParse(val, out decimal num))
                {
                    issues.Add(new LineParseIssue(lineNumber,
                        LineParseIssue.IssueTypes.ShouldNotBeANumber, header.Value, unalteredLine));
                    return;
                }

                if (valueTransformer != null)
                    val = valueTransformer(val);

                if (val.Length > 5)
                {
                    issues.Add(new LineParseIssue(
                        lineNumber, LineParseIssue.IssueTypes.UnsupportedAsset,
                        header.Value, unalteredLine));

                    return;
                }

                t.Uderlying = val.Trim().ToUpper();
            }

            public void TryAssignAction(Func<string, string> valueTransformer = null)
            {
                ValidateSetLineScopeWasCalled();
                TryAssignAction(_trade, _cols, _headers, _issues,
                    _lineNumber, _unalteredLine, valueTransformer);
            }
            public void TryAssignAction(ExternalTrade t, string[] cols, IEnumerable<HeaderTransformation> headers,
               List<LineParseIssue> issues, int lineNumber, string unalteredLine, Func<string, string> valueTransformer = null)
            {
                var header = headers
                    .First(a =>
                        a.ColumnType == HeaderTransformation.ExpectedChartlogColumnType.Action);
                var val = cols[header.Index].Trim();

                if (string.IsNullOrWhiteSpace(val))
                {
                    issues.Add(new LineParseIssue(lineNumber,
                        LineParseIssue.IssueTypes.MissingRequiredValue,
                        header.Value, unalteredLine));
                    return;
                }

                //check and make sure symbol is actually text and not a number
                if (decimal.TryParse(val, out decimal num))
                {
                    issues.Add(new LineParseIssue(lineNumber,
                        LineParseIssue.IssueTypes.ShouldNotBeANumber, header.Value, unalteredLine));
                    return;
                }

                if (valueTransformer != null)
                    val = valueTransformer(val);

                if (val.Length > 5)
                {
                    issues.Add(new LineParseIssue(
                        lineNumber, LineParseIssue.IssueTypes.UnsupportedAsset,
                        header.Value, unalteredLine));

                    return;
                }

                t.Action = val.Trim().ToUpper();
            }




            public void TryAssignShareCount(Func<string, string> valueTransformer = null)
            {
                ValidateSetLineScopeWasCalled();
                TryAssignShareCount(_trade, _cols, _headers, _issues,
                    _lineNumber, _unalteredLine, valueTransformer);
            }


            public void TryAssignShareCount(ExternalTrade t, string[] cols, IEnumerable<HeaderTransformation> headers,
                List<LineParseIssue> issues, int lineNumber, string unalteredLine, Func<string, string> valueTransformer = null)
            {
                var header = headers
                    .First(a =>
                        a.ColumnType == HeaderTransformation.ExpectedChartlogColumnType.Shares);
                var val = cols[header.Index];

                if (string.IsNullOrWhiteSpace(val))
                {
                    issues.Add(new LineParseIssue(lineNumber,
                        LineParseIssue.IssueTypes.MissingRequiredValue,
                        header.Value, unalteredLine));
                    return;
                }

                //share count should never contain a negative or positive sign
                if (val.Contains("-"))
                    val = val.Replace("-", string.Empty);
                if (val.Contains("+"))
                    val = val.Replace("+", string.Empty);

                if (valueTransformer != null)
                    val = valueTransformer(val);

                try
                {
                    t.Quantity = decimal.Parse(val);

                    if (t.Quantity < 0)
                    {
                        issues.Add(new LineParseIssue(lineNumber, LineParseIssue.IssueTypes.NegativeNumber,
                            header.Value, unalteredLine));
                    }

                    if (t.Quantity == 0)
                    {
                        issues.Add(new LineParseIssue(
                            lineNumber, LineParseIssue.IssueTypes.ZeroValue,
                            header.Value, unalteredLine));
                    }
                }
                catch (Exception e)
                {
                    issues.Add(new LineParseIssue(lineNumber, LineParseIssue.IssueTypes.NotANumber,
                        headers.First(a => a.ColumnType == HeaderTransformation.ExpectedChartlogColumnType.Shares).Value, unalteredLine));
                }
            }


            public void TryAssignPrice(Func<string, string> valueTransformer = null)
            {
                ValidateSetLineScopeWasCalled();
                TryAssignPrice(_trade, _cols, _headers, _issues,
                    _lineNumber, _unalteredLine, valueTransformer);
            }

            public void TryAssignPrice(ExternalTrade t, string[] cols, IEnumerable<HeaderTransformation> headers,
                List<LineParseIssue> issues, int lineNumber, string unalteredLine, Func<string, string> valueTransformer = null)
            {
                var header = headers
                    .First(a =>
                        a.ColumnType == HeaderTransformation.ExpectedChartlogColumnType.Price);

                var val = cols[header.Index];

                if (string.IsNullOrWhiteSpace(val))
                {
                    issues.Add(new LineParseIssue(lineNumber,
                        LineParseIssue.IssueTypes.MissingRequiredValue,
                        header.Value, unalteredLine));
                    return;
                }

                //price should never contain a negative or positive sign
                if (val.Contains("-"))
                    val = val.Replace("-", string.Empty);
                if (val.Contains("+"))
                    val = val.Replace("+", string.Empty);

                if (valueTransformer != null)
                    val = valueTransformer(val);

                try
                {
                    t.Price = decimal.Parse(val);

                    if (t.Price < 0)
                    {
                        issues.Add(new LineParseIssue(lineNumber, LineParseIssue.IssueTypes.NegativeNumber,
                            header.Value, unalteredLine));
                    }

                    if (t.Price == 0)
                    {
                        issues.Add(new LineParseIssue(
                            lineNumber, LineParseIssue.IssueTypes.ZeroValue,
                            header.Value, unalteredLine));
                    }
                }
                catch (Exception e)
                {
                    issues.Add(new LineParseIssue(lineNumber,
                        LineParseIssue.IssueTypes.NotANumber,
                        headers.First(a => a.ColumnType == HeaderTransformation.ExpectedChartlogColumnType.Price).Value, unalteredLine));
                }
            }


            public void TryAssignAccount(Func<string, string> valueTransformer = null)
            {
                ValidateSetLineScopeWasCalled();
                TryAssignAccount(_trade, _cols, _headers, _issues,
                    _lineNumber, _unalteredLine, valueTransformer);
            }

            public void TryAssignAccount(ExternalTrade t, string[] cols, IEnumerable<HeaderTransformation> headers,
                List<LineParseIssue> issues, int lineNumber, string unalteredLine,
                Func<string, string> valueTransformer = null)
            {
                var header = headers
                    .First(a =>
                        a.ColumnType == HeaderTransformation.ExpectedChartlogColumnType.Account);

                var val = cols[header.Index];


                if (string.IsNullOrWhiteSpace(val))
                {
                    issues.Add(new LineParseIssue(lineNumber,
                        LineParseIssue.IssueTypes.MissingRequiredValue,
                        header.Value,
                        unalteredLine));

                    return;
                }

                t.Account = val;
            }

            public void TryAssignCommissions(Func<string, string> valueTransformer = null)
            {
                ValidateSetLineScopeWasCalled();
                TryAssignCommissions(_trade, _cols, _headers, _issues,
                    _lineNumber, _unalteredLine, valueTransformer);
            }


            public void TryAssignCommissions(ExternalTrade t, string[] cols, IEnumerable<HeaderTransformation> headers,
                List<LineParseIssue> issues, int lineNumber, string unalteredLine, Func<string, string> valueTransformer = null)
            {
                var header = headers
                    .First(a =>
                        a.ColumnType == HeaderTransformation.ExpectedChartlogColumnType.Commissions);

                //since commissions is the only optional value, if it ends up at the end of a row, the user may just omit it
                //this will prevent index out of range exceptions
                if (cols.Length <= header.Index)
                {
                    t.Commissions = 0;
                    return;
                }

                var val = cols[header.Index];


                if (string.IsNullOrWhiteSpace(val))
                {
                    //commissions are not mandatory in files but are mandatory in chartlog
                    t.Commissions = 0;
                    return;
                }

                //commissions should never contain a negative or positive sign
                if (val.Contains("-"))
                    val = val.Replace("-", string.Empty);
                if (val.Contains("+"))
                    val = val.Replace("+", string.Empty);

                if (valueTransformer != null)
                    val = valueTransformer(val);

                try
                {
                    t.Commissions = decimal.Parse(val);

                    if (t.Commissions < 0)
                    {
                        issues.Add(new LineParseIssue(lineNumber, LineParseIssue.IssueTypes.NegativeNumber,
                            header.Value, unalteredLine));
                    }
                }
                catch (Exception e)
                {
                    issues.Add(new LineParseIssue(lineNumber,
                        LineParseIssue.IssueTypes.NotANumber,
                        headers.First(a => a.ColumnType == HeaderTransformation.ExpectedChartlogColumnType.Commissions).Value, unalteredLine));
                }
            }
        }
        public class ColumnMapperHelper
        {
            private readonly string[] _headerValues;
            private Dictionary<string, HeaderTransformation.ExpectedChartlogColumnType> _dict = new Dictionary<string, HeaderTransformation.ExpectedChartlogColumnType>();
            public ColumnMapperHelper(string[] headerValues)
            {
                _headerValues = headerValues;
            }

            public ColumnMapperHelper AddAccount(string headerName)
            {
                _dict.Add(headerName.ToLower().Trim(), HeaderTransformation.ExpectedChartlogColumnType.Account);
                return this;
            }

            public ColumnMapperHelper AddTimestamp(string headerName)
            {
                _dict.Add(headerName.ToLower().Trim(), HeaderTransformation.ExpectedChartlogColumnType.Timestamp);
                return this;
            }

            public ColumnMapperHelper AddDirection(string headerName)
            {
                _dict.Add(headerName.ToLower().Trim(), HeaderTransformation.ExpectedChartlogColumnType.Direction);
                return this;
            }

            public ColumnMapperHelper AddSymbol(string headerName)
            {
                _dict.Add(headerName.ToLower().Trim(), HeaderTransformation.ExpectedChartlogColumnType.Symbol);
                return this;
            }

            public ColumnMapperHelper AddShareCount(string headerName)
            {
                _dict.Add(headerName.ToLower().Trim(), HeaderTransformation.ExpectedChartlogColumnType.Shares);
                return this;
            }

            public ColumnMapperHelper AddPrice(string headerName)
            {
                _dict.Add(headerName.ToLower().Trim(), HeaderTransformation.ExpectedChartlogColumnType.Price);
                return this;
            }

            public ColumnMapperHelper AddCommissions(string headerName)
            {
                _dict.Add(headerName.ToLower().Trim(), HeaderTransformation.ExpectedChartlogColumnType.Commissions);
                return this;
            }

            #region V2
            public ColumnMapperHelper AddUnderlying(string headerName)
            {
                _dict.Add(headerName.ToLower().Trim(), HeaderTransformation.ExpectedChartlogColumnType.Underlying);
                return this;
            }
            public ColumnMapperHelper AddAction(string headerName)
            {
                _dict.Add(headerName.ToLower().Trim(), HeaderTransformation.ExpectedChartlogColumnType.Action);
                return this;
            }
            
            #endregion

            public List<HeaderTransformation> BuildTransformations()
            {
                var transformations = new List<HeaderTransformation>();
                for (var i = 0; i < _headerValues.Length; i++)
                {
                    var normalizedHeaderVal = _headerValues[i].Trim().ToLower();

                    if (_dict.ContainsKey(normalizedHeaderVal))
                    {
                        transformations.Add(new HeaderTransformation(_headerValues[i], i, _dict[normalizedHeaderVal]));
                    }
                }

                return transformations;
            }

            public List<HeaderTransformation> BuildTransformations(
                Dictionary<HeaderTransformation.ExpectedChartlogColumnType, string> dict)
            {
                var transformations = new List<HeaderTransformation>();
                var reverseDict = dict.ToDictionary(a => a.Value, b => b.Key);

                for (var i = 0; i < _headerValues.Length; i++)
                {
                    var normalizedHeaderVal = _headerValues[i].Trim().ToLower();

                    if (reverseDict.ContainsKey(normalizedHeaderVal))
                    {
                        transformations.Add(new HeaderTransformation(_headerValues[i], i, reverseDict[normalizedHeaderVal]));
                    }
                }

                return transformations;
            }
        }
    }
}
