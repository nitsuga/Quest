using System;

namespace Quest.Common.Messages
{
    [Serializable]
    public class DeleteResource : Request
    {
        public string Callsign { get; set; }

        public override string ToString()
        {
            return "DeleteResource " + Callsign;
        }
    }
}