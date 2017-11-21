using Quest.Common.Messages.Device;
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