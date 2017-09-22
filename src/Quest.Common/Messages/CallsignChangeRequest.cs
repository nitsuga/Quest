using System;

namespace Quest.Common.Messages
{
    /// <summary>
    ///     the device can submit a callsign request. The callsign may be igored. The device should
    ///     not assume it owns the callsign until it recieves a response. passing a blank callsign
    ///     effectively requests the current callsign (i.e. it doesn't delete the callsign).
    /// </summary>

    [Serializable]
    public class CallsignChangeRequest : Request
    {
        /// <summary>
        ///     request a change to this callsign, or obtain current callsign if left empty
        /// </summary>
        
        public string Callsign { get; set; }

        public override string ToString()
        {
            return $"CallsignChange Callsign={Callsign}";
        }
    }
}