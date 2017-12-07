using Newtonsoft.Json;

namespace Quest.Lib.Google.Places
{
    public class Places
    {
        [JsonProperty("results")]
        public Place[] Results;
    }

    public class Place
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("type")]
        public string Type;

        [JsonProperty("formatted_address")]
        public string FormattedAddress;

        [JsonProperty("geometry")]
        public Geom Geometry;

        [JsonProperty("icon")]
        public string Icon;

        [JsonProperty("image")]
        public string Image;
    }

    public class Geom
    {
        [JsonProperty("location")]
        public Location Location;
    }

    public class Location
    {
        [JsonProperty("lat")]
        public double Lat;

        [JsonProperty("lon")]
        public double Lon;
    }
}

