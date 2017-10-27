using System;

namespace Quest.Common.Messages.HEMS
{
    [Serializable]
    public class LogonRecord
    {
        public Guid commsID;

        public string Callsign;

        public DateTime LoggedOn;

        public bool ReceiveAll;
    }
}
