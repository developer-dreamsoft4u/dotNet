

using Chartlog.Parser.TakeHome.Domain.Models;
using Serilog;

namespace Chartlog.Parser.TakeHome.Domain.Infrastructure.Links
{
    public class SessionRecordingLink : Link
    {
        public SessionRecordingLink(Link decorator,
            ILogger log) : base(decorator, log)
        {
           
        }

        protected override async Task<ProcessRequest> HandleAsync(ProcessRequest request)
        {
            var req = TypeCheck<ProcessFileRequest>(request);
            req.StartTime = DateTime.UtcNow;
        

            //await _tradeUploadSessionService.AddUploadSessionAsync(new CreateOrUpdateTradeUploadSessionArgs(req,
            //    TradeUploadSession.UploadType.Manual, TradeUploadSession.SessionStatus.Validating));

            //await _tradeUploadSessionService.SaveChangesAsync();

            try
            {
                //happy path, no exceptions are thrown, this is how we save the session to the db
                var response = await TryRunDecoratorOrReturnAsync(req);

                //await _tradeUploadSessionService.SaveCompletedTradeUploadSession(
                //    new CreateOrUpdateTradeUploadSessionArgs(response as TradesParsedRequest, 
                //        TradeUploadSession.UploadType.Manual,
                //        response.Status,
                //        response.ValidationMessage));

                //await _tradeUploadSessionService.SaveChangesAsync();

                return response;
            }
            catch (FileProcessorException e)
            {
                //failure during validating the file, this is how we save the session to the db
                //var existingSession = await _tradeUploadRepo.GetTradeUploadSessionAsync(req.SessionId, req.UserId);

                //if (existingSession == null)
                //{
                //    _log
                //        .Build()
                //        .WithProperty(Properties.UserId, req.UserId)
                //        .Warning<SessionRecordingLink>("Session cannot be updated, it was not previously saved!");
                //    throw;
                //}


                //await _tradeUploadSessionService.SaveCompletedTradeUploadSession(
                //    new CreateOrUpdateTradeUploadSessionArgs(
                //        e, existingSession, TradeUploadSession.UploadType.Manual));

                //await _tradeUploadSessionService.SaveChangesAsync();

                throw;
            }
            catch (Exception e)
            {
                //something really shitty happened that we didn't expect, end the session with an unknown state
                //_log
                //    .Build()
                //    .WithProperty(Properties.UserId, req.UserId)
                //    .Error("Unexpected error occured", e);


                //var existingSession = await _tradeUploadRepo.GetTradeUploadSessionAsync(req.SessionId, req.UserId);

                //if (existingSession == null)
                //{
                //    _log
                //        .Build()
                //        .WithProperty(Properties.UserId, req.UserId)
                //        .Warning("Session cannot be updated, it was not previously saved!");
                //    throw;
                //}


                //await _tradeUploadSessionService.SaveCompletedTradeUploadSession(
                //    new CreateOrUpdateTradeUploadSessionArgs(
                //        new FileProcessorException(TradeUploadSession.ErrorTypeEnum.Unknown,
                //            "An unknown error occured", request.SessionId),
                //        existingSession, TradeUploadSession.UploadType.Manual));

                //await _tradeUploadSessionService.SaveChangesAsync();

                throw;
            }
            
        }
    }
}
