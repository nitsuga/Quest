using System;
using Quest.Common.Messages;
using Quest.Common.Messages.Telephony;

namespace Quest.Lib.EISEC
{
    /// <summary>
    ///     Info re the connection with the EDB
    /// </summary>
    public class QueryRequest
    {
        public CallLookupRequest Details;
        public CallLookupResponse Address;
        public int RequestId;
        public RequestState State;
        public DateTime TimeSent;
        public DateTime SendAtTime;
        public int RequeryCount;
    }
}