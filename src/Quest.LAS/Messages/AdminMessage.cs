using System;

namespace Quest.LAS.Messages
{
    public class AdminMessage
    {
        public AdminMessageTypeEnum AdminMessageType { get; set; }
        public DateTime PingTime { get; set; }
    }

}
