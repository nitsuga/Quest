using System;
using System.Collections.Generic;

/// <summary>
/// all of this has moved to the core
/// </summary>
namespace Quest.Common.Messages.Gazetteer
{
    [Serializable]
    public class InfoSearchResult
    {
        public long Count;
        public List<LocationDocument> Documents;
    }
}