
using System;
using System.Runtime.Serialization;

namespace Quest.Lib.HEMS.Message
{
    [DataContract]
    [Serializable]
    public class EventNotification
    {
        [DataMember]
        public String Callsign { get; set; }

        [DataMember]
        public String EventId { get; set; }

    }
}
