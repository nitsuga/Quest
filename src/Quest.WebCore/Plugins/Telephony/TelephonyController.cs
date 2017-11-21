using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Quest.Lib.ServiceBus;
using Quest.WebCore.Services;
using System;

namespace Quest.WebCore.Plugins.Telephony
{
    public class TelephonyController : Controller
    {
        private AsyncMessageCache _messageCache;
        private readonly IPluginService _pluginService;
        private readonly TelephonyPlugin _plugin;

        public TelephonyController(
                TelephonyPlugin plugin,
                AsyncMessageCache messageCache,
                IPluginService pluginService
            )
        {
            _plugin = plugin;
            _pluginService = pluginService;
            _messageCache = messageCache;
        }

        [HttpGet]
        public TelephonySettings GetSettings()
        {
            return _plugin.GetSettings();
        }

        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public ActionResult SubmitCLI(string cli, string extension)
        {
            _plugin.sendTestCli(cli, extension, _messageCache);
            var js = JsonConvert.SerializeObject(true);
            var result = new ContentResult
            {
                Content = js,
                ContentType = "application/json"
            };

            return result;
        }



    }

    [Serializable]
    public class TelephonySettings
    {
        public string MapServer;
        public double Zoom;
        public double Latitude;
        public double Longitude;
    }
}