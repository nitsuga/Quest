using System;
using System.Collections.Generic;

namespace Quest.Common.Messages
{
    /// <summary>
    ///     contains a list of locations that are near to a device.
    /// </summary>
    [Serializable]
    
    public class MapItemsResponse : Response
    {

        public List<ResourceItem> Resources;
        // these are children of the resources
        //public List<DeviceItem> Devices;
        public List<EventMapItem> Events;
        public List<QuestDestination> Destinations;

        // the revision returned
        public long Revision;

        // the actual current revision
        public long CurrRevision;
    }

    
}