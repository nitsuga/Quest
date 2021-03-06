﻿using System;

namespace Quest.Common.Messages.GIS
{
    /// <summary>
    ///     sent by a device to request nearby locations
    /// </summary>
    [Serializable]    
    public class MapItemsRequest : Request
    {
        public string[] ResourceGroups { get; set; }

        public bool ResourcesAvailable { get; set; }
        
        public bool ResourcesBusy { get; set; }
        
        public bool IncidentsImmediate { get; set; }
       
        public bool IncidentsOther { get; set; }
        
        public bool Hospitals { get; set; }
        
        public bool Standby { get; set; }

        public bool Stations { get; set; }

        public bool ResourceToSbp { get; set; }

        /// <summary>
        /// Only return items changed after this revision number
        /// </summary>
        public long Revision { get; set; }

        public override string ToString()
        {
            return ""; // String.Format("EventUpdate EventId={0} Updated={1}", EventId, Updated);
        }
    }

    
}