using System;

namespace Quest.Common.Messages
{
    [Serializable]
    
    public class AssignDeviceRequest : Request
    {
        /// <summary>
        ///     callsign to which this event has been dispatched
        /// </summary>
        
        public string Callsign { get; set; }

        /// <summary>
        ///     unique Id of the event
        /// </summary>
        
        public string EventId { get; set; }

        /// <summary>
        ///     dispatch to nearby resources
        /// </summary>
        
        public bool Nearby { get; set; }

        public override string ToString()
        {
            return $"AssignDeviceRequest EventId={EventId} Callsign={Callsign}";
        }
    }

}