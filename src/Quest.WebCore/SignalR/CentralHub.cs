﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Quest.Lib.DependencyInjection;
using Quest.Lib.Trace;
using System;

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

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
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
            await Groups.RemoveAsync(Context.ConnectionId, group);
            await Clients.All.InvokeAsync("leavegroup", user, group);
            Logger.Write($"LeaveGroup {user}->{group}");
        }

        public async Task JoinGroup(string user, string group)
        {
            await Groups.AddAsync(Context.ConnectionId, group);
            await Clients.All.InvokeAsync("joingroup", user, group);
            Logger.Write($"JoinGroup {user}->{group}");
        }

    }
}
