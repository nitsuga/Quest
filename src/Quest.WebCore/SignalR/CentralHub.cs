using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Quest.Lib.DependencyInjection;
using System.Threading;
using Quest.Lib.ServiceBus;

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

            await Clients.Client(Context.ConnectionId).InvokeAsync("SetUsersOnline", await GetUsersOnline());
            await base.OnConnectedAsync();
        }

        public override Task OnUsersJoined(UserDetails[] users)
        {
            return Clients.Client(Context.ConnectionId).InvokeAsync("UsersJoined", users);
        }

        public override Task OnUsersLeft(UserDetails[] users)
        {
            return Clients.Client(Context.ConnectionId).InvokeAsync("UsersLeft", users);
        }

        public async Task Send(string user, string message)
        {
            await Clients.All.InvokeAsync("Send", user, message);
        }

        public async Task LeaveGroup(string user, string group)
        {
            await Groups.RemoveAsync(user, group);
            await Clients.All.InvokeAsync("LeaveGroup", user);
        }

        public async Task JoinGroup(string user, string group)
        {
            await Groups.AddAsync(user, group);
            await Clients.All.InvokeAsync("JoinGroup", user);
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
