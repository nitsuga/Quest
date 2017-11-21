using Quest.Common.Messages.Incident;
using System.Collections.Generic;

namespace Quest.Lib.Incident
{
    public interface IIncidentStore
    {
        void Close(string serial);
        QuestIncident Get(string id);
        QuestIncident Update(IncidentUpdateRequest item);
        List<QuestIncident> GetIncidents(long revision, bool includeCatA = false, bool includeCatB = false);
    }
}