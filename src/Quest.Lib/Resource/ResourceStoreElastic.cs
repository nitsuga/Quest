using Quest.Common.Messages;
using Quest.Common.Messages.CAD;
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
    }
}
