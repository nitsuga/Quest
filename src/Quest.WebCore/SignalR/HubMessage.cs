using System.Collections.Generic;

namespace Quest.WebCore.SignalR
{
    public class HubMessage : IHubMessage
    {
        public string UserId { get; set; }
        public string PluginName { get; set; }
        public string PluginInstanceName { get; set; }
        public string EventRaised { get; set; }
        public Dictionary<string, string> EventParameters { get; set; }
    }
}
