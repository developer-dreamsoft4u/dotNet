

using Chartlog.Parser.TakeHome.Domain.Models;
using Serilog;

namespace Chartlog.Parser.TakeHome.Domain.Infrastructure.Links
{
    public class BackupFileLink : Link
    {

        public BackupFileLink(Link decorator, ILogger log) : base(decorator, log)
        {
            
        }

        protected override async Task<ProcessRequest> HandleAsync(ProcessRequest request)
        {
            var contentRequest = TypeCheck<ProcessContentRequest>(request);

            _log
                .Debug("Attempting to backup file");

            //save file in blob storage
            //var result = await _backupService.BackupAsync(new BackupRequest(
            //    contentRequest.UserId,
            //    hash,
            //    fileBinaries,
            //    contentRequest.StartTime,
            //    contentRequest.FileName));


            //now log the backup in table storage
            //await _backupService.RecordBackupAsync(new RecordBackupRequest(
            //    result.PathToBackup,
            //    hash,
            //    contentRequest.FileName,
            //    contentRequest.Integration,
            //    contentRequest.UserId,
            //    contentRequest.SessionId));

            _log
                .Debug("File backedup to Azure Storage");

            return await TryRunDecoratorOrReturnAsync(contentRequest);
        }
    }
}
