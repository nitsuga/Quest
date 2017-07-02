using System;

namespace Quest.Common.Messages
{
    [Serializable]
    
    public class SendMessageRequest : Request
    {
        
        public MessageBody messageBody { get; set; }

        public override string ToString()
        {
            return "";
        }
    }

    
}