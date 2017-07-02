#pragma warning disable 0169,649
#if NET45
using System.Data.Spatial;
#endif
using Nest;

namespace Quest.Mobile.Models
{
    public class DisplayDocument
    {
        /// <summary>
        /// Score
        /// </summary>
        public string c { get; set; }
        public float s { get; set; }
        public string t { get; set; }
        public string src { get; set; }
        public string ID { get; set; }
        public string d { get; set; }
        public string url { get; set; }
        public string st { get; set; }
        public string i { get; set; }
        public string v { get; set; }
        public GeoLocation l { get; set; }
        public PolygonGeoShape pg { get; set; }
        public MultiLineStringGeoShape ml { get; set; }
        public string grp { get; set; }
    }
    public class p
    {
        public double x;
        public double y;
    }

}
