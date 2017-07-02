using System;
using System.Runtime.Serialization;

namespace Quest.Lib.HEMS.Message
{
    [DataContract]
    [Serializable]
    public class HEMSMessage
    {
        [DataMember]
        public MessageBase MessageBody { get; set; }
    }
}
