using Quest.Common.Messages;
using Quest.Lib.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Quest.Api.Extensions
{
    public static class MQHelper
    {
        public static RES Submit<RES, REQ>(this REQ request, AsyncMessageCache cache, int timeout = 10)
            where RES : Response
            where REQ : Request
        {
            request.RequestId = Guid.NewGuid().ToString();
            var result = cache.SendAndWait<RES>(request, new TimeSpan(0, 0, timeout));
            return result;
        }

        public static TRes Submit<TRes>(this Request request, AsyncMessageCache cache, int timeout = 10) where TRes : class
        {
            request.RequestId = Guid.NewGuid().ToString();
            var result = cache.SendAndWait<TRes>(request, new TimeSpan(0, 0, timeout));
            return result;
        }

    }
}
