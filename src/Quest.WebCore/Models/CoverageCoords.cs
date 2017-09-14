using System.Collections.Generic;


namespace Quest.WebCore.Services
{
    public class CoverageCoords
    {
        public int Easting { get; set; }

        public int Northing { get; set; }

        public int Value { get; set; }

    }

    public class LatLngCoverageCoords
    {
        [Newtonsoft.Json.JsonProperty("lat")]
        public double Lat { get; set; }

        [Newtonsoft.Json.JsonProperty("lon")]
        public double Lng { get; set; }

        [Newtonsoft.Json.JsonProperty("value")]
        public int Value { get; set; }
    }

    public class CoverageList
    {
        public CoverageList(List<LatLngCoverageCoords> latlngcoords) 
        {
            LatLngCoords = latlngcoords;
        }

        [Newtonsoft.Json.JsonProperty("data")]
        public List<LatLngCoverageCoords> LatLngCoords { get; set; }
    }
}
