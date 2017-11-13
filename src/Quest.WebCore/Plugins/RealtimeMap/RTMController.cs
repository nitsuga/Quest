using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Quest.Common.Messages.GIS;
using Quest.Common.Messages.Resource;
using Quest.Lib.ServiceBus;
using Quest.WebCore.Services;
using System;
using System.Threading.Tasks;

namespace Quest.WebCore.Plugins.RealtimeMap
{
    public class RTMController : Controller
    {
        private AsyncMessageCache _messageCache;
        private readonly IPluginService _pluginService;
        private readonly RealtimeMapPlugin _plugin;

        public RTMController(
                RealtimeMapPlugin plugin,
                AsyncMessageCache messageCache,
                IPluginService pluginService
            )
        {
            _plugin = plugin;
            _pluginService = pluginService;
            _messageCache = messageCache;
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
        public async Task<ActionResult> GetMapItems(MapItemsRequest request)
        {
            try
            {
                var results = await _messageCache.SendAndWaitAsync<MapItemsResponse>(request, new TimeSpan(0, 0, 10));
                var js = JsonConvert.SerializeObject(results);
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