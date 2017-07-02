using System;
using System.Collections.Generic;

namespace Quest.Common.Messages
{

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

    
    [Serializable]
    public class CustomCoverageResponse : Response
    {
        
        public string id { get; set; }

        
        public List<TrialCoverageResponse> results { get; set; }

        public override string ToString()
        {
            if (results == null)
                return $"CustomCoverageResponse id={id} list is null";
            return $"CustomCoverageResponse id={results.Count} list count={id}";
        }
    }

    
    [Serializable]
    public class VehicleOverride
    {
        
        public string Callsign { get; set; }

        
        public int Easting { get; set; }

        
        public int Northing { get; set; }
    }

    
    [Serializable]
    public class TrialCoverageResponse : Response
    {
        
        public CoverageMap Map { get; set; }

        
        public double Before { get; set; }

        
        public double After { get; set; }

        
        public double Delta { get; set; }

        
        public bool LowIsBad { get; set; }

        public void UpdateDelta()
        {
            Delta = (After - Before)*100;

            if (Delta > 100)
                Delta = 100;

            if (Delta < -100)
                Delta = -100;
        }
    }

    /// <summary>
    ///     provides a comprehensive coverage map
    /// </summary>
    
    [Serializable]
    public class CoverageMap
    {
         public int Blocksize;

         public int Columns;

         public byte[] Data;

         public double Percent;

         public int Rows;

        public CoverageMap()
        {
        }

        public CoverageMap(string Name)
        {
            this.Name = Name;
        }

        
        public string Name { get; set; }

        
        public int OffsetX { get; set; }

        
        public int OffsetY { get; set; }
    }
}