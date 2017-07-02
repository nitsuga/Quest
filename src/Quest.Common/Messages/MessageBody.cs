using System;

namespace Quest.Common.Messages
{
    [Serializable]
    
    public class MessageBody
    {
        
        public DateTime Sent { get; set; }

        
        public string Destination { get; set; }

        
        public string Source { get; set; }

        
        public string Message { get; set; }

        
        public string MessageType { get; set; }
    }

    
}