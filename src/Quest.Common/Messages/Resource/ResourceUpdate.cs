using Quest.Common.Messages.GIS;
using System;

namespace Quest.Common.Messages.Resource
{
    /// <summary>
    /// notification that a resource has changed in the system
    /// </summary>
    [Serializable]
    public class ResourceUpdate : MessageBase
    {
        public string Callsign;

        // optional item details (missing if the item has been deleted)
        public QuestResource Item;
    }
}