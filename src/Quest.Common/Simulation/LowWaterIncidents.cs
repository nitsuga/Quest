using System;
using Quest.Common.Messages;

namespace Quest.Common.Simulation
{
    [Serializable]
    public class LowWaterIncidents : MessageBase
    {
        public long lastIncidentId;
    }

}
