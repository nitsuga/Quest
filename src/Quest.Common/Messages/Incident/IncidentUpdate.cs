using Quest.Common.Messages.GIS;
using System;

namespace Quest.Common.Messages.Incident
{
    /// <summary>
    /// indicates that an incident update has occurred
    /// </summary>
    [Serializable]
    public class IncidentUpdate : Request
    {
        public string serial;
        public QuestIncident Item;
    }
}