using Quest.Common.Messages;

namespace Quest.Common.Simulation
{
    public class CallsignUpdate:MessageBase
    {
        public string Callsign;
        public int ResourceId;
    }

}
