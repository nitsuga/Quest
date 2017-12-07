using Quest.Common.Messages;
using System.Collections.Generic;

namespace Quest.LAS.Messages
{
    public class CadRawMessage : MessageBase
    {
        public Dictionary<string, string> Headers;
        public byte[] MessageText;
    }
}
