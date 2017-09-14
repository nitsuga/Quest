using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Quest.Lib.ServiceBus;
using Quest.Lib.Trace;
using Quest.Common.Messages;
using System.Web.WebSockets;
using Quest.WebCore.Models;

namespace Quest.WebCore.Services
{
    /// <summary>
    /// handles connection back to client web sessions to push data
    /// </summary>
    public class ClientConnectionService
    {
        private MessageHandler _msgHandler;

        private SocketParams _client;

        private ResourceService _resourceService;

        private IncidentService _incidentService;

        private MessageCache _messageCache;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageCache"></param>
        public ClientConnectionService(MessageCache messageCache, ResourceService resourceService, IncidentService incidentService)
        {
            _messageCache = messageCache;
            _resourceService = resourceService;
            _incidentService = incidentService;
            _msgHandler = new MessageHandler();

            _msgHandler.AddHandler<CallLookupResponse>(CallLookupResponseHandler);
            _msgHandler.AddHandler<CallLookupRequest>(CallLookupRequestHandler);
            _msgHandler.AddHandler<ResourceDatabaseUpdate>(ResourceDatabaseUpdateHandler);
            _msgHandler.AddHandler<IncidentDatabaseUpdate>(IncidentDatabaseUpdateHandler);
            _msgHandler.AddHandler<IntelIncident>(IntelIncidentHandler);
            _msgHandler.AddHandler<IntelIncidentDelete>(IntelIncidentDeleteHandler);
            _msgHandler.AddHandler<RoutingEngineStatus>(RoutingEngineStatusHandler);


            // process the message using our message handler
            _messageCache.MsgSource.NewMessage +=
                (s, e) => _msgHandler.ProcessMessage(_messageCache.MsgSource, e);

            // ask for routing engine status
            _messageCache.MsgSource.Broadcast(new RoutingEngineStatusRequest());

        }


        /// <summary>
        /// Process an incoming request - this is a long running task that handling incoming requests
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task ProcessSocketRequest(AspNetWebSocketContext context)
        {
            var socket = context.WebSocket;
            var userId = context.LogonUserIdentity.Name;
            _client = new SocketParams()
            {
                Options = null,
                Socket = context,
            };


            // process socket messages
            while (true)
            {
                var buffer = new ArraySegment<byte>(new byte[1024]);
                var result = await socket.ReceiveAsync(buffer, CancellationToken.None);
                switch (socket.State)
                {
                    case WebSocketState.Connecting:
                    case WebSocketState.CloseSent:
                    case WebSocketState.None:
                        break;

                    case WebSocketState.Aborted:
                    case WebSocketState.Closed:
                    case WebSocketState.CloseReceived:
                        await
                            socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "close requested",
                                CancellationToken.None);
                        _msgHandler.Clear();
                        return;

                    case WebSocketState.Open:
                        var userMessage = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);

                        //this data contains request update parameters, update our flags
                        _client.Options = JsonConvert.DeserializeObject<StateFlags>(userMessage);
                        break;
                }
            }
        }

        /// <summary>
        /// TODO: LD => Sends messages to connected clients but is there no other elegant way ?
        /// </summary>
        public void SendToClient(object feature)
        {
            if (feature == null) throw new ArgumentNullException(nameof(feature));
            try
            {
                if (_client?.Socket != null && _client.Socket.WebSocket.State == WebSocketState.Open)
                {
                    var msg = JsonConvert.SerializeObject(feature, Formatting.None,
                        new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects });

                    var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg));

                    Logger.Write($"Sending {feature} to {_client.Socket}", GetType().Name);
                    _client.Socket.WebSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
            catch (ObjectDisposedException ex)
            {
                Logger.Write("Socket client disconnected - " + ex.Message, GetType().Name);
                _msgHandler.Clear();
            }
        }

        Response RoutingEngineStatusHandler(NewMessageArgs e)
        {
            var update = e.Payload as RoutingEngineStatus;
            SendToClient(update);
            return null;
        }

        Response CallLookupResponseHandler(NewMessageArgs e)
        {
            var update = e.Payload as CallLookupResponse;
            SendToClient(update);
            return null;
        }

        Response CallLookupRequestHandler(NewMessageArgs e)
        {
            var update = e.Payload as CallLookupRequest;
            SendToClient(update);
            return null;
        }

        /// <summary>
        /// recieved a notification that resource change details have been written to the database
        /// onward send to the clients
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        Response ResourceDatabaseUpdateHandler(NewMessageArgs e)
        {
            var update = e.Payload as ResourceDatabaseUpdate;

            if (update != null && update.Item != null && _client != null)
            {
                StateFlags flags = _client.Options as StateFlags;

                if (flags == null)
                    return null;

                if ((flags.Avail && update.Item.Available) || (flags.Busy && update.Item.Busy))
                {
                    var resFeature = _resourceService.GetResourceUpdateFeature(update);
                    SendToClient(resFeature);
                }

                // state changed to not available or busy
                if (update.Item.PrevStatus != update.Item.Status)
                {
                    var delFeature = _resourceService.GetResourceDeleteFeature(update);
                    SendToClient(delFeature);
                }
            }
            return null;
        }

        /// <summary>
        /// recieved a notification that incident change details have been written to the database
        /// onward send to the clients
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        Response IncidentDatabaseUpdateHandler(NewMessageArgs e)
        {
            var update = e.Payload as IncidentDatabaseUpdate;
            if (update != null && update.Item != null)
            {
                // send to clients
                StateFlags flags = _client.Options as StateFlags;
                if (flags != null)
                {
                    if (flags.CatA && Convert.ToBoolean(update.Item.Priority.StartsWith("R")) ||
                        flags.CatA && Convert.ToBoolean(!update.Item.Priority.StartsWith("R")))
                    {
                        var feature = IncidentService.GetIncidentUpdateFeature(update.Item);
                        SendToClient(feature);
                    }
                }
            }
            return null;
        }

        Response IntelIncidentHandler(NewMessageArgs e)
        {
            var update = e.Payload as IntelIncident;
            if (update == null) return null;
            // send to clients
            var flags = _client.Options as StateFlags;
            if (flags != null)
            {
                SendToClient(update);
            }
            return null;
        }

        Response IntelIncidentDeleteHandler(NewMessageArgs e)
        {
            var update = e.Payload as IntelIncidentDelete;
            if (update == null) return null;
            // send to clients
            var flags = _client.Options as StateFlags;
            if (flags != null)
            {
                SendToClient(update);
            }
            return null;
        }


        private class SocketParams
        {
            public object Options { get; set; }
            public AspNetWebSocketContext Socket { get; set; }
        }
    }
}
