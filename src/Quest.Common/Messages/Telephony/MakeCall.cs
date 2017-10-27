using System;

namespace Quest.Common.Messages.Telephony
{
    [Serializable]
    public class MakeCall: MessageBase
    {
        public MakeCall()
        {
        }

        public MakeCall(int requestId, string caller, string callee)
        {
            RequestId = requestId;
            Caller = caller;
            Callee = callee;
        }

        public int RequestId { get; set; }
        public string Caller { get; set; }
        public string Callee { get; set; }

        public override string ToString()
        {
            return $"MakeCall {RequestId} {Caller}->{Callee}";
        }
    }
}