using System;

namespace Quest.Common.Messages
{
    /// <summary>
    ///     an item listed in a nearby search.
    /// </summary>
    [Serializable]
    
    public class EventMapItem
    {
        public long revision;

        
        public int ID { get; set; }

        
        public double Y { get; set; }

        
        public double X { get; set; }

        
        public string EventId { get; set; } // Event ID

        
        public string Notes { get; set; } // Notes

        
        public string Priority { get; set; } // Priority

        
        public string Status { get; set; } // Status

        
        public string Created { get; set; } // created

        
        public DateTime? LastUpdated { get; set; } // created

        
        public int AssignedResources { get; set; } // created

        
        public string PatientAge { get; set; } // created

        
        public string Location { get; set; } // created       

        
        public string LocationComment { get; set; } // created       

        
        public string ProblemDescription { get; set; } // created       
        
        public string DeterminantDescription { get; set; } // created       
        
        public string Determinant { get; set; } // created       
        
        public string AZ { get; set; } // created       
        
        public string PatientSex { get; set; } // created       
        
    }

    
}