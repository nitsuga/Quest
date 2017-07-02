using System;
using System.Collections.Generic;
using System.Linq;

namespace Quest.Common.Messages
{
    [Serializable]
    public class EventUpdate : MessageBase
    {
        public override string ToString()
        {
            return String.Format("EventUpdate EventId={0} Updated={1}", EventId, Updated);
        }
        public String Callsign;
        public String EventId;
        public String Age;
        public String Sex;
        public String Address;
        public String AZGrid;
        public int Easting;
        public int Northing;
        public float Latitude;
        public float Longitude;
        public String Determinant;
        public DateTime CallOrigin;
        public DateTime Dispatched;
        public DateTime Updated;
        public String Notes;
    }


    [Serializable]
    public class LogonRecord
    {
        public Guid commsID;

        public string Callsign;

        public DateTime LoggedOn;

        public bool ReceiveAll;
    }

    [Serializable]
    public class LoggedOnList : MessageBase
    {
        public List<LogonRecord> Users;

        public override string ToString()
        {

            if (Users != null)
            {
                var all = Users.Select(x => x.Callsign).ToArray();

                return String.Format("Logged On List Count={0} Callsigns={1}", Users.Count(), string.Join(",", all));
            }
            else
            {
                return String.Format("Logged On List Count=NULL ");
            }
        }
    }
}
