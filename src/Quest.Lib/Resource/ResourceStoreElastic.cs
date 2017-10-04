using Quest.Common.Messages;

namespace Quest.Lib.Resource
{
    public class ResourceStoreElastic : IResourceStore
    {
        public QuestResource GetByCallsign(string callsign)
        {
            throw new System.NotImplementedException();
        }

        public QuestResource GetByFleetNo(string fleetno)
        {
            throw new System.NotImplementedException();
        }

        public QuestResource GetByResourceId(int resourceId)
        {
            throw new System.NotImplementedException();
        }

        public int GetOffroadStatusId()
        {
            return 0;
        }

        public QuestResource Update(ResourceUpdate item)
        {
            return null;
        }
    }
}
