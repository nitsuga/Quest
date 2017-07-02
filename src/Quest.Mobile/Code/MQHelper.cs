using Quest.Common.Messages;
using System;

namespace Quest.Mobile.Code
{
    public static class MQHelper
    {

        public static RES Submit<RES, REQ>(this REQ request, int timeout = 10)
            where RES : Response
            where REQ : Request
        {
            request.RequestId = Guid.NewGuid().ToString();
            var result = MvcApplication.MsgClientCache.SendAndWait<RES>(request, new TimeSpan(0, 0, timeout));
            return result;
        }

        public static TRes Submit<TRes>(this Request request, int timeout = 10) where TRes:class 
        {
            request.RequestId = Guid.NewGuid().ToString();
            var result = MvcApplication.MsgClientCache.SendAndWait<TRes>(request, new TimeSpan(0, 0, timeout));
            return result;
        }

    }
}