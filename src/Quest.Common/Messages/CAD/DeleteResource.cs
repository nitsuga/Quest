using System;

namespace Quest.Common.Messages.CAD
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