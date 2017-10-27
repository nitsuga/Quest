using System;

namespace Quest.Common.Messages.CAD
{

    [Serializable]
    public class CadEventStatus
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