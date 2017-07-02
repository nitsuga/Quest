
using System;
using System.Runtime.Serialization;

namespace Quest.Lib.HEMS.Message
{
    [DataContract]
    [Serializable]
    public class DataRequest : MessageBase
    {
        public override string ToString()
        {
            return String.Format("DataRequest Command={0} Callsign={1}", Command, Callsign);
        }

        [DataMember]
        public String Command { get; set; }

        [DataMember]
        public String Callsign { get; set; }
    }

    [DataContract]
    [Serializable]
    public class DataResponse : MessageBase
    {
        public override string ToString()
        {
            return String.Format("DataResponse Command={0} Callsign={1}", Command, Callsign);
        }

        [DataMember]
        public String Command { get; set; }

        [DataMember]
        public String Callsign { get; set; }

        [DataMember]
        public String Response { get; set; }
    }
}
