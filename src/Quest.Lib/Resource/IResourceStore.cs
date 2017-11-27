using Quest.Common.Messages.Resource;
using System.Collections.Generic;

namespace Quest.Lib.Resource
{
    public interface IResourceStore
    {
        bool FleetNoExists(string fleetno);
        QuestResource GetByFleetNo(string fleetno);
        QuestResource GetByCallsign(string callsign);
        List<QuestResource> GetResources(long revision, string[] resourceGroups, bool avail, bool busy);
        string GetStatusDescription(bool available, bool busy, bool enroute, bool rest);
        int GetOffroadStatusId();
        ResourceUpdateResult Update(ResourceUpdateRequest item);
        void Clear();
        ResourceAssignments GetAssignmentStatus();
        ResourceAssignmentStatus UpdateResourceAssign(ResourceAssignmentStatus item);
    }
}