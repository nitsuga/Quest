using System;
using System.Collections.Generic;

namespace Quest.Common.Messages.Routing
{
    /// <summary>
    /// Obtain a custom coverage request
    /// </summary>
    [Serializable]
    public class CustomCoverageRequest : Request
    {
        
        public string id { get; set; }

        
        public List<VehicleOverride> overrides { get; set; }

        
        public string RoutingEngine { get; set; }

        public override string ToString()
        {
            return $"CustomCoverageRequest id={id}";
        }
    }
}