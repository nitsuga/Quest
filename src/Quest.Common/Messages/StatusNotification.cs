using System;

namespace Quest.Common.Messages.Device
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