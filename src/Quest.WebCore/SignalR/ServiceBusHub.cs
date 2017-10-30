using Quest.Common.ServiceBus;
using Quest.Lib.DependencyInjection;
using System;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quest.Common.Messages.Resource;
using Quest.Common.Messages.Incident;
using Quest.Lib.Trace;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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

        ~ServiceBusHub()
        {

        }

        public void Initialise(string queue)
        {
            ConnectToCentralHub();
            ConnectToServiceBus(queue);
        }

        private void ConnectToServiceBus(string queue)
        {
            _msgSource.Initialise(queue);
            // register on the service bus
            _msgSource.NewMessage += _msgSource_NewMessage;
        }

        private void ConnectToCentralHub()
        {
            var builder = new HubConnectionBuilder();

            //JsonSerializerSettings json_settings = new JsonSerializerSettings
            //{
            //    ContractResolver = new CamelCasePropertyNamesContractResolver(),
            //    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            //    //TypeNameHandling = TypeNameHandling.All,
            //    //TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            //};

            _connection = builder
                .WithUrl("http://localhost:63147/hub")
                .WithConsoleLogger()
                //.WithJsonProtocol(json_settings)
                .Build();

            _connection.On<UserDetails[]>("usersjoined", (parms) => UsersJoined(parms));
            _connection.On<UserDetails[]>("usersleft", (parms) => UsersLeft(parms));
            _connection.On<UserDetails[]>("send", (parms) => { });
            _connection.On<UserDetails[]>("leavegroup", (parms) => { });
            _connection.On<UserDetails[]>("joingroup", (parms) => { });
            _connection.On<UserDetails[]>("groupmessage", (parms) => { });

            _connection.StartAsync();
        }

        private void UsersJoined(UserDetails[] users)
        {
            Logger.Write($"UsersJoined");
        }

        private void UsersLeft(UserDetails[] users)
        {
            Logger.Write($"UsersLeft");
        }

        /// <summary>
        /// receive message from service bus and send to the
        /// relevant groups
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _msgSource_NewMessage(object sender, NewMessageArgs e)
        {
            Logger.Write($"Got {e.Metadata.MsgType}");

            switch( e.Metadata.MsgType)
            {
                // dispatch messages to users
                // look up message type in 
                case "ResourceUpdate":
                    var resource = e.Payload as ResourceUpdate;

                    var json = JsonConvert.SerializeObject(resource, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                    var group = $"Resource.{resource.Item.Resource.StatusCategory}";
                    _connection.InvokeAsync("groupmessage", "ServiceBusHub", group, json);
                    break;

                case "IncidentUpdate":
                    var incident = e.Payload as IncidentUpdate;
                    var priority = $"Resource.{incident.Item.Incident.Priority}";
                    _connection.InvokeAsync("groupmessage", "ServiceBusHub", priority, incident);
                    break;
            }
        }
    }
}
