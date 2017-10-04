using Quest.Common.Messages;

namespace Quest.Lib.Resource
{
    public interface IResourceStore
    {
        QuestResource GetByResourceId(int resourceId);
        QuestResource GetByFleetNo(string fleetno);
        QuestResource GetByCallsign(string callsign);
        int GetOffroadStatusId();
        QuestResource Update(ResourceUpdate item);
    }    
}