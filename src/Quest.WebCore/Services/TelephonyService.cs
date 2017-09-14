using Quest.Common.Messages;
using Quest.Lib.ServiceBus;

namespace Quest.WebCore.Services
{
    
    public class TelephonyService
    {
        private int _callid;
        MessageCache _msgClientCache;

        public TelephonyService(MessageCache msgClientCache)
        {
            _msgClientCache = msgClientCache;
        }

        public void SubmitCli(string cli, string extension)
        {
            _callid++;

            var m1 = new CallLookupRequest() { CallId = _callid, DDI ="", CLI= cli};

            _msgClientCache.BroadcastMessage(m1);

            var m2 = new CallEvent { CallId = _callid, Extension = extension, EventType = CallEvent.CallEventType.Alerting };
            _msgClientCache.BroadcastMessage(m2);

            var m3 = new CallEvent { CallId = _callid, Extension= extension, EventType = CallEvent.CallEventType.Connected };
            _msgClientCache.BroadcastMessage(m3);
        }

    }


}