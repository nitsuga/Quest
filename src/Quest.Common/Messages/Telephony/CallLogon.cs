using System;

namespace Quest.Common.Messages.Telephony
{
    [Serializable]
    public class CallLogon : MessageBase
    {
        public string Extension { get; set; }
    }
}
