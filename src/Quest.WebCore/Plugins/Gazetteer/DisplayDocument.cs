using Nest;

namespace Quest.WebCore.Plugins.Gazetteer
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


}
