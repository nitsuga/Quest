using Nest;
using System;

namespace Quest.Common.Messages.Telephony
{
    [Serializable]
    public class CallLookupResponse : MessageBase
    {

        public bool IsMobile;

        public string Status { get; set; }

        public CallerDetailsStatusCode StatusCode { get; set; }

        /// <summary>
        /// single line address that produces a search hit
        /// </summary>

        public string SearchableAddress { get; set; }


        public string[] Address { get; set; }

        public int Requery { get; set; }

        public int Altitude { get; set; }

        public int Angle { get; set; }

        public int CallId { get; set; }

        public int Confidence { get; set; } // 0-99

        public int Direction { get; set; }

        public int Eastings { get; set; } // (metres)

        public string Name { get; set; }

        public int Northings { get; set; } // (metres)

        public int SemiMajor { get; set; }

        public int SemiMinor { get; set; }

        public int Speed { get; set; }

        public string TelephoneNumber { get; set; }

        public int RejectCode { get; set; }


        public PolygonGeoShape Shape;
    }
}
