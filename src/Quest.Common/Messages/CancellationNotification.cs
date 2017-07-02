using System;

namespace Quest.Common.Messages
{
    [Serializable]
    
    public class CancellationNotification : IDeviceNotification
    {
        
        public string EventId { get; set; }
    }

    
}