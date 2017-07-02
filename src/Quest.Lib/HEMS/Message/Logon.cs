using System;
using System.Runtime.Serialization;

namespace Quest.Lib.HEMS.Message
{
    [DataContract]
    [Serializable]
    public class CancelNotifications : MessageBase
    {
        /// <summary>
        /// My DeviceToken used for Push messages
        /// </summary>
        [DataMember]
        public String DeviceToken { get; set; }

    }


    [DataContract]
    [Serializable]
    public class Logon : MessageBase
    {
        [DataMember]
        public String AppId { get; set; }

        /// <summary>
        /// My callsign
        /// </summary>
        [DataMember]
        public String Callsign { get; set; }

        /// <summary>
        /// send me a maximum of these events
        /// </summary>
        [DataMember]
        public int MaxEvents { get; set; }

        /// <summary>
        /// Send me jobs since this time
        /// </summary>
        [DataMember]
        public DateTime LastUpdate { get; set; }

        /// <summary>
        /// Receive all jobs, not just those for my callsign
        /// </summary>
        [DataMember]
        public Boolean ReceiveAll { get; set; }

        /// <summary>
        /// My DeviceToken used for Push messages
        /// </summary>
        [DataMember]
        public String DeviceToken { get; set; }

        [DataMember]
        public int DeviceType { get; set; }  // 0=Dont notify 1=Apple 2=Android 3=chrome 4=blackberry 5=windows phone

        public override string ToString()
        {
            return String.Format("Logon AppId={0} Callsign={1} MaxEvents={2} LastUpdate={3} DeviceToken={4} ReceiveAll={5}", AppId, Callsign, MaxEvents, LastUpdate, DeviceToken, ReceiveAll);
        }
    }
}
