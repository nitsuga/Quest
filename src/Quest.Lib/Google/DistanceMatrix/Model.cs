using Newtonsoft.Json;

namespace Quest.Lib.Google.DistanceMatrix
{
    public class Result
    {
        [JsonProperty("destination_addresses")]
        public string[] DestinationAddresses;

        [JsonProperty("origin_addresses")]
        public string[] OriginAddresses;

        [JsonProperty("rows")]
        public Row[] Rows;

        [JsonProperty("status")]
        public string Status;
    }

    public class Row
    {
        [JsonProperty("elements")]
        public Element[] Elements;
    }

    public class Element
    {
        [JsonProperty("distance")]
        public NumericValue Distance;

        [JsonProperty("duration")]
        public NumericValue Duration;

        [JsonProperty("duration_in_traffic")]
        public NumericValue DurationInTraffic;

        [JsonProperty("status")]
        public string Status;
    }

    public class NumericValue
    {
        [JsonProperty("text")]
        public string text;

        [JsonProperty("value")]
        public double Value;
    }
}
