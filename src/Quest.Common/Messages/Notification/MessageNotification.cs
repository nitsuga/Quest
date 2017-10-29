using Newtonsoft.Json;
using System;

namespace Quest.Common.Messages.Notification
{
    [Serializable]    
    public class MessageNotification : INotificationMessage
    {
        [JsonProperty("title")]
        public String Title { get; set; }
        [JsonProperty("silent")]
        public bool Silent { get; set; }
        [JsonProperty("message")]
        public String Message { get; set; }
        [JsonProperty("priority")]
        public int Priority { get; set; }
    }
}