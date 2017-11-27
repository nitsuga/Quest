using System;
using System.Collections.Generic;

namespace Quest.Common.Messages.Routing
{
    [Serializable]
    public class CustomCoverageResponse : Response
    {
        
        public string id { get; set; }

        
        public List<TrialCoverageResponse> results { get; set; }

        public override string ToString()
        {
            if (results == null)
                return $"CustomCoverageResponse id={id} list is null";
            return $"CustomCoverageResponse id={results.Count} list count={id}";
        }
    }
}