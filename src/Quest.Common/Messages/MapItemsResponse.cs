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
        
        public List<ResourceItem> Resources { get; set; }

        
        public List<int> DeleteResources { get; set; }

        // these are children of the resources
        
        public List<ResourceItem> Devices { get; set; }

        
        public List<EventMapItem> Events { get; set; }

        
        public List<QuestDestination> Destinations { get; set; }

        // the revision returned
        
        public long Revision { get; set; }

        // the actual current revision
        
        public long CurrRevision { get; set; }
    }

    
}