using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chartlog.Parser.TakeHome.Domain.Models
{
    public class ProcessContentRequest : ProcessFileRequest
    {
        public string? FileContent { get; set; }
        public byte[]? FileBinaries { get; set; }

        public ProcessContentRequest(ProcessFileRequest donor, FileContainer fileContent)
        {
            StartTime = donor.StartTime;
            SessionId = donor.SessionId;
            FileContent = fileContent.Content;
            Stream = donor.Stream;
            FileExtension = donor.FileExtension;
            Encoding = donor.Encoding;
            UserId = donor.UserId;
            Integration = donor.Integration;
            FileName = donor.FileName;
            FileBinaries = fileContent.Binaries;
        }
    }
}
