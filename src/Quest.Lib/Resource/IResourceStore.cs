using Quest.Common.Messages;

namespace Quest.Lib.Resource
{
    public interface IResourceStore
    {
        bool FleetNoExists(string fleetno);
        QuestResource GetByFleetNo(string fleetno);
        QuestResource GetByCallsign(string callsign);
        int GetOffroadStatusId();
        ResourceUpdateResult Update(ResourceUpdate item);
        void Clear();
    }    
}