using GeoJSON.Net.Feature;

namespace Quest.Common.Messages.Visual
{
    public class GetVisualsDataResponse : Response
    {
        /// <summary>
        /// list of visuals
        /// </summary>
        
        public FeatureCollection Geometry { get; set; }
    }
}