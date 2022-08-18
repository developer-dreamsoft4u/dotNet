using Chartlog.Parser.TakeHome.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chartlog.Parser.TakeHome.Domain.Infrastructure
{
    public interface IHeaderValidator
    {
        string IntegrationName { get; }
        string FriendlyIntegrationName { get; }

        int ValidateColumnHeaders(string content);
    }

    public interface IHeaderValidator<T> : IHeaderValidator where T : IntegrationType
    {

    }

    public class HeaderValidator<T> : IHeaderValidator<T> where T : IntegrationType
    {
        private readonly IColumnHeaderValidator _validator;
        private Lazy<T> Integration = new Lazy<T>(Activator.CreateInstance<T>);
        public string IntegrationName => Integration.Value.IntegrationName;
        public string FriendlyIntegrationName => Integration.Value.FriendlyIntegrationName;

        public HeaderValidator(IColumnHeaderValidator validator)
        {
            _validator = validator;
        }
        public int ValidateColumnHeaders(string content)
        {
            return _validator.ValidateColumnHeadersAndReturnHeaderIndex(content, Integration.Value.RequiredHeaders);
        }
    }


    public interface IColumnHeaderValidator
    {
        int ValidateColumnHeadersAndReturnHeaderIndex(string content, params string[] requiredColumns);
    }

    public class ColumnHeaderValidator : IColumnHeaderValidator
    {
        public int ValidateColumnHeadersAndReturnHeaderIndex(string content, params string[] requiredColumns)
        {
            //some integrations don't have required column headers
            if (requiredColumns == null)
                return -1;

            var cols = requiredColumns.ToList();
            if (cols.Count == 0 || cols.All(string.IsNullOrWhiteSpace))
                throw new ArgumentException(nameof(requiredColumns), "requiredColumns columns is either null or has been supplied nothing but empty string values");

            var lines = content.SplitIntoRows();

            return TryFindMissingColumns(lines, requiredColumns.ToList());
        }

        public int TryFindMissingColumns(string[] lines, List<string> requiredColumns)
        {
            var targetLine = string.Empty;
            var targetIndex = -1;
            //first find any line that contains at least one of the required columns
            for (var index = 0; index < lines.Length; index++)
            {
                var line = lines[index];
                if (requiredColumns.Any(a => line.ToLower().Contains(a.ToLower())))
                {
                    targetLine = line;
                    targetIndex = index;
                    break;
                }
            }

            //all columns were missing
            if (targetLine == string.Empty)
                throw new MissingColumnsFileProcessException(ErrorTypeEnum.MissingAllColumns, requiredColumns.ToArray(), $"The following required column(s) are missing from your file: {string.Join(",", requiredColumns.ToArray())}");

            var missingColumns = requiredColumns.Where(a => !targetLine.ToLower().Contains(a.ToLower())).ToList();


            if (missingColumns.Any())
            {
                if (missingColumns.All(a => a.Contains('|')))
                {
                    //multiple columns with the same value, as long as one column is there then do not throw an exception
                    if (missingColumns.Select(a => a.Split('|')).All(a => a.Any(b => targetLine.ToLower().Contains(b.ToLower()))))
                    {
                        return targetIndex;
                    }
                }
                throw new MissingColumnsFileProcessException(ErrorTypeEnum.MissingSomeColumns, missingColumns.ToArray(), $"The following required column(s) are missing from your file: {string.Join(",", missingColumns.ToArray())}");
            }


            return targetIndex;
        }

        public class MissingColumnsFileProcessException : FileProcessorException
        {
            public string[] MissingColumns { get; }
            public MissingColumnsFileProcessException(ErrorTypeEnum type, string[] missingColumns, string message) : base(type, message)
            {
                MissingColumns = missingColumns;
            }
        }
    }
}
