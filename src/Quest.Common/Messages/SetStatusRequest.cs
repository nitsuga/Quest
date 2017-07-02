using System;

namespace Quest.Common.Messages
{
    /// <summary>
    ///     A device can request a change to a specific status code
    /// </summary>
    [Serializable]
    
    public class SetStatusRequest : Request
    {
        
        public string StatusCode { get; set; }

        
        public string CorrespondingEventId { get; set; }

        public override string ToString()
        {
            return ""; // String.Format("EventUpdate EventId={0} Updated={1}", EventId, Updated);
        }
    }

    
}