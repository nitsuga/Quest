using Quest.Common.Messages.Notification;
using System;

namespace Quest.Common.Messages.Device
{
    [Serializable]
    
    public class CancellationNotification : INotificationMessage
    {        
        public string EventId { get; set; }
    }
    
}