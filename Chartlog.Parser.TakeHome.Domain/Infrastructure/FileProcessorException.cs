using Chartlog.Parser.TakeHome.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chartlog.Parser.TakeHome.Domain.Infrastructure
{
    public class FileProcessorException : Exception
    {
        public readonly ErrorTypeEnum Type;
        public bool IsFatalSession => true;
        public readonly IEnumerable<LineParseIssue> Issues = new List<LineParseIssue>();
        public bool ContainsNonFatalErrors => false;
        public readonly Guid SessionId;

        public FileProcessorException(ErrorTypeEnum type, string message, Guid sessionId) : base(message)
        {
            Type = type;
        }

        public FileProcessorException(ErrorTypeEnum type, string message,
            IEnumerable<LineParseIssue> issues, Guid sessionId) : this(type, message, sessionId)
        {
            Issues = issues;
        }

        public FileProcessorException(ErrorTypeEnum type, string message) : base(message)
        {
            Type = type;
        }
    }
}
