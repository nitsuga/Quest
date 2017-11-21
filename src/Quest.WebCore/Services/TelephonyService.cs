using Quest.Common.Messages.Telephony;
using Quest.Lib.ServiceBus;

namespace Quest.WebCore.Services
{
    public class TelephonyService
    {
        private int _callid;
        AsyncMessageCache _msgClientCache;

        public TelephonyService(AsyncMessageCache msgClientCache)
        {
            _msgClientCache = msgClientCache;
        }
    }
}