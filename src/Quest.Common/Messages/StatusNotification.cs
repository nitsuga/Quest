using System;

namespace Quest.Common.Messages
{
    [Serializable]
    
    public class StatusNotification : IDeviceNotification
    {
        public StatusCode Status;

        public override string ToString()
        {
            return $"StatusNotification: {Status.Code}";
        }
    }    
}