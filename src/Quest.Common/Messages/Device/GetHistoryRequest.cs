using System;

namespace Quest.Common.Messages.Device
{
    /// <summary>
    ///     request an audit history for a specific device. This is useful for crew
    ///     that need to fill in paperwork but also useful for managers tracking
    ///     device activity.
    /// </summary>
    [Serializable]
    
    public class GetHistoryRequest : Request
    {
        
        public DateTime FromTime { get; set; }

        
        public DateTime ToTime { get; set; }

        
        public int MaxItems { get; set; }

        public override string ToString()
        {
            return ""; // String.Format("EventUpdate EventId={0} Updated={1}", EventId, Updated);
        }
    }

    
}