using Quest.Common.Messages;

namespace Quest.Common.Simulation
{
    public class MDTIncident : MessageBase
    {
        public string Callsign;
        public SimIncident IncidentDetails;
    }

}
