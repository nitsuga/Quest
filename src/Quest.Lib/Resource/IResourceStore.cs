using Quest.Common.Messages;
using Quest.Common.Messages.CAD;
using Quest.Common.Messages.Resource;

namespace Quest.Lib.Resource
{
    public interface IResourceStore
    {
        bool FleetNoExists(string fleetno);
        QuestResource GetByFleetNo(string fleetno);
        QuestResource GetByCallsign(string callsign);
        int GetOffroadStatusId();
        ResourceUpdateResult Update(ResourceUpdateRequest item);
        void Clear();
    }    
}