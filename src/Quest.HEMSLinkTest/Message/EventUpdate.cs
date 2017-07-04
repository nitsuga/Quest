using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HEMSLink.Message
{
    [DataContract]
    public class EventUpdate : MessageBase
    {
        public override string ToString()
        {
            return String.Format("EventUpdate EventId={0} Callsign={1} Updated={2}", EventId,Callsign,Updated);
        }

        [DataMember]
        public String EventId { get; set; }
        [DataMember]
        public String Callsign { get; set; }
        [DataMember]
        public String Age { get; set; }
        [DataMember]
        public String Sex { get; set; }
        [DataMember]
        public String Address { get; set; }
        [DataMember]
        public String AZGrid { get; set; }
        [DataMember]
        public int Easting { get; set; }
        [DataMember]
        public int Northing { get; set; }
        [DataMember]
        public float Latitude { get; set; }
        [DataMember]
        public float Longitude { get; set; }
        [DataMember]
        public String Determinant { get; set; }
        [DataMember]
        public DateTime CallOrigin { get; set; }
        [DataMember]
        public DateTime Dispatched { get; set; }
        [DataMember]
        public DateTime Updated { get; set; }
    }
}
