using System;

namespace Quest.WebCore.Plugins.Gazetteer
{
    [Serializable]
    public class SemanticSearchRequest
    {
        public string SearchText { get; set; }

        public int SearchMode { get; set; }

        public bool IncludeAggregates { get; set; }

        public int Skip { get; set; }

        public int Take { get; set; }

        public bool BoundsFilter { get; set; }

        public double W { get; set; }

        public double S { get; set; }

        public double E { get; set; }

        public double N { get; set; }

        public string FilterTerms { get; set; }

        public string DisplayGroup { get; set; }

        public string IndexGroup { get; set; }
    }
}