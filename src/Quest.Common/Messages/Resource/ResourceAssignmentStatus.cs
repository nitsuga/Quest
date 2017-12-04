using Quest.Common.Messages.GIS;
using System;

namespace Quest.Common.Messages.Resource
{

    public class ResourceAssignmentStatus
    {
        public enum StatusCode
        {
            InProgress,
            Warning,
            Cancelled,
            Arrived,
        }
        public QuestResource Resource;

        public DateTime Assigned;

        public DateTime? OriginalEta;

        public DateTime? CurrentEta;

        public string TTG;

        public DateTime? ArrivedAt;

        public DateTime? LeftAt;

        public DateTime? CancelledAt;

        public string Percent;

        public string Notes;

        public string DestinationCode;

        public LatLng StartPosition;

        public StatusCode Status;

    }

}
