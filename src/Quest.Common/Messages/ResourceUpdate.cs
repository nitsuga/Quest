using System;

namespace Quest.Common.Messages
{

    /// <summary>
    /// notificate that a database change has occurred for this resource
    /// </summary>
    [Serializable]
    public class ResourceDatabaseUpdate : Request
    {
        public string Callsign;

        // optional item details (missing if the item ha been deleted)
        public ResourceItem Item;
    }

    /// <summary>
    /// A resource update has been received from CAD - this is a full update
    /// </summary>
    [Serializable]
    public class ResourceUpdate : MessageBase
    {
        public QuestResource Resource;
        public DateTime UpdateTime;

        public override string ToString()
        {
            return $"ResourceUpdate {Resource.Callsign} Status={Resource.Status} type={Resource.ResourceType} event={Resource.Incident}";
        }
    }

    public class ResourceUpdateResult : MessageBase
    {
        public QuestResource OldResource;
        public QuestResource NewResource;
    }
}