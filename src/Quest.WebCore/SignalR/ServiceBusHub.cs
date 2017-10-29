using Quest.Common.ServiceBus;
using Quest.Lib.DependencyInjection;
using System;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Quest.WebCore.SignalR
{
    /// <summary>
    /// monitors the service bus and hib and passes messages on to web clients 
    /// via signalR CentralHub
    /// </summary>
    public class ServiceBusHub
    {
        public IServiceBusClient _msgSource;

        private IUserTracker _userTracker;
        private UserDetails[] users;
        private HubConnection _connection;

        private Dictionary<string, string[]> _groupMessages = new Dictionary<string, string[]>
        {
            { "ResourceUpdate", new string[] {"Resources.Available" } },
            { "IncidentUpdate", new string[] {"Incidents" } },
        };

        public ServiceBusHub(IServiceBusClient msgSource, IUserTracker userTracker)
        {
            _userTracker = userTracker;
            _msgSource = msgSource;
        }

        public void Initialise()
        {
            ConnectToCentralHub();
            ConnectToServiceBus();
        }

        private void ConnectToServiceBus()
        {
            // register on the service bus
            _msgSource.NewMessage += _msgSource_NewMessage;
        }

        private void ConnectToCentralHub()
        {
            var builder = new HubConnectionBuilder();

            _connection = builder
                .WithUrl("http://localhost:63147/hub")
                .WithMessagePackProtocol()
                .WithConsoleLogger()
                .Build();

            _connection.On<UserDetails[]>("UsersJoined", (parms) => UsersJoined(parms));
            _connection.On<UserDetails[]>("UsersLeft", (parms) => UsersLeft(parms));
            _connection.On<UserDetails[]>("UsersJoined", (parms) => { });
        }

        private void UsersJoined(UserDetails[] users)
        {
        }

        private void UsersLeft(UserDetails[] users)
        {
        }

        /// <summary>
        /// receive message from service bus and send to the
        /// relevant groups
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _msgSource_NewMessage(object sender, NewMessageArgs e)
        {
            // dispatch messages to users
            
            // look up message type in 
        }
    }
}
