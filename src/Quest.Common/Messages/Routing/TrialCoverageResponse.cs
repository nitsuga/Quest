using System;

namespace Quest.Common.Messages.Routing
{
    [Serializable]
    public class TrialCoverageResponse : Response
    {
        
        public CoverageMap Map { get; set; }

        
        public double Before { get; set; }

        
        public double After { get; set; }

        
        public double Delta { get; set; }

        
        public bool LowIsBad { get; set; }

        public void UpdateDelta()
        {
            Delta = (After - Before)*100;

            if (Delta > 100)
                Delta = 100;

            if (Delta < -100)
                Delta = -100;
        }
    }
}