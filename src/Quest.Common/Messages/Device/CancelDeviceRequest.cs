using System;

namespace Quest.Common.Messages.Device
{
    [Serializable]
    
    public class CancelDeviceRequest : Request
    {
        /// <summary>
        ///     callsign to which this event has been cancelled
        /// </summary>
        
        public string Callsign { get; set; }

        /// <summary>
        ///     unique Id of the event
        /// </summary>
        
        public string EventId { get; set; }

        public override string ToString()
        {
            return $"CancelDeviceRequest EventId={EventId} Callsign={Callsign}";
        }
    }
}