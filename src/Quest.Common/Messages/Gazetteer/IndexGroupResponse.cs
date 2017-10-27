using System;
using System.Collections.Generic;

namespace Quest.Common.Messages.Gazetteer
{
    [Serializable]
    public class IndexGroupResponse : Response
    {
        public List<IndexGroup> Groups;
        public override string ToString()
        {
            return $"IndexGroupResult {Groups.Count} items";
        }
    }
}