using Quest.Common.Messages.GIS;
using System;

namespace Quest.Common.Messages.Device
{
    /// <summary>
    ///     an single item of recorded history for a specific event and deviceId
    /// </summary>
    [Serializable]
    
    public class DeviceHistoryItem
    {
        /// <summary>
        ///     The time (utc) when this event occurred
        /// </summary>        
        public DateTime TimeStamp { get; set; }

        /// <summary>
        ///     callsign to which this hostory item relates
        /// </summary>        
        public string Callsign { get; set; }

        /// <summary>
        ///     event id of this history item
        /// </summary>        
        public string EventId { get; set; }

        /// <summary>
        ///     unique device id
        /// </summary>        
        public string DeviceId { get; set; }

        /// <summary>
        ///     The status of resource
        /// </summary>        
        public string Status { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string StatusGroup { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public LocationVector Vector { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Notes { get; set; } // other notable items such as EventUpdate, sent position report

        public override string ToString()
        {
            return ""; // String.Format("EventUpdate EventId={0} Updated={1}", EventId, Updated);
        }
    }

}