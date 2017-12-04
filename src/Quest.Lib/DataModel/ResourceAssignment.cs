using System;

namespace Quest.Lib.DataModel
{
    public partial class ResourceAssignment
    {
        public ResourceAssignment()
        {
        }

        public int ResourceAssignmentId { get; set; }
        public string Callsign { get; set; }        
        public int DestinationId { get; set; }
        public int Status { get; set; }
        public DateTime Assigned { get; set; }
        public DateTime? Eta { get; set; }
        public DateTime? OriginalEta { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime? ArrivedAt { get; set; }
        public DateTime? LeftAt { get; set; }
        public string Notes { get; set; }
        public float StartLatitude { get; set; }
        public float StartLongitude { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public Destinations Destination { get; set; }

    }
}
