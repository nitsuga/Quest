using Quest.Common.Messages;

namespace Quest.Common.Simulation
{
    public class MDTIncident : MessageBase
    {
        public int ResourceId;
        public SimIncident IncidentDetails;
    }

}
