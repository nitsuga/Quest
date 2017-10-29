using Quest.Common.Messages.Notification;
using System;

namespace Quest.Common.Messages.Device
{
    [Serializable]
    
    public class CallsignNotification : INotificationMessage
    {
        
        public string Callsign { get; set; }

        public override string ToString()
        {
            return $"CallsignNotification: {Callsign}";
        }
    }
    
}