using Autofac;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Quest.Lib.DependencyInjection;
using Quest.Lib.ServiceBus;
using Quest.WebCore.Interfaces;
using Quest.WebCore.Services;
using System;
using System.Collections.Generic;
using System.IO;

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

        public RTMController(AsyncMessageCache messageCache,
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
            _messageCache = messageCache;
            _resourceService = resourceService;
            _incidentService = incidentService;
            _destinationService = destinationService;
            _searchService = searchService;
            _routeService = routeService;
            _telephonyService = telephonyService;
            _securityService = securityService;
            _pluginService = pluginFactory;
        }

        [HttpGet]
        public ActionResult GetResources(bool avail = false, bool busy = false)
        {
            try
            {
                var r1 = _resourceService.GetResources(avail, busy);
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


        [HttpGet]
        public ActionResult GetIncidents(bool includeCatA = false, bool includeCatB = false)
        {
            try
            {
                var incs = _incidentService.GetIncidents(includeCatA, includeCatB);
                var js = JsonConvert.SerializeObject(incs);
                var result = new ContentResult
                {
                    Content = js,
                    ContentType = "application/json"
                };
                return result;
            }
            catch (Exception ex)
            {
                var errorMsg = "ERROR: Cannot complete Incidents" + ex.Message;
                var error = new ContentResult
                {
                    Content = errorMsg,
                    ContentType = "application/json"
                };

                return error;

                //throw new Exception("Couldn't get list of resources", ex);
            }
        }

    }

    /// <summary>
    /// This plugin is internal to the main Hud framework.
    /// It generates the Html presented to the user to allow them to select from a list of available plugins
    /// </summary>
    [Injection("RealtimeMapPlugin", typeof(IHudPlugin), Lifetime.PerDependency)]
    public class RealtimeMapPlugin : IHudPlugin
    {
        ILifetimeScope _scope;
        private IHostingEnvironment _env; 

        public RealtimeMapPlugin(ILifetimeScope scope, IHostingEnvironment env)
        {
            _scope = scope;
            _env = env;
            Properties = new Dictionary<string, object>();
        }

        /// <summary>
        /// The name of the plugin
        /// </summary>
        public string Name => "RealtimeMapPlugin"; // <-- must be the same as the injected Name

        public Dictionary<string, object> Properties { get; set; }

        public string MenuText => "MAP";

        public bool IsMenuItem => true;

        public string RenderHtml(string role)
        {
            return DrawContainer(role);
        }

        public string OnInit()
        {
            return "hud.plugins.rtmap.initMap(panelRole)";
        }

        public string OnPanelMoved()
        {
            return string.Empty;
        }

        public void InitializeWithProperties(Dictionary<string, object> properties)
        {
            // Do nothing
        }

        public void InitializeWithDefaultProperties()
        {
            // Do nothing
        }

        private string DrawContainer(string role)
        {
            const string templateFileName = "Map.html";
            var templateFolder = _env.WebRootPath + "/plugins/RealtimeMap/Lib";
            var gazHtml = File.ReadAllText($"{templateFolder}/{templateFileName}");
            gazHtml = gazHtml.Replace("id=mapdivplaceholder", $"id='map{role}'");
            return gazHtml;
        }
    }
}