using System;

namespace Quest.Common.Messages
{
    [Serializable]    
    public class ResourceItem: PointMapItem
    {
        public QuestResource Resource;
    }
}