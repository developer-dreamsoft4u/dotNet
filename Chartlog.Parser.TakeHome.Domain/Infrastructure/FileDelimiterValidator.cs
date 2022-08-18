using Chartlog.Parser.TakeHome.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chartlog.Parser.TakeHome.Domain.Infrastructure
{
    public interface IFileDelimiterValidator
    {
        void Execute(string content,
            int headerIndex,
            string[] requiredHeaders,
            string desiredDelimiter);
    }

    public class FileDelimiterValidator : IFileDelimiterValidator
    {

        public FileDelimiterValidator()
        {

        }


        public void Execute(string content,
            int headerIndex,
            string[] requiredHeaders,
            string desiredDelimiter)
        {
            var lines = content.SplitIntoRows();
            var targetLine = lines[headerIndex];
            var splitWithTargetDelimiter = targetLine.Split(new string[] { desiredDelimiter }, StringSplitOptions.None);

            //try to see if all values intersect, if they don't the delimiter in the file is incorrect
            if (!DoHeadersIntersectCompletlyWithRequiredHeaders(splitWithTargetDelimiter, requiredHeaders))
            {
                TryToDetectDelimiterAndThrow(targetLine, desiredDelimiter, requiredHeaders);
            }

        }

        private bool DoHeadersIntersectCompletlyWithRequiredHeaders(IEnumerable<string> compared, string[] requiredHeaders)
        {
            var matchCount = 0;
            foreach (var header in compared)
            {
                if (requiredHeaders.Any(a => a.ToLower().Trim() == header.ToLower().Trim())
                    || requiredHeaders.Any(a => a.Split('|').Any(b => b.ToLower().Trim() == header.ToLower().Trim())))
                {
                    //is good
                    matchCount++;
                }
            }
            return matchCount == requiredHeaders.Length;
        }

        private void TryToDetectDelimiterAndThrow(string line, string d, string[] requiredHeaders)
        {
            var possibleDelimiters = new[] { ",", ";", "\t", "|" };

            foreach (var delimiter in possibleDelimiters)
            {
                //this already failed, that's why we are searching for other possible delimiters
                if (delimiter == d)
                    continue;

                var split = line
                    .Split(new[] { delimiter }, StringSplitOptions.None);

                if (DoHeadersIntersectCompletlyWithRequiredHeaders(split, requiredHeaders))
                {
                    //we found a match
                    throw new FileProcessorException(ErrorTypeEnum.IncorrectFileDelimiter,
                        $"Your file should be delimited by \"{d}\" but appears to be delimited by a {delimiter} character instead");
                }
            }

            //if we reach here, no match was found
            throw new FileProcessorException(ErrorTypeEnum.IncorrectFileDelimiter,
                $"Your file should be delimited by \"{d}\" We can't tell what character is separating each of your columns.");
        }
    }
}
