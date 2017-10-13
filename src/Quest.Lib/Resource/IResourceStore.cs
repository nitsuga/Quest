using Quest.Common.Messages;

namespace Quest.Lib.Resource
{
    public interface IResourceStore
    {
        QuestResource GetByFleetNo(string fleetno);
        QuestResource GetByCallsign(string callsign);
        int GetOffroadStatusId();
        ResourceUpdateResult Update(ResourceUpdate item);
        void Clear();
    }    
}