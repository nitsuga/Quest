using System;

namespace Quest.Common.Messages
{
    [Serializable]
    
    public class CancellationNotification : INotificationMessage
    {        
        public string EventId { get; set; }
    }
    
}