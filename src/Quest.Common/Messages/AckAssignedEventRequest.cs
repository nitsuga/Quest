using System;

namespace Quest.Common.Messages
{
    /// <summary>
    ///     Sent by a device 
    ///     Acknowledges that we will/wont be going to the event that has just been sent to us. The server, if it doesn't
    ///     receive this
    ///     message within some defined timespan, may elect to resend the event push
    /// </summary>
    [Serializable]    
    public class AckAssignedEventRequest : Request
    {
        /// <summary>
        ///     a flag indicating whether the device accepts the assignment
        /// </summary>        
        public bool Accept { get; set; }

        /// <summary>
        ///     an explicit indication of the event being accepted/rejected
        /// </summary>        
        public string EventId { get; set; }

        /// <summary>
        ///     a reason for accepting or not accepting
        /// </summary>        
        public string Reason { get; set; }

        public override string ToString()
        {
            return ""; // String.Format("EventUpdate EventId={0} Updated={1}", EventId, Updated);
        }
    }
}