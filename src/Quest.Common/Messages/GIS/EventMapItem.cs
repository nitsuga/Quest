using Quest.Common.Messages.Incident;
using System;

namespace Quest.Common.Messages.GIS
{

    /// <summary>
    ///     an item listed in a nearby search.
    /// </summary>
    [Serializable]
    public class IncidentItem: PointMapItem
    {
        public QuestIncident Incident;
    }
}