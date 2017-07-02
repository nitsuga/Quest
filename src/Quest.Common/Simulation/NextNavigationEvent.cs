using Quest.Common.Messages;

namespace Quest.Common.Simulation
{
    /// <summary>
    /// timed event noting a future navigation change
    /// </summary>
    public class NextNavigationEvent: MessageBase
    {
        public int ResourceId;
        public Waypoint waypoint;
    }

}
