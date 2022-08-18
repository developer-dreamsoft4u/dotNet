using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chartlog.Parser.TakeHome.Domain.Models
{
    public enum ErrorTypeEnum
    {
        Unknown,
        WrongEncoding,
        UnknownEncoding,
        WrongFileExtension,
        MissingSomeColumns,
        IncorrectPlatformSelection,
        UnsupportedFileTypeUploaded,
        MissingAllColumns,
        EmptyFile,
        IncorrectFileDelimiter,
        MissingAccountNumber,
        TradeProcessorError,
        TradeFileParsingError

    }
}
