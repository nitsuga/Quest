using System;

namespace Quest.Common.Messages
{
    /// <summary>
    ///     In response to a callsign change this class sets the new callsign for a device.
    /// </summary>

    [Serializable]
    public class CallsignChangeResponse : Response
    {
        /// <summary>
        ///     The old callsign
        /// </summary>        
        public string OldCallsign { get; set; }

        /// <summary>
        ///     the new callsign assigned to this device
        /// </summary>        
        public string NewCallsign { get; set; }
    }
}