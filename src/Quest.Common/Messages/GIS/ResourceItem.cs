using Quest.Common.Messages.Resource;
using System;

namespace Quest.Common.Messages.GIS
{
    [Serializable]    
    public class ResourceItem: PointMapItem
    {
        public QuestResource Resource;
    }
}