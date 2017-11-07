using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Quest.Common.Messages.GIS;
using Quest.Common.Messages.Resource;
using Quest.Lib.ServiceBus;
using Quest.WebCore.Services;
using System;

namespace Quest.WebCore.Plugins.RealtimeMap
{
    public class RTMController : Controller
    {
        private AsyncMessageCache _messageCache;
        private ResourceService _resourceService;
        private IncidentService _incidentService;
        private DestinationService _destinationService;
        private SearchService _searchService;
        private RouteService _routeService;
        private TelephonyService _telephonyService;
        private SecurityService _securityService;
        private readonly IPluginService _pluginService;
        RealtimeMapPlugin _plugin;

        public RTMController(
                RealtimeMapPlugin plugin,
                AsyncMessageCache messageCache,
                IPluginService pluginFactory,
                ResourceService resourceService,
                IncidentService incidentService,
                DestinationService destinationService,
                SearchService searchService,
                RouteService routeService,
                TelephonyService telephonyService,
                VisualisationService visualisationService,
                SecurityService securityService
            )
        {
            _plugin = plugin;
            _resourceService = resourceService;
        }

        [HttpGet]
        public MapSettings GetSettings()
        {
            return _plugin.GetSettings();
        }

        /// <summary>
        /// Get selected map items to display
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetMapItems(MapItemsRequest request)
        {
            try
            {
                var r1 = _resourceService.GetMapItems(request);
                var js = JsonConvert.SerializeObject(r1);
                var result = new ContentResult
                {
                    Content = js,
                    ContentType = "application/json"
                };

                return result;
            }
            catch (Exception ex)
            {
                var js = JsonConvert.SerializeObject(new { error = ex.Message });
                var result = new ContentResult
                {
                    Content = js,
                    ContentType = "application/json"
                };
                return result;

            }
        }

        [HttpPost]
        public ActionResult AssignResource(ResourceAssign request)
        {
            return null;
        }
    }

    [Serializable]
    public class MapSettings
    {
        public string MapServer;
        public double Zoom;
        public double Latitude;
        public double Longitude;
    }
}