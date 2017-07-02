using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.WebSockets;
using Quest.Mobile.Service;
using Quest.Lib.ServiceBus;

namespace Quest.Mobile.Controllers
{
    /// <summary>
    /// Manages resources updates provided via the xReplayPlayer
    /// </summary>
    //[EnableCors("*", "*", "*")]
    public class NotificationsController : ApiController
    {
        
        MessageCache _messageCache;
        ResourceService _resourceService;
        IncidentService _incidentService;

        public NotificationsController(MessageCache messageCache,
            ResourceService resourceService,
            IncidentService incidentService
        )
        {
            _messageCache = messageCache;
            _resourceService = resourceService;
            _incidentService = incidentService;
        }

        /// <summary>
        /// Initial connection commes in here. We pass a function to handle incoming connections.
        /// That function requires a class that will hold the incoming connection parameters sent
        /// from the client. In our case, this is a StateFlags class 
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage Get()
        {
            if (HttpContext.Current.IsWebSocketRequest)
            {
                ClientConnectionService clientService = new ClientConnectionService(_messageCache, _resourceService, _incidentService);

                Func<AspNetWebSocketContext, Task> userFunc = clientService.ProcessSocketRequest;
                //Func<WebSocket, Task> userFunc = clientService.ProcessSocketRequest;
                HttpContext.Current.AcceptWebSocketRequest(userFunc);
            }

            return new HttpResponseMessage(HttpStatusCode.SwitchingProtocols);
        }

   }
}
