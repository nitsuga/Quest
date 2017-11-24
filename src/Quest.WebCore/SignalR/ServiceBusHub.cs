using Quest.Common.ServiceBus;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.SignalR;
using Quest.Common.Messages.Resource;
using Quest.Common.Messages.Incident;
using Quest.Lib.Trace;
using Newtonsoft.Json;

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
        //private UserDetails[] users;
        private HubConnection _connection;

        public ServiceBusHub(IServiceBusClient msgSource, IUserTracker userTracker)
        {
            _userTracker = userTracker;
            _msgSource = msgSource;
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

            _connection = builder
                .WithUrl("http://localhost:63147/hub")
                .WithConsoleLogger()
                .Build();

            _connection.On<UserDetails[]>("usersjoined", (parms) => UsersJoined(parms));
            _connection.On<UserDetails[]>("usersleft", (parms) => UsersLeft(parms));
            _connection.On<string, string>("send", (name, message) => { });
            _connection.On<string, string>("leavegroup", (name, group) => { });
            _connection.On<string, string>("joingroup", (name, group) => { });
            _connection.On<string, string, string>("groupmessage", (name, group, message) => { });

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

                    var json = JsonConvert.SerializeObject(resource, new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All
                    });

                    var group = $"Resource.{resource.Item.StatusCategory}";
                    _connection.InvokeAsync("groupmessage", "ServiceBusHub", group, json);
                    break;

                case "ResourceStatusChange":
                    // ESB notifies us that a resource has changed its status
                    var resource_status_update = e.Payload as ResourceStatusChange;

                    // only interested in notifying major changes e.g. Available->Busy , not just AOR->ASB
                    if (resource_status_update.NewStatusCategory != resource_status_update.OldStatusCategory)
                    {

                        var json1 = JsonConvert.SerializeObject(resource_status_update, new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.All
                        });

                        // broadcast notification to both client groups
                        var oldgroup = $"Resource.{resource_status_update.OldStatusCategory}";
                        if (oldgroup.Length > 0)
                            _connection.InvokeAsync("groupmessage", "ServiceBusHub", oldgroup, json1);

                        var newgroup = $"Resource.{resource_status_update.NewStatusCategory}";
                        if (newgroup.Length > 0)
                            _connection.InvokeAsync("groupmessage", "ServiceBusHub", newgroup, json1);
                    }
                    break;

                case "IncidentUpdate":
                    var incident = e.Payload as IncidentUpdate;
                    var priority = $"Resource.{incident.Item.Priority}";
                    _connection.InvokeAsync("groupmessage", "ServiceBusHub", priority, incident);
                    break;

                case "ResourceAssignmentChanged":
                    var assignments = e.Payload as ResourceAssignmentChanged;
                    _connection.InvokeAsync("groupmessage", "ServiceBusHub", "ResourceAssignments", assignments.Items);
                    break;
            }
        }
    }
}
