using Quest.Common.Messages;
using Quest.Common.Messages.Incident;

namespace Quest.Lib.Incident
{
    public interface IIncidentStore
    {
        void Close(string serial);
        QuestIncident Get(string id);
        QuestIncident Update(IncidentUpdate item);
    }
}