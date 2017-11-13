using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Quest.Common.Messages.GIS;
using Quest.Lib.ServiceBus;
using Quest.WebCore.Services;
using System;

namespace Quest.WebCore.Plugins.RealtimeMap
{
    public class AACController : Controller
    {
        private AsyncMessageCache _messageCache;
        private IncidentService _incidentService;
        private DestinationService _destinationService;
        private TelephonyService _telephonyService;
        private SecurityService _securityService;
        private readonly IPluginService _pluginService;

        public AACController(AsyncMessageCache messageCache,
                IPluginService pluginFactory,
                IncidentService incidentService,
                DestinationService destinationService,
                TelephonyService telephonyService,
                VisualisationService visualisationService,
                SecurityService securityService
            )
        {
            _messageCache = messageCache;
            _incidentService = incidentService;
            _destinationService = destinationService;
            _telephonyService = telephonyService;
            _securityService = securityService;
            _pluginService = pluginFactory;
        }

        /// <summary>
        /// Get selected map items to display
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetAssignmentStatus()
        {
            try
            {
                //var r1 = _resourceService.GetMapItems(request);
                //var js = JsonConvert.SerializeObject(r1);
                var result = new ContentResult
                {
                //    Content = js,
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
    }
}