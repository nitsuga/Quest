using Newtonsoft.Json;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;

namespace Quest.Mobile.Models
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PointFeature<T> : Feature
    {
        T obj;

        public T GetProperties()
        {
            return obj;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="obj"></param>
        public PointFeature(IGeometryObject position, T obj, string id = null) : base( position, obj, id )
        {
        }
    }
    
    
    /// <summary>
    /// 
    /// </summary>
    public class DestinationFeatureCollection : FeatureCollection //<DestinationFeature>
    {
    }

    /// <summary>
    /// 
    /// </summary>
    public class DestinationFeature : PointFeature<GTDestinationFeatureProperties>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="obj"></param>
        public DestinationFeature(IGeometryObject position, GTDestinationFeatureProperties obj) : base(position, obj)
        {
        }

        [JsonProperty("id")]
        public string ID { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    public class GTDestinationFeatureProperties
    {
        [JsonProperty("destination")]
        public string destination { get; set; }

        [JsonProperty("destype")]
        public string destype { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("covertier")]
        public int covertier { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    public class AddressFeature : PointFeature<AddressFeatureProperties>
    {
        public AddressFeature(IGeometryObject position, AddressFeatureProperties obj) : base(position, obj)
        {
        }

        [JsonProperty("id")]
        public string ID { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class AddressFeatureProperties
    {
        [JsonProperty("title")]
        public string Title { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class IncidentFeatureCollection : FeatureCollection //<IncidentFeature>
    {
    }

    /// <summary>
    /// 
    /// </summary>
    public class IncidentFeature : PointFeature<GTIncidentFeatureProperties>
    {
        public IncidentFeature(IGeometryObject position, GTIncidentFeatureProperties obj) : base(position, obj)
        {
        }

        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("ft")]
        public string FeatureType { get; set; }

        [JsonProperty("a")]
        public string Action { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class GTIncidentFeatureProperties
    {
        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("priority")]
        public string Priority { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("lastupdate")]
        public string LastUpdate { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("determinant")]
        public string Determinant { get; set; }

        [JsonProperty("incidentid")]
        public string IncidentId { get; set; }

        [JsonProperty("resources")]
        public int? AssignedResources { get; set; }

        [JsonProperty("az")]
        public string AZ { get; set; }

        [JsonProperty("sex")]
        public string Sex { get; set; }

        [JsonProperty("age")]
        public string Age { get; set; }

        [JsonProperty("prob")]
        public string ProblemDescription { get; set; }

        [JsonProperty("loccomment")]
        public string LocationComment { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    public class ResourceFeatureCollection : FeatureCollection // <ResourceFeature>
    {
    }

    /// <summary>
    /// 
    /// </summary>
    public class ResourceFeature : PointFeature<GTResourceFeatureProperties>
    {
        public ResourceFeature(IGeometryObject position, GTResourceFeatureProperties obj) : base(position, obj)
        {
        }

        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("ft")]
        public string FeatureType { get; set; }

        [JsonProperty("a")]
        public string Action { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    public class GTResourceFeatureProperties
    {
        public string ID { get; set; }

        public string Callsign { get; set; }

        public string ResourceType { get; set; }
        public string ResourceTypeGroup { get; set; }

        public string TimeStamp { get; set; }

        public string Area { get; set; }

        public string ETA { get; set; }

        public string Fleet { get; set; }

        public string Destination { get; set; }

        public string currStatus { get; set; }

        public string StatusCategory { get; set; }

        public string IncSerial { get; set; }

        public string Road { get; set; }

        public string Comment { get; set; }

        public string Standby { get; set; }

        [JsonProperty("skill")]
        public string Skill { get; set; }

        public int? Direction { get; set; }

        public double? Speed { get; set; }
    }
}