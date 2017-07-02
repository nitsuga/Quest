using System;

namespace Quest.Common.Messages
{
    [Serializable]
    
    public class MessageNotification : IDeviceNotification
    {
        
        public MessageBody messageBody { get; set; }
    }

    
}