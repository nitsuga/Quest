using System;

namespace Quest.Common.Messages.CAD
{

    [Serializable]
    public class RingbackStatus
    {
        /// <summary>
        ///     The event Id / CAD number
        /// </summary>
        public string Serial { get; set; }

        public DateTime LastRingback { get; set; }

        public override string ToString()
        {
            return $"RingbackStatus {Serial} @ {LastRingback}";
        }
    }
}