using Quest.Common.Messages.GIS;
using System;

namespace Quest.Common.Messages.Device
{
    /// <summary>
    ///     The device can, at its discretion, send position reports back to the server.
    /// </summary>
    [Serializable]
    
    public class PositionUpdateRequest : Request
    {
        public PositionUpdateRequest()
        {
            Vector = new LocationVector();
        }

        /// <summary>
        ///     respresents an estimated position and speed vector of the devices
        /// </summary>
        
        public LocationVector Vector { get; set; }

        public override string ToString()
        {
            return ""; // String.Format("EventUpdate EventId={0} Updated={1}", EventId, Updated);
        }
    }

    
}