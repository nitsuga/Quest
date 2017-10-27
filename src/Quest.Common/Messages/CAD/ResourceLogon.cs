using System;

namespace Quest.Common.Messages.CAD
{
    [Serializable]
    public class ResourceLogon : Request
    {
        public string Callsign { get; set; }
        public DateTime Logon { get; set; }
        public DateTime Logoff { get; set; }

        public override string ToString()
        {
            return "ResourceLogon " + Callsign + " logoff @ " + Logoff;
        }
    }
}