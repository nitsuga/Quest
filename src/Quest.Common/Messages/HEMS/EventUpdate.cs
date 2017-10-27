using System;

namespace Quest.Common.Messages.HEMS
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
}
