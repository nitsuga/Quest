using System;

namespace Quest.LAS.Messages
{
    public class AdminMessage: ICadMessage
    {
        public AdminMessageTypeEnum AdminMessageType { get; set; }
        public DateTime PingTime { get; set; }
    }

}
