using Quest.Common.Messages.Destination;
using Quest.Common.Messages.Incident;
using Quest.Common.Messages.Resource;
using System;
using System.Collections.Generic;

namespace Quest.Common.Messages.GIS
{
    /// <summary>
    ///     contains a list of locations that are near to a device.
    /// </summary>
    [Serializable]
    
    public class MapItemsResponse : Response
    {
        public List<QuestResource> Resources;
        public List<QuestIncident> Incidents;
        public List<QuestDestination> Destinations;

        // the revision returned
        public long Revision;

        // the actual current revision
        public long CurrRevision;
    }

    
}