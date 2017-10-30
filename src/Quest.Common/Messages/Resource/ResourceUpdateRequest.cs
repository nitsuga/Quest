using System;

namespace Quest.Common.Messages.Resource
{

    /// <summary>
    /// A resource update has been received from CAD - this could be a partial update with some fields
    /// set to null. A field with null does not overwrite the system version of that field
    /// </summary>
    [Serializable]
    public class ResourceUpdateRequest : Request
    {
        public QuestResource Resource;
        public DateTime UpdateTime;

        public override string ToString()
        {
            return $"ResourceUpdateRequest {Resource.Callsign} Status={Resource.Status} type={Resource.ResourceType} event={Resource.EventId}";
        }
    }
}