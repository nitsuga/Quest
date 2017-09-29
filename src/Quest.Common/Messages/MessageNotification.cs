using System;

namespace Quest.Common.Messages
{
    [Serializable]    
    public class MessageNotification : INotificationMessage
    {
        public String Text { get; set; }
        public int Priority { get; set; }
    }
}