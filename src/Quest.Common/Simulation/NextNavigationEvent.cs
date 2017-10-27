using Quest.Common.Messages;
using Quest.Common.Messages.Routing;

namespace Quest.Common.Simulation
{
    /// <summary>
    /// timed event noting a future navigation change
    /// </summary>
    public class NextNavigationEvent: MessageBase
    {
        public int Callsign;
        public Waypoint waypoint;
    }

}
