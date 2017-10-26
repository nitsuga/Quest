using System;

namespace Quest.Common.Messages
{

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
            return $"ResourceUpdate {Resource.Callsign} Status={Resource.Status} type={Resource.ResourceType} event={Resource.EventId}";
        }
    }

    public class ResourceUpdateResult : MessageBase
    {
        public QuestResource OldResource;
        public QuestResource NewResource;
    }
}