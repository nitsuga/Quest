using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Quest.Common.Messages.Gazetteer;
using Quest.Common.Messages.GIS;
using Quest.Common.Messages.Resource;
using Quest.Common.Messages.Routing;
using Quest.Lib.ServiceBus;
using Quest.WebCore.Services;
using System;
using System.IO;
using System.Text;
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

        [HttpGet]
        public async Task<ActionResult> GetVehicleCoverage(string code)
        {
            var builder = new StringBuilder();
            var writer = new StringWriter(builder);
            var ser = new JsonSerializer();
            GetCoverageResponse map;
            try
            {
                GetCoverageRequest request = new GetCoverageRequest { Code = code };

                map = await _messageCache.SendAndWaitAsync<GetCoverageResponse>(request, new TimeSpan(0, 0, 30));

                if (map == null)
                    map = new GetCoverageResponse() { Message = "no vehicle coverage available", Success = false };

                var js = JsonConvert.SerializeObject(map);
                var result = new ContentResult
                {
                    Content = js,
                    ContentType = "application/json"
                };

                return result;
            }
            catch (Exception)
            {
                var errorMsg = "ERROR: Cannot get coverage";
                ser.Serialize(writer, errorMsg);
                var error = new ContentResult
                {
                    Content = builder.ToString(),
                    ContentType = "application/json"
                };

                return error;
            }
        }

        [HttpPost]
        public async Task<AssignToDestinationResponse> AssignResource(AssignToDestinationRequest request)
        {
            var result = await _messageCache.SendAndWaitAsync<AssignToDestinationResponse>(request, new TimeSpan(0, 0, 10));
            return result;
        }

        [HttpGet]
        public async Task<SearchResponse> InfoSearch(InfoSearchRequest request)
        {
            var result = await _messageCache.SendAndWaitAsync<SearchResponse>(request, new TimeSpan(0, 0, 10));
            return result;
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