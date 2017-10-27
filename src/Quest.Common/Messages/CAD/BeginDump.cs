using System;

namespace Quest.Common.Messages.CAD
{
    [Serializable]
    public class BeginDump : Request
    {
        public string From { get; set; }

        public override string ToString()
        {
            return "BeginDump ";
        }
    }
}