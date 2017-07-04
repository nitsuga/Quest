using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HEMSLink.Message
{
    [DataContract]
    public class Logon : MessageBase 
    {
        [DataMember]
        public String AppId { get; set; }

        [DataMember]
        public int MaxEvents { get; set; }
        [DataMember]
        public DateTime LastUpdate { get; set; }

        public override string ToString()
        {
            return String.Format("Logon AppId={0} MaxEvents={1} LastUpdate={2}", AppId,MaxEvents,LastUpdate);
        }
    }
}
