using System.Collections.Generic;
using Quest.Common.Messages.Resource;

namespace Quest.Lib.Resource
{
    public class ResourceStoreElastic : IResourceStore
    {
        public bool FleetNoExists(string fleetno)
        {
            throw new System.NotImplementedException();
        }

        public QuestResource GetByCallsign(string callsign)
        {
            throw new System.NotImplementedException();
        }

        public QuestResource GetByFleetNo(string fleetno)
        {
            throw new System.NotImplementedException();
        }

        public QuestResource GetByCallsign(int Callsign)
        {
            throw new System.NotImplementedException();
        }

        public int GetOffroadStatusId()
        {
            return 0;
        }

        public QuestResource Update(ResourceUpdateRequest item)
        {
            return null;
        }

        public void Clear()
        {
            throw new System.NotImplementedException();
        }

        ResourceUpdateResult IResourceStore.Update(ResourceUpdateRequest item)
        {
            throw new System.NotImplementedException();
        }

        public string GetStatusDescription(bool available, bool busy, bool enroute, bool rest)
        {
            throw new System.NotImplementedException();
        }

        public List<QuestResource> GetResources(long revision, string[] resourceGroups, bool avail = false, bool busy = false)
        {
            throw new System.NotImplementedException();
        }

        public ResourceAssignmentStatus UpdateResourceAssign(ResourceAssignmentStatus item)
        {
            throw new System.NotImplementedException();
        }

        List<ResourceAssignmentStatus> IResourceStore.GetAssignmentStatus()
        {
            throw new System.NotImplementedException();
        }

        public ResourceAssignmentStatus UpdateResourceAssign(ResourceAssignmentUpdate item)
        {
            throw new System.NotImplementedException();
        }
    }
}
