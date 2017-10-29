using System.Collections.Generic;
using GeoJSON.Net.Feature;
using Newtonsoft.Json;

namespace Quest.Common.Messages.Visual
{



    public class Visual
    {        
        public VisualId Id { get; set; }
                
        public List<TimelineData> Timeline { get; set; }

        /// <summary>
        /// This item as a feature collection for display
        /// </summary>        
        [JsonIgnore]
        public FeatureCollection Geometry;
    }


}