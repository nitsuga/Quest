using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Quest.Lib.DependencyInjection;
using System.Threading;
using System.Net.WebSockets;

namespace Quest.WebCore.SignalR
{
    [Injection()]
    public class CentralHub : HubWithPresence
    {
        Thread timer;

        public CentralHub(IUserTracker userTracker)
            : base(userTracker)
        {
            timer = new Thread(new ThreadStart(() =>
            {
                //var client = new ClientWebSocket();
                //client.ConnectAsync("http://localhost/hub", CancellationToken.None);
                //Task.WhenAll(Receu)
                //Microsoft.AspNetCore.SignalR.Client.HubConnection =new Microsoft.AspNetCore.SignalR.Client.HubConnection()
                //this.Send
                Timer t = new Timer((x) =>
                {
                    // Clients.All.InvokeAsync("Send", DateTime.Now.ToLongTimeString());
                }, null, 3000, 3000);
            }));

            timer.Start();

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
            //Context.User.Identity.Name
            await Clients.All.InvokeAsync("Send", user, message);
        }

        public async Task LeaveGroup(string user, string group)
        {
            await Groups.RemoveAsync(user, group);
        }

        public async Task JoinGroup(string user, string group)
        {
            await Groups.AddAsync(user, group);

            //Context.User.Identity.Name
            //await Clients.All.InvokeAsync("Send", user, message);
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
