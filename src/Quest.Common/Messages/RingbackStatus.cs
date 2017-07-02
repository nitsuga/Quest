using System;
using System.Collections.Generic;

namespace Quest.Common.Messages
{
    [Serializable]
    public class RingbackStatusList
    {
        /// <summary>
        ///     The event Id / CAD number
        /// </summary>
        public List<RingbackStatus> Items { get; set; }

        public override string ToString()
        {
            if (Items != null)
                return $"RingbackStatus List count = {Items.Count} ";
            return "RingbackStatus List Empty";
        }
    }

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