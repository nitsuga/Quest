using System;
using System.Runtime.Serialization;

namespace Quest.Lib.HEMS
{
    [DataContract]
    [Serializable]
    public class NotificationRecord
    {
        [DataMember]
        public String Callsign;

        [DataMember]
        public String DeviceToken { get; set; }

        [DataMember]
        public int DeviceType { get; set; }

        [DataMember]
        public Boolean ReceiveAll { get; set; }

    }

}
