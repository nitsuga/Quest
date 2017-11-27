using System.Collections.Generic;

namespace Quest.WebCore.SignalR
{
    public interface IHubMessage
    {
        string UserId { get; set; }
        string PluginName { get; set; }
        string PluginInstanceName { get; set; }
        string EventRaised { get; set; }
        Dictionary<string, string> EventParameters { get; set; }
    }
}
