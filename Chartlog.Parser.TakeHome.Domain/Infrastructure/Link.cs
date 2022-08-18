using Chartlog.Parser.TakeHome.Domain.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chartlog.Parser.TakeHome.Domain.Infrastructure
{
    public abstract class Link
    {
        protected readonly Link _decorator;
        protected readonly ILogger _log;

        protected Link(Link decorator, ILogger log)
        {
            _decorator = decorator;
            _log = log;
        }


        protected T TypeCheck<T>(ProcessRequest req) where T : ProcessRequest
        {
            var res = req as T;
            if (res == null)
                throw new ArgumentException("Request", $"The parameter passed into {GetType().Name} must be of type {nameof(T)}");

            return res;
        }

        protected async Task<ProcessRequest> TryRunDecoratorOrReturnAsync(ProcessRequest req)
        {
            if (_decorator != null)
                return await _decorator.ExecuteAsync(req);

            return req;
        }

        protected abstract Task<ProcessRequest> HandleAsync(ProcessRequest request);

        public async Task<ProcessRequest> ExecuteAsync(ProcessRequest request)
        {


            var result = await HandleAsync(request);

            return result;
        }
    }
}
