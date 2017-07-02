using Quest.Common.Messages;

namespace Quest.Lib.Resource
{
    public interface IResourceStore
    {
        QuestResource Get(int fleetno);
        QuestResource Get(string callsign);
        int GetOffroadStatusId();
        QuestResource Update(ResourceUpdate item);
    }    
}