using System;

namespace Quest.Common.Messages.CAD
{
    [Serializable]
    public class XCOutbound : MessageBase
    {
        public string Command;
        public string Channel;
    }
}