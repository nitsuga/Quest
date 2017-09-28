using System;

namespace Quest.Common.Messages
{
    /// <summary>
    ///     This class contains details relating to a specific emergency event.
    /// </summary>
    [Serializable]
    
    public class EventNotification : INotificationMessage
    {
        
        public string Priority { get; set; }

        /// <summary>
        ///     unique Id of the event
        /// </summary>
        
        public string EventId { get; set; }

        
        public string PatientAge { get; set; }

        
        public string Sex { get; set; }

        
        public string Location { get; set; }

        
        public string LocationComment { get; set; }

        
        public string AZGrid { get; set; }

        
        public double? Latitude { get; set; }

        
        public double? Longitude { get; set; }

        
        public string Determinant { get; set; }

        
        public string CallOrigin { get; set; }

        
        public string Created { get; set; }

        
        public string Updated { get; set; }

        
        public string Reason { get; set; }

        
        public string Notes { get; set; }

        public override string ToString()
        {
            return $"EventNotification EventId={EventId} Updated={Updated}";
        }
    }

}