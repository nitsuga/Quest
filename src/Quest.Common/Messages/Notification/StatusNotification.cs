using Quest.Common.Messages.Device;
using Quest.Common.Messages.Notification;
using System;

namespace Quest.Common.Messages.Notification
{
    [Serializable]
    
    public class StatusNotification : INotificationMessage
    {
        public StatusCode Status;

        public override string ToString()
        {
            return $"StatusNotification: {Status.Code}";
        }
    }    
}