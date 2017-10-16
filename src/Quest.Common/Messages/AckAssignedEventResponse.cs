using System;

namespace Quest.Common.Messages
{
    /// <summary>
    ///     Send by the server to a device in response to AckAssignedRequest
    /// </summary>
    [Serializable]    
    public class AckAssignedEventResponse : Response
    {
    }
}