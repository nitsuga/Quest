using Quest.Common.Messages.GIS;
using System;

namespace Quest.Common.Messages.Incident
{
    /// <summary>
    /// indicatesthat a database update has occurred
    /// </summary>
    [Serializable]
    public class IncidentDatabaseUpdate : Request
    {
        public string serial;
        public EventMapItem Item;
    }
}