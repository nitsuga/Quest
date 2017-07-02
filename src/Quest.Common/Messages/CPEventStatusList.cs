using System;
using System.Collections.Generic;

namespace Quest.Common.Messages
{
    [Serializable]
    public class CPEventStatusList : Request
    {
        /// <summary>
        ///     The event Id / CAD number
        /// </summary>
        public List<CPEventStatus> Items { get; set; }

        public override string ToString()
        {
            if (Items != null)
                return $"Event Status List count = {Items.Count} ";
            return "Event Status List Empty";
        }
    }

    [Serializable]
    public class CPEventStatus
    {
        /// <summary>
        ///     The event Id / CAD number
        /// </summary>
        public string Serial { get; set; }

        public string Status { get; set; }

        public string CallerTelephone { get; set; }

        public string LocationComment { get; set; }

        public string ProblemDescription { get; set; }

        public string Age { get; set; }

        public string Sex { get; set; }

        public override string ToString()
        {
            return
                $"EventStatus {Serial} @ {Status} @ {CallerTelephone} @ {LocationComment} @ {ProblemDescription} @ {Age} @ {Sex}";
        }
    }
}