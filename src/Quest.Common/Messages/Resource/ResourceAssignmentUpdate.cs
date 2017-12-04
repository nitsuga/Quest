using Quest.Common.Messages.GIS;
using System;

namespace Quest.Common.Messages.Resource
{
    /// <summary>
    /// holds information about a specific assignment and its current progress.
    /// The system tracks progress toward the target and sets a warning status 
    /// if the resource is not expected to arrive on time.
    /// </summary>
    public class ResourceAssignmentUpdate
    {
        public enum StatusCode
        {
            InProgress,
            Warning,
            Cancelled,
            Arrived,
        }

        public String Callsign;

        public DateTime Assigned;

        public DateTime? OriginalEta;

        public DateTime? CurrentEta;

        public DateTime? ArrivedAt;

        public DateTime? LeftAt;

        public DateTime? CancelledAt;

        public string Notes;

        public string DestinationCode;

        public LatLng StartPosition;

        public StatusCode Status;

    }

}
