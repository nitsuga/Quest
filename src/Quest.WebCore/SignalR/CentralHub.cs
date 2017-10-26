using System;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Quest.Lib.DependencyInjection;
using System.Threading;
using System.Net.WebSockets;
using Quest.Common.Messages;
using Quest.Lib.ServiceBus;
using Quest.Common.ServiceBus;

namespace Quest.WebCore.SignalR
{
    [Injection()]
    public class CentralHub : HubWithPresence
    {
        Thread timer;
        AsyncMessageCache _messageCache;

        public CentralHub(IUserTracker userTracker, AsyncMessageCache messageCache)
            : base(userTracker)
        {
            _messageCache = messageCache;
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

        public IObservable<IServiceBusMessage> StreamMessages()
        {
            return Observable.Create(
                async (IObserver<IServiceBusMessage> observer) =>
                {
                    _messageCache.MsgSource.NewMessage += (x,y)=> {
                        observer.OnNext(y.Payload);                            
                    };
                    await Task.Delay(10);
                });
        }

        private void MsgSource_NewMessage(object sender, Common.ServiceBus.NewMessageArgs e)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<String> GetAllStreams()
        {
            return new List<string> { "Resources", "Incidents" };
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
