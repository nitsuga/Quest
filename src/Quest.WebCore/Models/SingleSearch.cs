using System;
using Quest.Common.Messages;
using Quest.Common.Messages.Gazetteer;

namespace Quest.WebCore.Services
{
    [Serializable]
    public class SingleSearch
    {
        // ReSharper disable once InconsistentNaming
        public string searchText;
        // ReSharper disable once InconsistentNaming
        public LocationDocument bestmatch;
        // ReSharper disable once InconsistentNaming
        public bool complete;
        // ReSharper disable once InconsistentNaming
        public bool header;
        // ReSharper disable once InconsistentNaming
        public string status;
        // ReSharper disable once InconsistentNaming
        public long count;
        // ReSharper disable once InconsistentNaming
        public double score;
    }
}