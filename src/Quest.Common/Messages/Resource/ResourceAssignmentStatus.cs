using Quest.Common.Messages.GIS;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quest.Common.Messages.Resource
{
    /// <summary>
    /// holds information about a specific assignment and its current progress.
    /// The system tracks progress toward the target and sets a warning status 
    /// if the resource is not expected to arrive on time.
    /// </summary>
    public class ResourceAssignmentStatus
    {
        enum Status
        {
            InProgress,
            Warning,
            Cancelled,
            Arrived,
        }
        public string FleetNo;

        public DateTime Assigned;

        public DateTime OriginalEta;

        public DateTime CurrentEta;

        public DateTime ArrivedAt;

        public DateTime CancelledAt;

        public string Notes;

        public string Destination;

        public LatLongCoord DestinationPosition;

        public LatLongCoord OriginalPosition;

        public LatLongCoord CurrentPosition;

    }
}
