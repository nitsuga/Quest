using System;

namespace Quest.LAS.Messages
{
    public class IncidentCancellation: ICadMessage
    {
        public int IncidentNumber { get; set; }
        public DateTime IncidentDateTime { get; set; }
        public string CancelReason { get; set; }
    }

}
