using System;

namespace Quest.Common.Messages
{
    /// <summary>
    ///     sent by a device to request nearby locations
    /// </summary>
    [Serializable]
    
    public class MapItemsRequest : Request
    {
        public bool ResourcesAvailable { get; set; }
        
        public bool ResourcesBusy { get; set; }
        
        public bool IncidentsImmediate { get; set; }
       
        public bool IncidentsOther { get; set; }
        
        public bool Hospitals { get; set; }
        
        public bool Standby { get; set; }

        public bool Stations { get; set; }

        public long Revision { get; set; }

        public override string ToString()
        {
            return ""; // String.Format("EventUpdate EventId={0} Updated={1}", EventId, Updated);
        }
    }

    
}