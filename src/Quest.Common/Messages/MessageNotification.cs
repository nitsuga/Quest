using System;

namespace Quest.Common.Messages
{
    [Serializable]
    
    public class MessageNotification : INotificationMessage
    {
        
        public MessageBody messageBody { get; set; }
    }

    
}