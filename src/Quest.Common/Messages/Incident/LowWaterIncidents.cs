using System;

namespace Quest.Common.Messages.Incident
{
    /// <summary>
    /// This message is emitted when the number of simulated incidents drops below a certain threshold
    /// </summary>
    [Serializable]
    public class LowWaterIncidents : MessageBase
    {
        public long lastIncidentId;
    }
}