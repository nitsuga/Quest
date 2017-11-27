using System;

namespace Quest.Common.Messages.Routing
{
    
    /// <summary>
    ///     A device requests that it is to be deregistered for activity. No more push messages
    ///     will be sent to the device and the AuthToken will be unauthorised. Subsequent messages
    ///     that contain the devices AuthToken will be responded to with an authorisation failure
    /// </summary>
    [Serializable]
    public class GetCoverageRequest : Request
    {
        /// <summary>
        /// name of the coverage layer to get
        /// </summary>
        public string Code;
    }    
}