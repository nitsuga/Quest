using System;

namespace Quest.Common.Messages.Telephony
{
    [Serializable]
    public class CallLookupRequest : MessageBase
    {
        public int CallId { get; set; }

        public string CLI { get; set; }

        public string DDI { get; set; }


        public override string ToString()
        {
            return $"New Call Callid={CallId} CLI={CLI} DDI={DDI}";
        }
    }
}
