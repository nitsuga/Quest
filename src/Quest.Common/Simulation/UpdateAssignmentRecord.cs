using System;
using Quest.Common.Messages;

namespace Quest.Common.Simulation
{
    public class UpdateAssignmentRecord:MessageBase
    {
        public int ResourceId;
        public long IncidentId;
        public String Message;
        public ResourceStatus Status;
    }

}
