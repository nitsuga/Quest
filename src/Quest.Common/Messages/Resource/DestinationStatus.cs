using Quest.Common.Messages.Destination;
using System.Collections.Generic;

namespace Quest.Common.Messages.Resource
{

    public class DestinationStatus
    {
        public enum StatusCode
        {
            Uncovered,
            InProgress,
            Covered,
        }

        public QuestDestination Destination;

        public StatusCode Status;

        public List<ResourceAssignmentStatus> Assignments;

        public List<QuestResource> Nearby;
    }

}
