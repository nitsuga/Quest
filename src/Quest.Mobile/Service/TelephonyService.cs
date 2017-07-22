using Quest.Common.Messages;

namespace Quest.Mobile.Service
{

    public class TelephonyService
    {
        private int _callid;

        public void SubmitCli(string cli, string extension)
        {
            _callid++;

            var m1 = new CallLookupRequest() { CallId = _callid, DDI ="", CLI= cli};

            MvcApplication.MsgClientCache.BroadcastMessage(m1);

            var m2 = new CallEvent { CallId = _callid, Extension = extension, EventType = CallEvent.CallEventType.Alerting };
            MvcApplication.MsgClientCache.BroadcastMessage(m2);

            var m3 = new CallEvent { CallId = _callid, Extension= extension, EventType = CallEvent.CallEventType.Connected };
            MvcApplication.MsgClientCache.BroadcastMessage(m3);
        }

    }


}