using System;

namespace Quest.Common.Messages.Telephony
{
    [Serializable]
    public class CallEnd : MessageBase
    {
        public int CallId;
        public string Extension;
    }
}
