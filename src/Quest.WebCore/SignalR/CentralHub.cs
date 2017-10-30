using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Quest.Lib.DependencyInjection;
using Quest.Lib.Trace;

namespace Quest.WebCore.SignalR
{
    /// <summary>
    ///  central hub where messages from clients arrive and are dispatched to other users
    ///  users may join arbitary groups.
    /// </summary>
    [Injection()]
    public class CentralHub : HubWithPresence
    {
        public CentralHub(IUserTracker userTracker)
            : base(userTracker)
        {
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.Client(Context.ConnectionId).InvokeAsync("setusersonline", await GetUsersOnline());
            await base.OnConnectedAsync();
        }

        public override Task OnUsersJoined(UserDetails[] users)
        {
            return Clients.Client(Context.ConnectionId).InvokeAsync("usersjoined", users);
        }

        public override Task OnUsersLeft(UserDetails[] users)
        {
            return Clients.Client(Context.ConnectionId).InvokeAsync("usersleft", users);
        }

        public async Task Send(string user, string message)
        {
            await Clients.All.InvokeAsync("send", user, message);
        }

        public async Task GroupMessage(string user, string group, string message)
        {
            var grp = Clients.Group(group);
            await grp?.InvokeAsync("groupmessage", user, group, message);
            Logger.Write($"GroupMessage {user}->{group}->{message}");
        }

        public async Task LeaveGroup(string user, string group)
        {
            await Groups.RemoveAsync(user, group);
            await Clients.All.InvokeAsync("leavegroup", user, group);
            Logger.Write($"LeaveGroup {user}->{group}");
        }

        public async Task JoinGroup(string user, string group)
        {
            await Groups.AddAsync(user, group);
            await Clients.All.InvokeAsync("joingroup", user, group);
            Logger.Write($"JoinGroup {user}->{group}");
        }

    }

    public interface IHubMessage
    {
        string UserId { get; set; }
        string PluginName { get; set; }
        string PluginInstanceName { get; set; }
        string EventRaised { get; set; }
        Dictionary<string, string> EventParameters { get; set; }
    }

    public class HubMessage : IHubMessage
    {
        public string UserId { get; set; }
        public string PluginName { get; set; }
        public string PluginInstanceName { get; set; }
        public string EventRaised { get; set; }
        public Dictionary<string, string> EventParameters { get; set; }
    }
}
